using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(GridScript))]
public class GridManager : MonoBehaviour
{
    private Tilemap tilemap;
    private GridScript gridScript;
    [SerializeField] private TileSet tileSet;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        gridScript = GetComponent<GridScript>();
        gridScript.Init(8, 5);
        Set(2, 2, 1);
        Set(3, 2, 2);
        Set(5, 2, 3);
        UpdateTileMap();
    }

    void UpdateTileMap()
    {
        for (int x = 0; x < gridScript.GetLength(); x++)
        {
            for (int y = 0; y < gridScript.GetHeight(); y++)
                {
                    UpdateTileMap(x, y);
                }
            }
        }
    private void UpdateTileMap(int x, int y)
    {
        int tileId = gridScript.Get(x, y);
        if (tileId == -1)
        {
            return; // Invalid tile index
        }
            tilemap.SetTile(new Vector3Int(x, y, 0), tileSet.tiles[tileId]);
    }

    public void Set(int x, int y, int to)
    {
        gridScript.Set(x, y, to);
        UpdateTileMap();
    }

}
