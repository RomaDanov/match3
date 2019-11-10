using UnityEngine;
using UnityEngine.UI;

public class ScoreView : MonoBehaviour
{
    [SerializeField] private Text totalScore;
    [SerializeField] private Text comboText;
    [SerializeField] private Animation scoreAnimation;
    [SerializeField] private Animation comboAnimation;

    private const string ScoreAnimationName = "ScoreAnimation";
    private const string ComboAnimationName = "ComboAnimation";

    public void SetScore(int score)
    {
        scoreAnimation.Stop(ScoreAnimationName);
        totalScore.text = score.ToString();
        scoreAnimation.Play(ScoreAnimationName);
    }

    public void SetCombo(int comboFactor)
    {
        comboAnimation.Stop(ComboAnimationName);
        comboText.text = $"x{comboFactor}";
        comboAnimation.Play(ComboAnimationName);
    }
}
