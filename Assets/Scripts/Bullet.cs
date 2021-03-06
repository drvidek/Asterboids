using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector2 direction;
    public float spd;
    public float scale;
    public float power = 1f;
    public Color color;
    [SerializeField] private SpriteRenderer _myRenderer;
    [SerializeField] private GameObject _deathPart;
    [SerializeField] private ParticleSystem _trailPartSys;

    virtual protected void Start()
    {
        _myRenderer = GetComponent<SpriteRenderer>();
        _trailPartSys = GetComponentInChildren<ParticleSystem>();
        color = tag == "Player" ? Color.white : Color.red;
        GameManager.bullets.Add(this);
        ApplyProperties();
    }

    virtual public void ApplyProperties()
    {
        transform.localScale = new Vector3(scale, scale, 1);
        _myRenderer.color = color;
        if (tag == "Player")
            _trailPartSys.Stop();
    }

    protected void Update()
    {
        if (GameManager.IsPlaying())
        {
            Move();
            transform.Rotate(new Vector3(0, 0, 360) * Time.deltaTime);
            if (Vector2.Distance(transform.position, Vector2.zero) > 200f)
                StartCoroutine(EndOfLife());
        }
    }

    protected virtual void Move()
    {
        Vector3 _dest = transform.position + ((Vector3)direction.normalized * spd * Time.deltaTime);

        float _dist = Vector2.Distance(transform.position, _dest);

        if (tag == "Player")
        {
            RaycastHit2D[] _rayHit;
            _rayHit = Physics2D.CircleCastAll(transform.position, scale, direction, _dist);
            if (_rayHit.Length > 0)
            {
                foreach (RaycastHit2D item in _rayHit)
                {
                    if (Collision(item.collider))
                    {
                        _dest = item.point;
                        continue;
                    }
                }
            }
        }
        else
        {
            Collider2D[] _rayHit;
            _rayHit = Physics2D.OverlapCircleAll(transform.position, scale / 2f);
            if (_rayHit.Length > 0)
            {
                foreach (Collider2D item in _rayHit)
                {
                    Collision(item);
                }
            }
        }

        transform.position = _dest;
    }

    virtual protected bool Collision(Collider2D _hit)
    {
        if (_hit.tag != tag && _myRenderer.isVisible)
        {
            CombatAgent _hitAgent = _hit.GetComponent<CombatAgent>();
            Bullet _hitBullet = _hit.GetComponent<Bullet>();
            if (_hitAgent != null)
            {
                _hitAgent.TakeDamage(power);
                if (!_hitAgent.GetComponent<PlayerMain>())
                {
                    GlobalScore.IncreaseScore(10, (Vector2)_hitAgent.transform.position);
                    GlobalScore.IncreaseComboMeter(0.2f);
                }
            }
            else
            if (_hitBullet != null)
            {
                StartCoroutine(_hitBullet.EndOfLife());
                GlobalScore.IncreaseScore(20, (Vector2)_hitBullet.transform.position);
                GlobalScore.IncreaseComboMeter(0.2f);
            }
            StartCoroutine(EndOfLife());
            return true;
        }
        else
            return false;
    }

    public IEnumerator EndOfLife()
    {
        if (tag != "Player")
        {
            GameObject _partOb = Instantiate(_deathPart);
            _partOb.transform.position = transform.position;
            ParticleSystem _partSys = _partOb.GetComponent<ParticleSystem>();
            var _main = _partSys.main;
            _main.startColor = _myRenderer.color;
        }
        GameManager.bullets.Remove(this);
        Destroy(this.gameObject);
        yield return null;
    }
}
