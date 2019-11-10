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
    [SerializeField] private float spawnTileInterval = 1.0f;
    public float SpawnTileInterval => spawnTileInterval;
    [SerializeField] private float pauseAfterDestroyMatches = 1.0f;
    public float PauseAfterDestroyMatches => pauseAfterDestroyMatches;
    [SerializeField] private float pauseAfterFillInBoard = 1.0f;
    public float PauseAfterFillInBoard => pauseAfterFillInBoard;
    [SerializeField] private float pauseAfterSwap = 1.0f;
    public float PauseAfterSwap => pauseAfterSwap;
    [SerializeField] private float pauseAfterShuffle = 1.0f;
    public float PauseAfterShuffle => pauseAfterShuffle;
    [SerializeField] private float fillInBoardInterval = 1.0f;
    public float FillInBoardInterval => fillInBoardInterval;
}
