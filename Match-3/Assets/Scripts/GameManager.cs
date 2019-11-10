using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private BoardController boardController;
    [SerializeField] private UIController uiController;

    private int comboFactor;

    private LevelData currentLevel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        uiController.SetSelectLevelButtonVisible(false);
        uiController.SetScoreVisible(false);
        uiController.InitLevelsScreen(LevelsData.GetLevels());
        uiController.SetLevelsContainerVisible(true);
    }

    public void LoadLevel(int levelIndex)
    {
        if (currentLevel != null)
        {
            ScoreController.OnScoreChanged -= uiController.SetScore;
            ScoreController.OnScoreReseted -= uiController.ResetScore;
            boardController.OnTilesDestroyed -= OnTilesDestroyed;
            boardController.OnFinishDestroyed -= OnFinishDestroyed;
        }

        uiController.SetScoreVisible(false);
        uiController.SetSelectLevelButtonVisible(false);
        uiController.SetLevelsContainerVisible(false);

        comboFactor = 0;
        LevelData levelData = LevelsData.GetLevelByIndex(levelIndex);

        if (levelData == null)
        {
            Debug.LogError($"Ошибка при загрузке {levelIndex} уровня! \nНевозможно продолжить игру.");
            return;
        }

        currentLevel = levelData;

        StartCoroutine(boardController.CreateBoard(levelData, OnBoardCreated));
    }

    private void OnBoardCreated()
    {
        ScoreController.OnScoreChanged += uiController.SetScore;
        ScoreController.OnScoreReseted += uiController.ResetScore;
        ScoreController.Reset();
        uiController.SetScoreVisible(true);
        uiController.SetSelectLevelButtonVisible(true);

        boardController.OnTilesDestroyed += OnTilesDestroyed;
        boardController.OnFinishDestroyed += OnFinishDestroyed;
    }

    public void OnTilesDestroyed(int count)
    {
        comboFactor++;
        ScoreController.AddScore(GameSettings.Instance.ScorePerTile * count * comboFactor);
        if (comboFactor > 1)
        {
            uiController.SetCombo(comboFactor);
        }
    }

    public void OnFinishDestroyed()
    {
        comboFactor = 0;
    }
}
