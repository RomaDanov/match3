using System;
using UnityEngine;

public static class ScoreController
{
    public static event Action<int> OnScoreChanged;
    public static event Action OnScoreReseted;

    public static int TotalScore { get; private set; }

    public static void AddScore(int score)
    {
        TotalScore += score;
        OnScoreChanged?.Invoke(TotalScore);
    }

    public static void Reset()
    {
        TotalScore = 0;
        OnScoreReseted?.Invoke();
    }
}
