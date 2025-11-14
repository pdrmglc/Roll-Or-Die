using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class GridManager : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 20;
    public int height = 20;
    public GameObject tilePrefab;
    public bool useTilemapGenerator = false;
    private Tile[,] map;
    private bool gridGenerated = false;
    public List<GameObject> spawnedTiles = new List<GameObject>();

    
    void Awake()
    {
        map = new Tile[width, height];
        CheckForTilemapGenerator();
    }
    
    void Start()
    {
        if (!gridGenerated)
        {
            CheckForTilemapGenerator();
        }
    }

    private void CheckForTilemapGenerator()
    {
        TilemapGridGenerator tilemapGenerator = FindAnyObjectByType<TilemapGridGenerator>();
        bool shouldUseGenerator = useTilemapGenerator || (tilemapGenerator != null);

        if (tilemapGenerator != null && !useTilemapGenerator)
        {
            useTilemapGenerator = true;
        }

        if (!shouldUseGenerator && !gridGenerated)
        {
            GenerateGrid();
            gridGenerated = true;
        }
    }
    
    public void DestroyGrid()
    {
        foreach (GameObject tile in spawnedTiles)
        {
            if (tile != null)
                GameObject.Destroy(tile);
        }

        spawnedTiles.Clear();
    }


    public void GenerateGrid()
    {
        if (gridGenerated)
        {
            return;
        }
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 tilePosition = new Vector3(
                    x - (width / 2f) + 0.5f,
                    y - (height / 2f) + 0.5f,
                    0
                );
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.name = $"Tile {x}, {y}";
                tile.transform.SetParent(transform);

                Tile tileScript = tile.GetComponent<Tile>();
                tileScript.gridPosition = new Vector2Int(x, y);
                tileScript.originalColor = (x + y) % 2 == 0 ? Color.white : Color.gray;
                map[x, y] = tileScript;
            }
        }
        
        gridGenerated = true;
    }

    public Tile GetTile(Vector2Int position)
    {
        if (useTilemapGenerator || !gridGenerated)
        {
            Tile[] allTiles = GetComponentsInChildren<Tile>();
            foreach (Tile tile in allTiles)
            {
                if (tile != null && tile.gridPosition == position)
                {
                    return tile;
                }
            }
            return null;
        }
        else
        {
            if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
            {
                return null;
            }
            return map[position.x, position.y];
        }
    }

    public Tile GetTile(Vector3 position)
    {
        if (useTilemapGenerator || !gridGenerated)
        {
            Tile[] allTiles = GetComponentsInChildren<Tile>();
            Tile closestTile = null;
            float closestDistance = float.MaxValue;
            
            foreach (Tile tile in allTiles)
            {
                if (tile != null)
                {
                    float distance = Vector3.Distance(tile.transform.position, position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTile = tile;
                    }
                }
            }
            return closestTile;
        }
        else
        {
            float relX = position.x + (width / 2f) - 0.5f;
            float relY = position.y + (height / 2f) - 0.5f;

            int x = Mathf.RoundToInt(relX);
            int y = Mathf.RoundToInt(relY);

            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return null;
            }

            return map[x, y];
        }
    }

    public int GetHeuristic(Tile start, Tile end)
    {
        int dx = Mathf.Abs(start.gridPosition.x - end.gridPosition.x);
        int dy = Mathf.Abs(start.gridPosition.y - end.gridPosition.y);
        return dx + dy;
    }

    private List<Tile> RetracePath(Tile start, Tile end)
    {
        List<Tile> path = new List<Tile>();
        Tile current = end;
        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }
    
    public List<Tile> GetPath(Tile start, Tile end)
    {
        List<Tile> open = new List<Tile>();
        HashSet<Tile> closed = new HashSet<Tile>();

        open.Add(start);
        start.gCost = 0;
        start.hCost = GetHeuristic(start, end);
        start.parent = null;

        while (open.Count > 0)
        {
            Tile current = open.OrderBy(t => t.fCost).ThenBy(t => t.hCost).First();

            if (current == end)
            {
                return RetracePath(start, end);
            }
            open.Remove(current);
            closed.Add(current);

            foreach (Tile neighbor in GetTileNeighbors(current.gridPosition))
            {
                if (closed.Contains(neighbor) || neighbor.isOccupied)
                    continue;
                int tempG = current.gCost + neighbor.moveCost;

                if (!open.Contains(neighbor) || tempG < neighbor.gCost)
                {
                    neighbor.gCost = tempG;
                    neighbor.hCost = GetHeuristic(neighbor, end);
                    neighbor.parent = current;
                    if (!open.Contains(neighbor))
                        open.Add(neighbor);
                }
            }
        }
        return null;
    }

    public List<Tile> GetTileNeighbors(Vector2Int tilePosition, bool includeDiagonals = true)
    {
        List<Tile> neighbors = new List<Tile>();

        if (useTilemapGenerator || !gridGenerated)
        {
            Tile[] allTiles = GetComponentsInChildren<Tile>();
            Tile centerTile = null;
            
            foreach (Tile tile in allTiles)
            {
                if (tile != null && tile.gridPosition == tilePosition)
                {
                    centerTile = tile;
                    break;
                }
            }
            
            if (centerTile == null) return neighbors;
            
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    
                    Vector2Int neighborPos = new Vector2Int(tilePosition.x + x, tilePosition.y + y);
                    
                    foreach (Tile tile in allTiles)
                    {
                        if (tile != null && tile.gridPosition == neighborPos)
                        {
                            if (!includeDiagonals && IsDiagonal(centerTile, tile))
                                continue;
                            
                            neighbors.Add(tile);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int posX = tilePosition.x + x;
                    int posY = tilePosition.y + y;

                    if (posX < 0 || posY < 0 || posX >= width || posY >= height)
                        continue;

                    Tile current = map[posX, posY];
                    if (current == null) continue;

                    if (current.gridPosition == tilePosition)
                        continue;

                    if (!includeDiagonals && IsDiagonal(map[tilePosition.x, tilePosition.y], current))
                        continue;
                    
                    neighbors.Add(current);
                }
            }
        }
        return neighbors;
    }

    public bool IsDiagonal(Tile a, Tile b)
    {
        int dx = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
        int dy = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);

        return dx == 1 && dy == 1;

    }

    public void ResetGridHighlights()
    {
        if (useTilemapGenerator || !gridGenerated)
        {
            // Busca todos os tiles filhos do GridManager
            Tile[] allTiles = GetComponentsInChildren<Tile>();
            foreach (Tile tile in allTiles)
            {
                if (tile != null && Tile.selectedTile != tile)
                {
                    tile.inMoveRange = false;
                    tile.inAttackRange = false;
                    tile.ChangeColor(tile.originalColor);
                }
            }
        }
        else
        {
            foreach (Tile tile in map)
            {
                if (tile != null && Tile.selectedTile != tile)
                {
                    tile.inMoveRange = false;
                    tile.inAttackRange = false;
                    tile.ChangeColor(tile.originalColor);
                }
            }
        }
    }

    public void ClearTileOccupations()
    {
        if (useTilemapGenerator || !gridGenerated)
        {
            Tile[] allTiles = GetComponentsInChildren<Tile>();
            foreach (Tile tile in allTiles)
            {
                if (tile != null)
                {
                    tile.isOccupied = false;
                }
            }
        }
        else
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile tile = map[x, y];
                    if (tile != null)
                    {
                        tile.isOccupied = false;
                    }
                }
            }
        }
    }


    public List<Tile> GetHighlightRange(Vector2Int start, int moveRange, int attackRange)
    {
        Tile tileStart = GetTile(start);
        if (tileStart == null)
        {
            return new List<Tile>();
        }
        
        if (moveRange == int.MaxValue)
        {
            List<Tile> allTiles = new List<Tile>();
            if (useTilemapGenerator || !gridGenerated)
            {
                Tile[] allTilesArray = GetComponentsInChildren<Tile>();
                foreach (Tile tile in allTilesArray)
                {
                    if (tile != null && !tile.isOccupied)
                    {
                        allTiles.Add(tile);
                        tile.inMoveRange = true;
                    }
                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (map[x, y] != null && !map[x, y].isOccupied)
                        {
                            allTiles.Add(map[x, y]);
                            map[x, y].inMoveRange = true;
                        }
                    }
                }
            }
            return allTiles;
        }
        ResetGridHighlights();
        List<Tile> moveTiles = new List<Tile>();
        Dictionary<Tile, int> costSoFar = new Dictionary<Tile, int>();
        Queue<Tile> edge = new Queue<Tile>();

        edge.Enqueue(tileStart);
        costSoFar[tileStart] = 0;

        while (edge.Count > 0)
        {
            Tile current = edge.Dequeue();
            int currentCost = costSoFar[current];

            foreach (Tile neighbor in GetTileNeighbors(current.gridPosition))
            {
                int stepCost = IsDiagonal(current, neighbor) ? 1 + neighbor.moveCost : neighbor.moveCost;
                int newCost = currentCost + stepCost;

                if (newCost <= moveRange && (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]) && !neighbor.isOccupied)
                {
                    costSoFar[neighbor] = newCost;
                    edge.Enqueue(neighbor);

                    if (!moveTiles.Contains(neighbor) && neighbor != tileStart)
                    {
                        moveTiles.Add(neighbor);
                        neighbor.inMoveRange = true;
                    }

                }
            }
        }

        HashSet<Tile> attackTiles = new HashSet<Tile>();
        foreach (Tile origin in moveTiles.Concat(new List<Tile> { tileStart }))
        {
            Queue<(Tile tile, int distance)> attackQueue = new Queue<(Tile, int)>();
            attackQueue.Enqueue((origin, 0));
            HashSet<Tile> visited = new HashSet<Tile> { origin };

            while (attackQueue.Count > 0)
            {
                var (tile, distance) = attackQueue.Dequeue();

                foreach (Tile neighbor in GetTileNeighbors(tile.gridPosition, false))
                {
                    if (!visited.Contains(neighbor) && distance + 1 <= attackRange)
                    {
                        visited.Add(neighbor);
                        attackQueue.Enqueue((neighbor, distance + 1));
                        attackTiles.Add(neighbor);
                        neighbor.inAttackRange = true;
                    }
                    {
                        
                    }
                }
            }
        }

        return moveTiles.Concat(attackTiles).Distinct().ToList();
    }

    public void HighlightRange(Tile start, int moveRange, int attackRange)
    {
        List<Tile> reachableTiles = GetHighlightRange(start.gridPosition, moveRange, attackRange);

        foreach (Tile tile in reachableTiles)
        {
            if (Unit.Selected && Unit.Selected.inCombatMode)
            {
                if (tile.inMoveRange)
                {
                    tile.ChangeColor(Color.cyan);
                }
                else if (tile.inAttackRange)
                {
                    tile.ChangeColor(Color.red);
                }
            }
            else
            {
                // Fora do modo combate, não pinta os tiles
                tile.ChangeColor(tile.originalColor);
            }
        }
    }
}
