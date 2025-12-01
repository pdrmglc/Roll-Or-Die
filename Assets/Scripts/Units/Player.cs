using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public string playerName;
    public Unit selectedUnit;
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
            // Se TurnManager não foi inicializado, procura na cena
            if (TurnManager.instance == null)
            {
                TurnManager tm = FindAnyObjectByType<TurnManager>();
                if (tm == null)
                {
                    Debug.LogError("TurnManager não encontrado na cena!");
                    return null;
                }
            }

            // Retorna o jogador ativo de forma segura
            if (TurnManager.instance.players == null || 
                TurnManager.instance.activePlayerIndex < 0 || 
                TurnManager.instance.activePlayerIndex >= TurnManager.instance.players.Length)
            {
                Debug.LogError("Índice de jogador inválido!");
                return null;
            }

            return TurnManager.instance.players[TurnManager.instance.activePlayerIndex];
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
            if (TurnManager.instance != null)
            {
                TurnManager.instance.SetCombatMode(true);
                TurnManager.instance.ShowTurnBanner();
            }
        }
        else
        {
            // Saiu do combate → deixa turno sem seleção
            if (TurnManager.instance != null)
            {
                TurnManager.instance.SetCombatMode(false);
            }
            selectedUnit = null;
        }
    }


    public void ChangeSelectUnit(Unit unit)
    {
        // Atualiza seleção local
        selectedUnit = unit;

        // Use o singleton em vez da referência
        if (TurnManager.instance != null)
        {
            TurnManager.instance.OnUnitSelected(this, unit);
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
