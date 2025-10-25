using UnityEngine;
using UnityEngine.Tilemaps;

public class GridControl : MonoBehaviour
{
    private Tilemap targetTilemap;
    [SerializeField] private GridManager gridManager;


    private void Awake() {

        targetTilemap = GameObject.FindGameObjectWithTag("HighlightMap").GetComponent<Tilemap>();
        gridManager = GameObject.FindGameObjectWithTag("WorldTileMap").GetComponent<GridManager>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseInput();
        }

    }

    private void MouseInput()
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int clickPosition = targetTilemap.WorldToCell(worldPoint);
        gridManager.Set(clickPosition.x, clickPosition.y, 1);

    }
}
