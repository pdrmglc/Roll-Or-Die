using UnityEngine;

public class GridScript : MonoBehaviour
{
    private int length;
    private int height;

    int[,] grid;

    public void Init(int length, int height)
    {
        grid = new int[length, height];
        this.length = length;
        this.height = height;
    }

    public void Set(int x, int y, int to)
    {
        if (CheckPosition(x, y) == false)
        {
            Debug.LogError("GridScript Set Position out of bounds: " + x + "," + y);
            return;
        }
        grid[x, y] = to;
    }

    public int Get(int x, int y)
    {
        if (CheckPosition(x, y) == false)
        {
            Debug.LogError("GridScript Get Position out of bounds: " + x + "," + y);
            return -1;
        }
        return grid[x, y];
    }

    public bool CheckPosition(int x, int y)
    {
        if (x < 0 || y < 0 || x >= length || y >= height)
        {
            return false;
        }
        return true;
    }

    public int GetLength()
    {
        return length;
    }
    public int GetHeight()
    {
        return height;
    }
}
