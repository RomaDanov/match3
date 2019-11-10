using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private ScoreView scoreView;
    [SerializeField] private Transform levelsContainer;
    [SerializeField] private LevelButton levelButtonPrefab;
    [SerializeField] private GameObject selectLevelsButton;

    public void SetScoreVisible(bool isVisible)
    {
        scoreView.gameObject.SetActive(isVisible);
    }

    public void SetScore(int score)
    {
        scoreView.SetScore(score);
    }

    public void ResetScore()
    {
        scoreView.SetScore(0);
    }

    public void SetCombo(int combo)
    {
        scoreView.SetCombo(combo);
    }

    public void SetSelectLevelButtonVisible(bool isVisible)
    {
        selectLevelsButton.gameObject.SetActive(isVisible);
    }

    public void SetLevelsContainerVisible(bool isVisible)
    {
        InputManager.IsActive = !isVisible;
        levelsContainer.gameObject.SetActive(isVisible);
    }

    public void OnSetActiveLevelScreen()
    {
        if (!levelsContainer.gameObject.activeSelf)
        {
            InputManager.IsActive = false;
            levelsContainer.gameObject.SetActive(true);
        }
        else
        {
            InputManager.IsActive = true;
            levelsContainer.gameObject.SetActive(false);
        }
    }

    public void InitLevelsScreen(LevelData[] levels)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            LevelButton levelButton = Instantiate(levelButtonPrefab, levelsContainer);
            levelButton.OnLevelSelected += GameManager.Instance.LoadLevel;
            levelButton.SetLevel(levels[i].Level);
        }
    }
}
