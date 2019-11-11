using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action<int> OnTilesDestroyed;
    public event Action OnFinishDestroyed;

    private LevelData level;
    [SerializeField] private Tile tilePrefab;

    private Tile[,] board;

    private Tile firstTile;
    private Tile secondTile;

    public IEnumerator CreateBoard(LevelData levelData, Action finishCallback)
    {
        InputManager.IsActive = false;

        yield return StartCoroutine(ClearBoard());

        level = levelData;

        InputManager.OnTapDown += OnTapDown;
        InputManager.OnSwipe += DetectSwipe;

        board = new Tile[level.Rows, level.Columns];

        Vector2 tileSize = tilePrefab.transform.lossyScale;
        float newBoardPositionX = -((tileSize.x / 2)) * (level.Columns - 1);
        float newBoardPositionY = ((tileSize.y / 2)) * (level.Rows - 1);
        transform.position = new Vector2(newBoardPositionX, newBoardPositionY);

        Vector2 startPos = transform.position;

        yield return StartCoroutine(FillinBoardByData(level));
        yield return StartCoroutine(FillInBoardAndDestroyMatches());
        while (!CheckIfHasTurn())
        {
            yield return StartCoroutine(ShuffleBoard());
            yield return StartCoroutine(FillInBoardAndDestroyMatches());
        }

        InputManager.IsActive = true;

        finishCallback?.Invoke();
    }

    private IEnumerator ClearBoard()
    {
        if (board == null || board.Length <= 0)
        {
            yield return null;
        }
        else
        {
            InputManager.OnTapDown -= OnTapDown;
            InputManager.OnSwipe -= DetectSwipe;

            for (int row = 0; row < level.Rows; row++)
            {
                for (int column = 0; column < level.Columns; column++)
                {
                    if (board[row, column] == null)
                    {
                        continue;
                    }

                    if (row == level.Rows - 1 && column == level.Columns - 1)
                    {
                        yield return StartCoroutine(board[row, column].DisappearTile());
                    }
                    else
                    {
                        StartCoroutine(board[row, column].DisappearTile());
                    }

                    board[row, column] = null;
                }
            }
        }
    }

    private IEnumerator CreateTile(int row, int column, TileData data)
    {
        Vector2 tileSize = tilePrefab.transform.lossyScale;
        Tile tile = Instantiate(tilePrefab, transform);
        tile.Init(data.ID, data.sprite, data.color);
        tile.ChangePosition(0, column, true);
        board[row, column] = tile;
        yield return StartCoroutine(tile.AppearTile());
        tile.ChangePosition(row, column, false);
    }

    private void OnTapDown(Vector2 position)
    {
        if (firstTile != null)
        {
            firstTile.Deselect();
        }

        firstTile = GetTileByPosition(position);

        if (firstTile != null)
        {
            firstTile.Select();
        }
    }

    private Tile GetTileByPosition(Vector2 position)
    {
        Tile tile = null;
        for (int row = 0; row < level.Rows; row++)
        {
            for (int column = 0; column < level.Columns; column++)
            {
                Tile tmpTile = board[row, column];
                if (position.x < tmpTile.transform.position.x + (tmpTile.transform.localScale.x / 2) && position.x > tmpTile.transform.position.x - (tmpTile.transform.localScale.x / 2) &&
                    position.y < tmpTile.transform.position.y + (tmpTile.transform.localScale.y / 2) && position.y > tmpTile.transform.position.y - (tmpTile.transform.localScale.y / 2))
                {
                    tile = tmpTile;
                    break;
                }
            }

            if (tile != null)
            {
                break;
            }
        }

        return tile;
    }

    private void DetectSwipe(InputManager.SwipeDirection swipeDirection)
    {
        if (firstTile == null)
        {
            return;
        }

        firstTile.Deselect();

        secondTile = null;
        switch (swipeDirection)
        {
            case InputManager.SwipeDirection.Up:
                if (firstTile.Row - 1 >= 0)
                {
                    secondTile = board[firstTile.Row - 1, firstTile.Column];
                }
                break;
            case InputManager.SwipeDirection.Right:
                if (firstTile.Column + 1 < level.Columns)
                {
                    secondTile = board[firstTile.Row, firstTile.Column + 1];
                }
                break;
            case InputManager.SwipeDirection.Down:
                if (firstTile.Row + 1 < level.Rows)
                {
                    secondTile = board[firstTile.Row + 1, firstTile.Column];
                }
                break;
            case InputManager.SwipeDirection.Left:
                if (firstTile.Column - 1 >= 0)
                {
                    secondTile = board[firstTile.Row, firstTile.Column - 1];
                }
                break;
            case InputManager.SwipeDirection.None:
                secondTile = null;
                break;
        }

        if (secondTile != null)
        {
            StartCoroutine(StartSwapingAndFindMatches(firstTile, secondTile));
        }
    }

    private IEnumerator StartSwapingAndFindMatches(Tile first, Tile second)
    {
        InputManager.IsActive = false;
        yield return StartCoroutine(SwapTiles(first, second));

        Tile[] firstMatches = FindMatches(first);
        Tile[] secondMatches = FindMatches(second);

        if (firstMatches.Length <= 0 && secondMatches.Length <= 0)
        {
            yield return StartCoroutine(SwapTiles(firstTile, secondTile));
        }
        else
        {
            yield return StartCoroutine(FillInBoardAndDestroyMatches());
            while (!CheckIfHasTurn())
            {
                yield return StartCoroutine(ShuffleBoard());
                yield return StartCoroutine(FillInBoardAndDestroyMatches());
            }
        }
        OnFinishDestroyed?.Invoke();
        InputManager.IsActive = true;
    }

    private IEnumerator FillInBoardAndDestroyMatches()
    {
        List<Tile> allMatches = new List<Tile>();
        while (FindAllMatches(out allMatches))
        {
            yield return StartCoroutine(DestroyMatches(allMatches.ToArray()));
            yield return new WaitForSeconds(GameSettings.Instance.PauseAfterDestroyMatches);
            yield return StartCoroutine(FillInBoardRandom());
        }
    }

    private IEnumerator FillinBoardByData(LevelData levelData)
    {
        for (int column = level.Columns - 1; column >= 0; column--)
        {
            StartCoroutine(FillInColumnByData(column, levelData));
        }

        while (activeFillinColumnsCoroutines > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(GameSettings.Instance.PauseAfterFillInBoard);
    }

    private IEnumerator FillInColumnByData(int column, LevelData levelData)
    {
        activeFillinColumnsCoroutines++;
        for (int row = levelData.Rows - 1; row >= 0; row--)
        {
            yield return StartCoroutine(CreateTile(row, column, TilesData.Instance.GetTileByID(levelData.Board[row, column])));
            yield return new WaitForSeconds(GameSettings.Instance.FillInBoardInterval);
        }
        activeFillinColumnsCoroutines--;
    }

    private IEnumerator FillInBoardRandom()
    {
        for (int column = level.Columns - 1; column >= 0; column--)
        {
            StartCoroutine(FillInColumnRandom(column));
        }

        while (activeFillinColumnsCoroutines > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(GameSettings.Instance.PauseAfterFillInBoard);
    }

    private int activeFillinColumnsCoroutines = 0;
    private IEnumerator FillInColumnRandom(int column)
    {
        activeFillinColumnsCoroutines++;
        int newTilesCount = 0;
        for (int row = level.Rows - 1; row >= 0; row--)
        {
            if (board[row, column] == null)
            {
                newTilesCount++;
            }
        }

        for (int row = level.Rows - 1; row >= 0; row--)
        {
            if (board[row, column] == null)
            {
                yield return new WaitForSeconds(GameSettings.Instance.FillInBoardInterval);

                if (row == 0)
                {
                    break;
                }

                int offset = 1;
                Tile upTile = board[row - offset, column];
                while (upTile == null)
                {
                    offset++;
                    if (row - offset < 0)
                    {
                        break;
                    }
                    else
                    {
                        upTile = board[row - offset, column];
                    }
                }

                if (upTile != null)
                {
                    upTile.ChangePosition(row, column, false);
                    board[row - offset, column] = null;
                    board[row, column] = upTile;
                }
            }
        }

        for (int i = newTilesCount; i >= 1; i--)
        {
            yield return StartCoroutine(CreateTile(i - 1, column, TilesData.Instance.GetRandomTile));
            yield return new WaitForSeconds(GameSettings.Instance.FillInBoardInterval);
        }
        activeFillinColumnsCoroutines--;
    }

    private IEnumerator SwapTiles(Tile firstTile, Tile secondTile)
    {
        int tmpRow = firstTile.Row;
        int tmpColumn = firstTile.Column;

        firstTile.ChangeOrder(1);
        secondTile.ChangeOrder(0);

        firstTile.ChangePosition(secondTile.Row, secondTile.Column, false);
        secondTile.ChangePosition(tmpRow, tmpColumn, false);

        board[tmpRow, tmpColumn] = secondTile;
        board[firstTile.Row, firstTile.Column] = firstTile;

        yield return new WaitForSeconds(GameSettings.Instance.PauseAfterSwap);
    }

    private bool FindAllMatches(out List<Tile> allMatches)
    {
        bool success = false;
        allMatches = new List<Tile>();
        for (int row = 0; row < level.Rows; row++)
        {
            for (int column = 0; column < level.Columns; column++)
            {
                Tile[] mathces = FindMatches(board[row, column]);
                if (mathces.Length > 0)
                {
                    allMatches.AddRange(mathces);
                    success = true;
                }
            }
        }

        return success;
    }

    private Tile[] FindMatches(Tile tile)
    {
        List<Tile> totalMatches = new List<Tile>();
        List<Tile> horizontalMatches = new List<Tile>();
        List<Tile> verticalMatches = new List<Tile>();

        if (tile == null)
        {
            return totalMatches.ToArray();
        }

        totalMatches.Add(tile);
        horizontalMatches.Add(tile);
        verticalMatches.Add(tile);

        for (int column = tile.Column - 1; column >= 0; column--)
        {
            if (board[tile.Row, column] != null && board[tile.Row, column].ID == tile.ID)
            {
                horizontalMatches.Add(board[tile.Row, column]);
            }
            else
            {
                break;
            }
        }

        for (int column = tile.Column + 1; column < level.Columns; column++)
        {
            if (board[tile.Row, column] != null && board[tile.Row, column].ID == tile.ID)
            {
                horizontalMatches.Add(board[tile.Row, column]);
            }
            else
            {
                break;
            }
        }

        if (horizontalMatches.Count < 3)
        {
            horizontalMatches.Clear();
        }
        else
        {
            if (horizontalMatches.Count == 4)
            {
                for (int column = tile.Column - 1; column >= 0; column--)
                {
                    if (horizontalMatches.Contains(board[tile.Row, column]))
                    {
                        continue;
                    }

                    if (board[tile.Row, column] != null)
                    {
                        horizontalMatches.Add(board[tile.Row, column]);
                    }
                }

                for (int column = tile.Column + 1; column < level.Columns; column++)
                {
                    if (horizontalMatches.Contains(board[tile.Row, column]))
                    {
                        continue;
                    }

                    if (board[tile.Row, column] != null)
                    {
                        horizontalMatches.Add(board[tile.Row, column]);
                    }
                }
            }
            else if (horizontalMatches.Count == 5)
            {
                for (int row = 0; row < level.Rows; row++)
                {
                    for (int column = 0; column < level.Columns; column++)
                    {
                        if (horizontalMatches.Contains(board[row, column]))
                        {
                            continue;
                        }

                        if (board[row, column] != null && board[row, column].ID == tile.ID)
                        {
                            horizontalMatches.Add(board[row, column]);
                        }
                    }
                }
            }
        }
        horizontalMatches.Remove(tile);


        for (int row = tile.Row - 1; row >= 0; row--)
        {
            if (board[row, tile.Column] != null && board[row, tile.Column].ID == tile.ID)
            {
                verticalMatches.Add(board[row, tile.Column]);
            }
            else
            {
                break;
            }
        }

        for (int row = tile.Row + 1; row < level.Rows; row++)
        {
            if (board[row, tile.Column] != null && board[row, tile.Column].ID == tile.ID)
            {
                verticalMatches.Add(board[row, tile.Column]);
            }
            else
            {
                break;
            }
        }

        if (verticalMatches.Count < 3)
        {
            verticalMatches.Clear();
        }
        else
        {
            if (verticalMatches.Count == 4)
            {
                for (int row = tile.Row - 1; row >= 0; row--)
                {
                    if (verticalMatches.Contains(board[row, tile.Column]))
                    {
                        continue;
                    }

                    if (board[row, tile.Column] != null)
                    {
                        verticalMatches.Add(board[row, tile.Column]);
                    }
                }

                for (int row = tile.Row + 1; row < level.Rows; row++)
                {
                    if (verticalMatches.Contains(board[row, tile.Column]))
                    {
                        continue;
                    }

                    if (board[row, tile.Column] != null)
                    {
                        verticalMatches.Add(board[row, tile.Column]);
                    }
                }
            }
            else if (verticalMatches.Count == 5)
            {
                for (int row = 0; row < level.Rows; row++)
                {
                    for (int column = 0; column < level.Columns; column++)
                    {
                        if (verticalMatches.Contains(board[row, column]))
                        {
                            continue;
                        }

                        if (board[row, column] != null && board[row, column].ID == tile.ID)
                        {
                            verticalMatches.Add(board[row, column]);
                        }
                    }
                }
            }
        }
        verticalMatches.Remove(tile);


        totalMatches.AddRange(horizontalMatches);
        totalMatches.AddRange(verticalMatches);

        if (totalMatches.Count < 3)
        {
            totalMatches.Clear();
        }

        return totalMatches.ToArray();
    }

    private IEnumerator DestroyMatches(Tile[] matches)
    {
        int totalDestroyedTiles = 0;
        for (int i = 0; i < matches.Length; i++)
        {
            if (matches[i] == null)
            {
                continue;
            }

            if (board[matches[i].Row, matches[i].Column] != null)
            {
                totalDestroyedTiles++;
            }

            board[matches[i].Row, matches[i].Column] = null;

            if (i == matches.Length - 1)
            {
                yield return StartCoroutine(matches[i].DisappearTile());
            }
            else
            {
                StartCoroutine(matches[i].DisappearTile());
            }
        }

        if (totalDestroyedTiles > 0)
        {
            OnTilesDestroyed?.Invoke(totalDestroyedTiles);
        }
    }

    private IEnumerator ShuffleBoard()
    {
        System.Random random = new System.Random();
        for (int row = 0; row < level.Rows; row++)
        {
            for (int column = 0; column < level.Columns; column++)
            {
                int randomRowIndex = random.Next(row + 1);
                int randomRColumnIndex = random.Next(column + 1);

                Tile currentTile = board[row, column];
                Tile randomTile = board[randomRowIndex, randomRColumnIndex];

                if (row == level.Rows - 1 && column == level.Columns - 1)
                {
                    yield return StartCoroutine(SwapTiles(currentTile, randomTile));
                }
                else
                {
                    StartCoroutine(SwapTiles(currentTile, randomTile));
                }

            }
        }

        yield return new WaitForSeconds(GameSettings.Instance.PauseAfterShuffle);
    }

    private bool CheckIfHasTurn()
    {
        for (int row = 0; row < level.Rows; row++)
        {
            for (int column = 0; column < level.Columns; column++)
            {
                Tile currentTile = board[row, column];

                if ((row - 1 >= 0 && column + 1 < level.Columns) && currentTile.ID == board[row - 1, column + 1].ID)
                {
                    if((row - 1 >= 0 && column + 2 < level.Columns) && currentTile.ID == board[row - 1, column + 2].ID)
                    {
                        /* 
                         * ^^^^^^^^^
                         * ^^^^^^^^^
                         * ^^#$^^^^^
                         * ^#^^^^^^^
                         * ^^^^^^^^^
                         */

                        return true;
                    }
                    else if ((row - 2 >= 0 && column + 1 < level.Columns) && currentTile.ID == board[row - 2, column + 1].ID)
                    {
                        /* 
                         * ^^^^^^^^^
                         * ^^$^^^^^^
                         * ^^#^^^^^^
                         * ^#^^^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                    else if ((row + 1 < level.Rows && column + 1 < level.Columns) && currentTile.ID == board[row + 1, column + 1].ID)
                    {
                        /* 
                         * ^^^^^^^^^
                         * ^^^^^^^^^
                         * ^^#^^^^^^
                         * ^#^^^^^^^
                         * ^^$^^^^^^
                         */
                        return true;
                    }
                }
                else if ((row + 1 < level.Rows && column + 1 < level.Columns) && currentTile.ID == board[row + 1, column + 1].ID)
                {
                    if ((row + 1 < level.Rows && column + 2 < level.Columns) && currentTile.ID == board[row + 1, column + 2].ID)
                    {
                        /* 
                         * ^^^^^^^^^
                         * ^^#^^^^^^
                         * ^^^#$^^^^
                         * ^^^^^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                    else if ((row + 2 < level.Rows && column + 1 < level.Columns) && currentTile.ID == board[row + 2, column + 1].ID)
                    {
                        /* 
                         * ^^^^^^^^^
                         * ^^#^^^^^^
                         * ^^^#^^^^^
                         * ^^^$^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                    else if ((row - 1 >= 0 && column + 1 < level.Columns) && currentTile.ID == board[row - 1, column + 1].ID)
                    {
                        /* 
                         * ^^^$^^^^^
                         * ^^#^^^^^^
                         * ^^^#^^^^^
                         * ^^^^^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                }
                else if ((row + 1 < level.Rows && column - 1 >= 0) && currentTile.ID == board[row + 1, column - 1].ID)
                {
                    if ((row + 1 < level.Rows && column - 2 >= 0) && currentTile.ID == board[row + 1, column - 2].ID)
                    {
                        /* 
                         * ^^^^^^^^^
                         * ^^#^^^^^^
                         * $#^^^^^^^
                         * ^^^^^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                    else if ((row + 2 < level.Rows && column - 1 >= 0) && currentTile.ID == board[row + 2, column - 1].ID)
                    {
                        /* 
                         * ^^^^^^^^^
                         * ^^#^^^^^^
                         * ^#^^^^^^^
                         * ^$^^^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                    else if ((row - 1 >= 0 && column - 1 >= 0) && currentTile.ID == board[row - 1, column - 1].ID)
                    {
                        /* 
                         * ^$^^^^^^^
                         * ^^#^^^^^^
                         * ^#^^^^^^^
                         * ^^^^^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                }
                else if ((row - 1 >= 0 && column - 1 >= 0) && currentTile.ID == board[row - 1, column - 1].ID)
                {
                    if ((row - 1 >= 0 && column - 2 >= 0) && currentTile.ID == board[row - 1, column - 2].ID)
                    {
                        /* 
                         * ^^^^^^^^^
                         * $#^^^^^^^
                         * ^^#^^^^^^
                         * ^^^^^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                    else if ((row - 2 >= 0 && column - 1 >= 0) && currentTile.ID == board[row - 2, column - 1].ID)
                    {
                        /* 
                         * ^$^^^^^^^
                         * ^#^^^^^^^
                         * ^^#^^^^^^
                         * ^^^^^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                    else if ((row - 1 >= 0 && column + 1 < level.Columns) && currentTile.ID == board[row - 1, column + 1].ID)
                    {
                        /* 
                         * ^^^^^^^^^
                         * ^#^$^^^^^
                         * ^^#^^^^^^
                         * ^^^^^^^^^
                         * ^^^^^^^^^
                         */
                        return true;
                    }
                }
                else if (column + 2 < level.Columns && currentTile.ID == board[row, column + 2].ID && column + 3 < level.Columns && currentTile.ID == board[row, column + 3].ID)
                {
                    /* 
                     * ^^^^^^^^^
                     * ^^^^^^^^^
                     * ^^^#^##^^
                     * ^^^^^^^^^
                     * ^^^^^^^^^
                     */
                    return true;
                }
                else if (column - 2 >= 0 && currentTile.ID == board[row, column - 2].ID && column - 3 >= 0 && currentTile.ID == board[row, column - 3].ID)
                {
                    /* 
                     * ^^^^^^^^^
                     * ^^^^^^^^^
                     * ##^#^^^^^
                     * ^^^^^^^^^
                     * ^^^^^^^^^
                     */
                    return true;
                }
                else if (row + 2 < level.Rows && currentTile.ID == board[row + 2, column].ID && row + 3 < level.Rows && currentTile.ID == board[row + 3, column].ID)
                {
                    /* 
                     * ^^^^^^^^^
                     * ^^^#^^^^^
                     * ^^^^^^^^^
                     * ^^^#^^^^^
                     * ^^^#^^^^^
                     * ^^^^^^^^^
                     */
                    return true;
                }
                else if (row - 2 >= 0 && currentTile.ID == board[row - 2, column].ID && row - 3 >= 0 && currentTile.ID == board[row - 3, column].ID)
                {
                    /* 
                     * ^^^#^^^^^
                     * ^^^#^^^^^
                     * ^^^^^^^^^
                     * ^^^#^^^^^
                     * ^^^^^^^^^
                     * ^^^^^^^^^
                     */
                    return true;
                }
            }
        }
        return false;
    }
}
