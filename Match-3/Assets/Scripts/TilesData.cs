using System.Linq;
using UnityEngine;

[System.Serializable]
public struct TileData
{
    public int ID;
    public Sprite sprite;
    public Color color;
}

[CreateAssetMenu(fileName = "TilesData", menuName = "Settings/Tiles Data", order = 51)]
public class TilesData : ScriptableObject
{
    [SerializeField] private TileData[] tiles;
    public TileData[] GetAllTiles => tiles;

    private static TilesData data;
    public static TilesData Instance
    {
        get
        {
            if (data == null)
            {
                data = Resources.Load("Settings/TilesData", typeof(TilesData)) as TilesData;
            }
            return data;
        }
    }

    public TileData GetRandomTile
    {
        get
        {
            int tileIndex = Random.Range(0, tiles.Length - 1);
            return tiles[tileIndex];
        }
    }

    public TileData GetTileByID(int id)
    {
        return tiles.ToList().Find(x => x.ID == id);
    }
}
