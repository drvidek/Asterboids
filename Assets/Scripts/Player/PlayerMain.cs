using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMain : CombatAgent
{
    [SerializeField] private float _spdMax = 40f, _spdCurrent, _spdAccelCurrent, _spdAccel, _fric = 1f, _turnSpd = 200f, _recoil = 2f;
    private bool _shotFired;
    Collider2D _myCollider;
    public bool ShotFired { set { _shotFired = value; } }
    [SerializeField] private Vector2 _myVelocity;
    public Vector2 Velocity { get { return _myVelocity; } set { _myVelocity = value ; } }
    float _boostDelay;
    public float BoostDelay { get { return _boostDelay; } set { _boostDelay = value; } }
    [SerializeField] private float _boostDelayMax = 3f, _boostRate = 1.3f, _boostDuration = 0.5f;
    bool _boostActive;

    bool _comboReset;

    [SerializeField] private GameObject _boostField;
    [SerializeField] private Image _boostImage;
    [SerializeField] private ParticleSystem _pingPartSys;
    [SerializeField] private ParticleSystem _thrustPartSys;

    new void Start()
    {
        base.Start();
        HandleKeybindFile.ReadSaveFile();
        _myCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.IsPaused())
        {
            _boostDelay = MathExt.Approach(_boostDelay, 0, Time.deltaTime);

            if (_boostActive && _boostDelay == 0)
            {
                BoostToggle(false);
            }
            else if (BoostActivated())
            {
                BoostToggle(true);
            }

            if (!_boostActive)
            {
                transform.Rotate(Turn());
                _myVelocity += Thrust();
                _myVelocity += Recoil();

            }

            if (_dead)
            {
                _myVelocity = Vector2.zero;
                _spriteRenderer.enabled = false;
                _myCollider.enabled = false;
            }

            Move((Vector3)_myVelocity * Time.deltaTime);

            if (!_boostActive)
            {
                _myVelocity.x = Mathf.Clamp(_myVelocity.x, -_spdMax, _spdMax);
                _myVelocity.y = Mathf.Clamp(_myVelocity.y, -_spdMax, _spdMax);
            }

            if (!ThrustButton() || _boostActive)
            {
                _myVelocity.x = MathExt.Approach(_myVelocity.x, 0, _fric * Time.deltaTime);
                _myVelocity.y = MathExt.Approach(_myVelocity.y, 0, _fric * Time.deltaTime);
            }

            ScreenWrap();
        }
        UpdateBoostGUI();
    }

    #region Inputs + Movement
    Vector3 Turn()
    {
        float _rotZ = (Input.GetKey(KeyBinds.keys["Left"])) ? 1 : (Input.GetKey(KeyBinds.keys["Right"])) ? -1 : 0;
        return new Vector3(0, 0, _rotZ) * _turnSpd * Time.deltaTime;
    }

    bool ThrustButton()
    {
        return (Input.GetKey(KeyBinds.keys["Forward"]));
    }

    bool BoostActivated()
    {
        return (Input.GetKeyDown(KeyBinds.keys["Boost"]) && _boostDelay == 0 && !_dead);
    }

    void BoostToggle(bool active)
    {
        _boostActive = active;
        _boostDelay = active ? _boostDuration : _boostDelayMax;
        _boostField.SetActive(active);
        if (active)
            _myVelocity = transform.up * _spdMax * _boostRate;
    }

    Vector2 Thrust()
    {
        if (ThrustButton())
        {
            _spdAccelCurrent += _spdAccel * Time.deltaTime;
            _spdCurrent = MathExt.Approach(_spdCurrent, _spdMax, _spdAccelCurrent * Time.deltaTime);
            if (_thrustPartSys.isStopped)
                _thrustPartSys.Play();
            return (Vector2)transform.up * _spdCurrent * Time.deltaTime;

        }
        else
        {
            _spdCurrent = 0f;
            _spdAccelCurrent = 0f;
            if (_thrustPartSys.isPlaying)
                _thrustPartSys.Stop();
            return Vector2.zero;
        }
    }

    Vector2 Recoil()
    {
        if (_shotFired)
        {
            _shotFired = false;
            return transform.up * -_recoil;
        }
        else
            return Vector2.zero;
    }

    void Move(Vector3 velocity)
    {
        Vector3 _dest = transform.position + velocity;

        if (!_boostActive && !_dead)
        {
            RaycastHit2D[] _rayHits = Physics2D.RaycastAll(transform.position, velocity, Vector3.Distance(transform.position, _dest));
            if (_rayHits.Length > 0)
            {
                bool hit = false;
                foreach (RaycastHit2D rayHit in _rayHits)
                {
                    if (rayHit.collider.tag != this.tag)
                    {
                        hit = true;
                        Debug.Log(rayHit.collider.name);
                        break;
                    }
                }
                if (hit)
                {
                    StartCoroutine(EndOfLife());
                }
            }
        }
        transform.position = _dest;
    }

    protected override void ScreenWrap()
    {
        base.ScreenWrap();

        if (_isWrappingX || _isWrappingY)
        {
            if (!_comboReset)
            {
                _pingPartSys.Play();
                GlobalScore.ResetCombo();
                _comboReset = true;
            }
        }
        else
            _comboReset = false;
    } 
    #endregion

    protected override IEnumerator EndOfLife()
    {
        _dead = true;

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject item in bullets)
        {
            if (item.GetComponent<Bullet>())
            {
                Destroy(item);
            }
        }

        ParticleSystem partSys = CreateDeathParticles();
        partSys.gameObject.transform.localScale = partSys.gameObject.transform.localScale * 3;
        yield return new WaitForSeconds(partSys.main.duration * 2f);
        GameManager.ChangeScene(0);
        yield return null;
    }

    public void UpdateBoostGUI()
    {
        _boostImage.fillAmount = _dead ? 0 : _boostDelay == 0 ? 0 : 1 - _boostDelay / _boostDelayMax;

    }

}
