using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public string playerName;
    public Unit selectedUnit;
    public TurnManager turnManager;

    public List<Unit> playerUnits;

    private bool inCombatMode = false;

    void Start()
    {
        foreach (Unit u in playerUnits)
            {
                u.owner = this;
            }
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
        // Alterna o modo
        inCombatMode = !inCombatMode;

        // Atualiza TODAS as Units do Player
        foreach (Unit unit in playerUnits)
        {
            unit.SetCombatMode(inCombatMode);

            // Habilita/desabilita PlayerMovement (movimento livre)
            PlayerMovement pm = unit.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.canMove = !inCombatMode;
        }

        // Garante que existe uma selectedUnit quando entrar no combate
        if (inCombatMode)
        {
            if (selectedUnit == null && playerUnits.Count > 0)
                selectedUnit = playerUnits[0];

            // Informa ao TurnManager que entrou no combate
            turnManager.SetCombatMode(true);
            turnManager.ShowTurnBanner();
        }
        else
        {
            // Saiu do combate → deixa turno sem seleção
            turnManager.SetCombatMode(false);
            selectedUnit = null;
        }
    }


    public void ChangeSelectUnit(Unit unit)
    {
        // Atualiza seleção local
        selectedUnit = unit;

        // Delega o trabalho de destacar ao TurnManager (que controla o grid)
        if (turnManager != null)
        {
            turnManager.OnUnitSelected(this, unit);
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
