using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class LevelData
{
    public int Level;
    public int Rows;
    public int Columns;
    public int[,] Board;

    public LevelData(int level = 0, int rows = 0, int columns = 0, int[,] board = null)
    {
        Level = level;
        Rows = rows;
        Columns = columns;
        Board = board;
    }
}

public static class LevelsData
{
    public const string LevelsFolderPath = "Levels";

    private static List<LevelData> levels = new List<LevelData>();

    public static LevelData GetLevelByIndex(int index)
    {
        LevelData cashedLevel = levels.Find(x => x.Level == index);
        if (cashedLevel != null)
        {
            return cashedLevel;
        }

        TextAsset file = Resources.Load<TextAsset>(LevelsFolderPath + "/level_" + index);
        string[] readedLines = file.text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (readedLines.Length <= 0)
        {
            return null;
        }

        LevelData levelData = new LevelData();

        bool error = false;
        for (int i = 0; i < readedLines.Length; i++)
        {
            for (int j = 0; j < readedLines[i].Length; j++)
            {
                int resultInt = -1;
                int.TryParse(readedLines[i][j].ToString(), out resultInt);
                if (resultInt == -1)
                {
                    error = true;
                }
            }
        }

        if (error)
        {
            return null;
        }

        levelData.Level = index;
        levelData.Rows = readedLines.Length;
        levelData.Columns = readedLines[0].Length;
        levelData.Board = new int[levelData.Rows, levelData.Columns];

        for (int row = 0; row < levelData.Rows; row++)
        {
            for (int column = 0; column < levelData.Columns; column++)
            {
                levelData.Board[row, column] = int.Parse(readedLines[row][column].ToString());
            }
        }
        levels.Add(levelData);
        return levelData;
    }

    public static LevelData[] GetLevels()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>(LevelsFolderPath);

        if (levels.Count == files.Length)
        {
            return levels.ToArray();
        }

        for (int i = 1; i <= files.Length; i++)
        {
            LevelData levelData = GetLevelByIndex(i);
            if (!levels.Contains(levelData))
            {
                levels.Add(levelData);
            }
        }

        return levels.ToArray();
    }
}
