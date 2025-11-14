using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public GridManager gridManager;

    public Tilemap groundTilemap;      // Tilemap usado para gerar o grid
    public Tilemap collisionTilemap;   // Tilemap usado para marcar ocupação

    public void GenerateGridFromTilemap()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("TilemapGridGenerator: Ground Tilemap não atribuído!");
            return;
        }

        if (collisionTilemap == null)
        {
            Debug.LogWarning("TilemapGridGenerator: Collision Tilemap não atribuído!");
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

        gridManager.spawnedTiles.Clear();

        BoundsInt bounds = groundTilemap.cellBounds;

        int minX = bounds.xMin;
        int minY = bounds.yMin;

        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase tile = groundTilemap.GetTile(pos);

            if (tile != null)
            {
                Vector3 worldPos = groundTilemap.CellToWorld(pos) + groundTilemap.cellSize / 2;
                GameObject spawned = Instantiate(tilePrefab, worldPos, Quaternion.identity);

                if (gridManager != null)
                    spawned.transform.SetParent(gridManager.transform);
                else
                    spawned.transform.SetParent(transform);

                Tile tileScript = spawned.GetComponent<Tile>();

                if (tileScript != null)
                {
                    Vector2Int gridPos = new Vector2Int(pos.x - minX, pos.y - minY);
                    tileScript.gridPosition = gridPos;

                    // 🔥 CHECANDO COLISÃO
                    if (collisionTilemap != null)
                    {
                        TileBase collisionTile = collisionTilemap.GetTile(pos);

                        if (collisionTile != null)
                        {
                            tileScript.isOccupied = true;
                            spawned.name += " [BLOCKED]";
                        }
                    }
                }

                spawned.name = $"Tile {pos} [Grid: {tileScript?.gridPosition}]";

                gridManager.spawnedTiles.Add(spawned);
            }
        }
    }
}
