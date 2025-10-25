using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PathNode
{
    public int xPos;
    public int yPos;
    public int gValue;
    public int hValue;
    public PathNode parentNode;

    public int fValue
    {
        get { return gValue + hValue; }
    }

    public PathNode(int xPos, int yPos)
    {
        this.xPos = xPos;
        this.yPos = yPos;
    }
}

[RequireComponent(typeof(GridScript))]
public class Pathfinding : MonoBehaviour
{

    GridScript gridMap;
    PathNode[,] pathNodes;
    void Start()
    {
        Init();
    }
    private void Init()
    {
        if (gridMap == null)
        {
            gridMap = GetComponent<GridScript>();
        }

        pathNodes = new PathNode[gridMap.GetLength(), gridMap.GetHeight()];

        for (int x = 0; x < gridMap.GetLength(); x++)
        {
            for (int y = 0; y < gridMap.GetHeight(); y++)
            {
                pathNodes[x, y] = new PathNode(x, y);
            }
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        Debug.Log($"Tentando acessar ({endX}, {endY}) dentro do grid [{pathNodes.GetLength(0)} x {pathNodes.GetLength(1)}]");

        PathNode startNode = pathNodes[startX, startY];
        PathNode targetNode = pathNodes[endX, endY];

        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();


        openList.Add(startNode);

        while (openList.Count > 0)
        {

            PathNode currentNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if ((openList[i].fValue < currentNode.fValue) ||
                    (openList[i].fValue == currentNode.fValue && openList[i].hValue < currentNode.hValue))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            List<PathNode> neighbourNodes = new List<PathNode>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    int checkX = currentNode.xPos + x;
                    int checkY = currentNode.yPos + y;

                    if (gridMap.CheckPosition(checkX, checkY) == false)
                    {
                        continue;
                    }

                    neighbourNodes.Add(pathNodes[checkX, checkY]);

                }

                for (int i = 0; i < neighbourNodes.Count; i++)
                {
                    PathNode neighbourNode = neighbourNodes[i];

                    if (closedList.Contains(neighbourNode))
                    {
                        continue;
                    }

                    if (gridMap.CheckWalkable(neighbourNode.xPos, neighbourNode.yPos) == false)
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int gCost = currentNode.gValue + CalculateDistance(currentNode, neighbourNode);

                    if (gCost < neighbourNode.gValue || !openList.Contains(neighbourNode))
                    {
                        neighbourNode.gValue = gCost;
                        neighbourNode.hValue = CalculateDistance(neighbourNode, targetNode);
                        neighbourNode.parentNode = currentNode;

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }

        }
        return null;

    }
    List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Reverse();
        return path;
    }
    
    int CalculateDistance(PathNode current, PathNode target)
    {
        int dstX = Mathf.Abs(current.xPos - target.xPos);
        int dstY = Mathf.Abs(current.yPos - target.yPos);

        // Se puder andar na diagonal:
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}