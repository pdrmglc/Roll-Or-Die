using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public GridManager gridManager;

    void Start()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError("TilemapGridGenerator: Nenhum Tilemap encontrado nesse GameObject!");
            return;
        }

        if (tilePrefab == null)
        {
            Debug.LogError("TilemapGridGenerator: tilePrefab não atribuído!");
            return;
        }

        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }

        BoundsInt bounds = tilemap.cellBounds;
        int minX = bounds.xMin;
        int minY = bounds.yMin;

        int count = 0;
        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                Vector3 worldPos = tilemap.CellToWorld(pos) + tilemap.cellSize / 2;
                GameObject spawned = Instantiate(tilePrefab, worldPos, Quaternion.identity);

                if (gridManager != null)
                {
                    spawned.transform.SetParent(gridManager.transform);
                }
                else
                {
                    spawned.transform.SetParent(transform);
                }

                Tile tileScript = spawned.GetComponent<Tile>();
                if (tileScript != null)
                {
                    Vector2Int gridPos = new Vector2Int(pos.x - minX, pos.y - minY);
                    tileScript.gridPosition = gridPos;
                    tileScript.originalColor = (gridPos.x + gridPos.y) % 2 == 0 ? Color.white : Color.gray;
                }

                spawned.name = $"Tile ({pos.x},{pos.y}) [Grid: {tileScript?.gridPosition}]";
                count++;
            }
        }

        if (gridManager == null)
        {
            Debug.LogWarning("TilemapGridGenerator: GridManager não encontrado!");
        }
    }
}

