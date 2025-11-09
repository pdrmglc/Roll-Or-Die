using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public string playerName;
    public static Unit selectedUnit;
    public GridManager gridManager;
    public TurnManager turnManager;

    public List<Unit> playerUnits;

    private bool inCombatMode = false;

    void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
        SnapUnits();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCombatMode();
        }
    }
    
    private void ToggleCombatMode()
    {
        inCombatMode = !inCombatMode;

        // Primeiro atualiza as unidades
        foreach (Unit unit in playerUnits)
        {
            unit.SetCombatMode(inCombatMode);
        }

        if (inCombatMode)
        {
            // Primeiro mostra a UI
            turnManager.SetCombatMode(true);
            // Depois mostra o banner inicial
            turnManager.ShowTurnBanner();
        }
        else
        {
            // Se sair do modo combate, esconde tudo
            turnManager.SetCombatMode(false);
        }

        // Por último atualiza os highlights
        if (selectedUnit != null)
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
            unit.gridPosition = unitTile.gridPosition;
            unit.transform.position = unitTile.transform.position;
            unitTile.isOccupied = true;
            Debug.Log($"Unidade {unit.name} posicionada na tile {unitTile.gridPosition}.");
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
