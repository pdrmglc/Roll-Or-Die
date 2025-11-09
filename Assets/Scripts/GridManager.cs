using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class GridManager : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 20;
    public int height = 20;
    public GameObject tilePrefab;
    private Tile[,] map;
    void Awake()
    {
        map = new Tile[width, height];
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 tilePosition = new Vector3(x - width / 2, y - height / 2, 0);
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.name = $"Tile {x}, {y}";
                tile.transform.SetParent(transform);

                Tile tileScript = tile.GetComponent<Tile>();
                tileScript.gridPosition = new Vector2Int(x, y);
                tileScript.originalColor = (x + y) % 2 == 0 ? Color.white : Color.gray;
                map[x, y] = tileScript;
            }
        }
    }

    public Tile GetTile(Vector2Int position)
    {
        return map[position.x, position.y];
    }

    public Tile GetTile(Vector3 position)
    {
        // Corrige o cálculo de offset baseado no grid real
        float relX = position.x + width / 2f;
        float relY = position.y + height / 2f;

        int x = Mathf.RoundToInt(relX);
        int y = Mathf.RoundToInt(relY);

        // Verifica limites pra evitar NullReference
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogWarning($"GetTile: posição ({x},{y}) fora dos limites do mapa ({width},{height}) para posição mundial {position}");
            return null;
        }

        return map[x, y];
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

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int posX = tilePosition.x + x;
                int posY = tilePosition.y + y;

                if (posX < 0 || posY < 0 || posX >= width || posY >= height)
                    continue;

                Tile current = map[posX, posY];

                if (current.gridPosition == tilePosition)
                    continue;

                if (!includeDiagonals && IsDiagonal(map[tilePosition.x, tilePosition.y], current))
                    continue;
                
                neighbors.Add(current);
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
        foreach (Tile tile in map)
        {
            if (Tile.selectedTile != tile)
            {
                tile.inMoveRange = false;
                tile.inAttackRange = false;
                tile.ChangeColor(tile.originalColor);
            }
        }
    }

    public List<Tile> GetHighlightRange(Vector2Int start, int moveRange, int attackRange)
    {
        if (moveRange == int.MaxValue)
        {
            List<Tile> allTiles = new List<Tile>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!map[x, y].isOccupied)
                    {
                        allTiles.Add(map[x, y]);
                        map[x, y].inMoveRange = true;
                    }
                }
            }
            return allTiles;
        }
        ResetGridHighlights();
        List<Tile> moveTiles = new List<Tile>();
        Dictionary<Tile, int> costSoFar = new Dictionary<Tile, int>();
        Queue<Tile> edge = new Queue<Tile>();

        Tile tileStart = map[start.x, start.y];
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
            if (Player.selectedUnit && Player.selectedUnit.inCombatMode)
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
