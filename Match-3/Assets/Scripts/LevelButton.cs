using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public event Action<int> OnLevelSelected;

    [SerializeField] private Text lable;

    private int level = 0;

    public void SetLevel(int levelIndex)
    {
        level = levelIndex;
        lable.text = $"Level {level}";
    }

    public void OnClick()
    {
        OnLevelSelected?.Invoke(level);
    }
}
