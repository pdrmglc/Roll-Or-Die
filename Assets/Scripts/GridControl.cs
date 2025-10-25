using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridControl : MonoBehaviour
{
    private Tilemap targetTilemap;
    [SerializeField] private GridManager gridManager;
    Pathfinding pathfinding;

    int currentX = 0;
    int currentY = 0;
    int targetX = 0;
    int targetY = 0;

    [SerializeField] TileBase highlightTile;

    private void Awake()
    {

        targetTilemap = GameObject.FindGameObjectWithTag("HighlightMap").GetComponent<Tilemap>();
        gridManager = GameObject.FindGameObjectWithTag("WorldTileMap").GetComponent<GridManager>();
        pathfinding = gridManager.GetComponent<Pathfinding>();
    }
    // Update is called once per frame
    void Update()
    {
        MouseInput();
    }

    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickPosition = targetTilemap.WorldToCell(worldPoint);
            // gridManager.Set(clickPosition.x, clickPosition.y, 1);
            targetX = clickPosition.x;
            targetY = clickPosition.y;

            List<PathNode> path = pathfinding.FindPath(currentX, currentY, targetX, targetY);

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    targetTilemap.SetTile(new Vector3Int(path[i].xPos, path[i].yPos, 0), highlightTile);
                }
                currentX = targetX;
                currentY = targetY;

            }
        }
    }
}
