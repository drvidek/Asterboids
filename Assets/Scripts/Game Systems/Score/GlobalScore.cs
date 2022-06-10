using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalScore
{

    public static void IncreaseScore(int points, Vector2 position)
    {
        ScoreKeeper _sm = GameObject.Find("GameManager").GetComponent<ScoreKeeper>();
        _sm.IncreaseScore(points, position);
    }

    public static void ResetScore()
    {
        ScoreKeeper _sm = GameObject.Find("GameManager").GetComponent<ScoreKeeper>();
        _sm.ResetScore();
    }

    public static string GetScore()
    {
        ScoreKeeper _sm = GameObject.Find("GameManager").GetComponent<ScoreKeeper>();
        return _sm.Score.ToString();
    }
}