using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance { get; private set; }
    
    public GridManager gridManager;
    public TilemapGridGenerator tilemapGenerator;

    private Unit[] allUnits;
    private Player activePlayer;

    public bool InCombat { get; private set; } = false;

    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();

        if (tilemapGenerator == null)
            tilemapGenerator = FindAnyObjectByType<TilemapGridGenerator>();
    }

    // ============================================================
    //  Inicialização do combate
    // ============================================================

    public void InitializeCombat(Player[] players)
    {
        // Juntar todas as Units de todos os players
        List<Unit> all = new List<Unit>();

        foreach (var p in players)
            all.AddRange(p.playerUnits);

        allUnits = all.ToArray();
    }

    public void EnterCombat(Player active)
    {
        InCombat = true;
        activePlayer = active;

        if (tilemapGenerator == null || gridManager == null)
        {
            Debug.LogError("CombatManager: faltando GridManager ou TilemapGridGenerator!");
            return;
        }

        // Destruir grid antigo
        gridManager.DestroyGrid();

        // Garantir que o player tem uma unidade selecionada
        if (activePlayer.selectedUnit == null)
        {
            if (activePlayer.playerUnits.Count > 0)
                activePlayer.ChangeSelectUnit(activePlayer.playerUnits[0]);
            else
            {
                Debug.LogError("CombatManager: activePlayer não tem units!");
                return;
            }
        }

        Unit unit = activePlayer.selectedUnit;

        // Posicionar o grid ao redor da unidade
        tilemapGenerator.player = unit.transform;

        tilemapGenerator.GenerateGridFromTilemap(allUnits);

        // Colocar unidades nos tiles
        SnapUnits();

        // Recalcular ocupações
        gridManager.RecalculateTileOccupations(allUnits);

        // Highlights
        UpdateHighlights(unit);
    }

    public void ExitCombat()
    {
        InCombat = false;
        gridManager.DestroyGrid();
    }

    // ============================================================
    //  Recriação do grid quando muda o turno
    // ============================================================

    public void RefreshCombat(Player newActivePlayer)
    {
        if (!InCombat) return;

        activePlayer = newActivePlayer;

        Unit unit = newActivePlayer.selectedUnit;
        if (unit == null)
        {
            Debug.LogError("CombatManager: jogador não tem selectedUnit ao mudar turno");
            return;
        }

        gridManager.DestroyGrid();

        tilemapGenerator.player = unit.transform;

        tilemapGenerator.GenerateGridFromTilemap(allUnits);

        UpdateHighlights(unit);
    }

    // ============================================================
    //  Highlights / Seleção de Unidade
    // ============================================================

    public void OnUnitSelected(Player player, Unit unit)
    {
        if (!InCombat) return;
        if (player != activePlayer) return;

        UpdateHighlights(unit);
    }

    private void UpdateHighlights(Unit unit)
    {
        Tile tile = gridManager.GetTile(unit.gridPosition);
        if (tile == null) return;

        gridManager.ResetGridHighlights();

        gridManager.HighlightRange(
            tile,
            unit.movementLeft,
            unit.attackRange
        );
    }

    // ============================================================
    //  Snapping
    // ============================================================

    private void SnapUnits()
    {
        foreach (Unit unit in allUnits)
        {
            Tile tile = gridManager.GetTile(unit.transform.position);

            if (tile == null)
            {
                Debug.LogWarning($"CombatManager: Tile não encontrado para {unit.name}");
                continue;
            }

            unit.gridPosition = tile.gridPosition;
            unit.transform.position = tile.transform.position;

            tile.isOccupied = true;
        }
    }
}
