using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public string playerName;
    public Unit selectedUnit;
    public GridManager gridManager;
    public TurnManager turnManager;

    public List<Unit> playerUnits;

    private bool inCombatMode = false;

    void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCombatMode();
        }
    }
    public static Player ActivePlayer
    {
        get
        {
            TurnManager tm = FindAnyObjectByType<TurnManager>();
            return tm.players[tm.activePlayerIndex];
        }
    }
    
    private void ToggleCombatMode()
    {
        inCombatMode = !inCombatMode;

        // Primeiro atualiza as unidades
        foreach (Unit unit in playerUnits)
        {
            unit.SetCombatMode(inCombatMode);

            // 🔹 Ativa/desativa o script PlayerMovement conforme o modo
            PlayerMovement pm = unit.GetComponent<PlayerMovement>();
            if (pm != null)
                {
                    pm.canMove = !inCombatMode;
                }
        }

        if (inCombatMode)
        {
            gridManager.ResetGridHighlights();
            gridManager.ClearTileOccupations();
            // Ativa a movimentação por grid
            gridManager.DestroyGrid(); // limpar qualquer lixo antigo
            FindAnyObjectByType<TilemapGridGenerator>().GenerateGridFromTilemap();

            SnapUnits();

            // Primeiro mostra a UI
            turnManager.SetCombatMode(true);
            // Depois mostra o banner inicial
            turnManager.ShowTurnBanner();
        }
        else
        {
            // Ativa a movimentação livre
            // Desativa a movimentação por grid
            // Se sair do modo combate, esconde tudo
            gridManager.DestroyGrid(); // limpar qualquer lixo antigo
            turnManager.SetCombatMode(false);
            selectedUnit = null;
        }

        // Por último atualiza os highlights
        if (inCombatMode && selectedUnit != null)
        {
            gridManager.ResetGridHighlights();
            gridManager.HighlightRange(
                gridManager.GetTile(selectedUnit.gridPosition),
                inCombatMode ? selectedUnit.movementLeft : int.MaxValue,
                inCombatMode ? selectedUnit.attackRange : 0
            );
        }
    }

    public void ChangeSelectUnit(Unit unit)
    {
        selectedUnit = unit;
        gridManager.HighlightRange(gridManager.GetTile(unit.gridPosition),
        inCombatMode ? unit.movementLeft: int.MaxValue,
        inCombatMode ? unit.attackRange: 0);
    }

    private void SnapUnits()
    {
        foreach (Unit unit in playerUnits)
        {
            Tile unitTile = gridManager.GetTile(unit.transform.position);
            if (unitTile == null)
            {
                Debug.LogWarning($"SnapUnits: Não foi possível encontrar tile para unidade {unit.name} na posição {unit.transform.position}");
                continue;
            }
            unit.gridPosition = unitTile.gridPosition;
            unit.transform.position = unitTile.transform.position;
            unitTile.isOccupied = true;
        }
    }

    public void ResetUnits()
    {
        foreach (Unit unit in playerUnits)
        {
            unit.movementLeft = unit.movementRange;
        }
    }
}
