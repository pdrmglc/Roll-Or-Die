using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public GridManager gridManager;

    public Tilemap groundTilemap;
    public Tilemap collisionTilemap;

    public Transform player; // <<< transforma do Unit
    public int radius = 12;   // <<< raio em células

    public void GenerateGridFromTilemap()
    {
        if (player == null)
        {
            Debug.LogError("TilemapGridGenerator: Player não atribuído!");
            return;
        }

        if (groundTilemap == null)
        {
            Debug.LogError("TilemapGridGenerator: Ground Tilemap não atribuído!");
            return;
        }

        if (tilePrefab == null)
        {
            Debug.LogError("TilemapGridGenerator: tilePrefab não atribuído!");
            return;
        }

        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();

        gridManager.spawnedTiles.Clear();

        // 🔥 Converte posição do player para coordenada de célula no tilemap
        Vector3Int playerCell = groundTilemap.WorldToCell(player.position);

        // 🔥 Percorre apenas o quadrado em torno do player
        for (int x = playerCell.x - radius; x <= playerCell.x + radius; x++)
        {
            for (int y = playerCell.y - radius; y <= playerCell.y + radius; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                TileBase tile = groundTilemap.GetTile(pos);

                if (tile == null)
                    continue; // não existe tile → não gera grid

                // Converte para posição no mundo
                Vector3 worldPos = groundTilemap.CellToWorld(pos) + groundTilemap.cellSize / 2;

                GameObject spawned = Instantiate(tilePrefab, worldPos, Quaternion.identity);

                if (gridManager != null)
                    spawned.transform.SetParent(gridManager.transform);
                else
                    spawned.transform.SetParent(transform);

                Tile tileScript = spawned.GetComponent<Tile>();

                if (tileScript != null)
                {
                    // Converte a posição real do tilemap para uma posição interna do seu grid
                    tileScript.gridPosition = new Vector2Int(pos.x, pos.y);

                    // Checa ocupação
                    if (collisionTilemap != null)
                    {
                        if (collisionTilemap.GetTile(pos) != null)
                        {
                            tileScript.isOccupied = true;
                            spawned.name += " [BLOCKED]";
                        }
                    }
                }

                spawned.name = $"Tile ({pos.x},{pos.y})";

                gridManager.spawnedTiles.Add(spawned);
            }
        }
    }
}
