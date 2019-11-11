using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings", order = 51)]
public class GameSettings : ScriptableObject
{
    private static GameSettings data;
    public static GameSettings Instance
    {
        get
        {
            if (data == null)
            {
                data = Resources.Load("Settings/GameSettings", typeof(GameSettings)) as GameSettings;
            }
            return data;
        }
    }

    [SerializeField] private int scorePerTile = 100;
    public int ScorePerTile => scorePerTile;

    [Header("Animation Settings")]
    [SerializeField] private float pauseAfterDestroyMatches = 0.1f;
    public float PauseAfterDestroyMatches => pauseAfterDestroyMatches;
    [SerializeField] private float pauseAfterFillInBoard = 0.2f;
    public float PauseAfterFillInBoard => pauseAfterFillInBoard;
    [SerializeField] private float pauseAfterSwap = 0.1f;
    public float PauseAfterSwap => pauseAfterSwap;
    [SerializeField] private float pauseAfterShuffle = 0.5f;
    public float PauseAfterShuffle => pauseAfterShuffle;
    [SerializeField] private float fillInBoardInterval = 0.005f;
    public float FillInBoardInterval => fillInBoardInterval;
}
