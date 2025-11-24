using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    [Header("Players")]
    public Player[] players;
    public int activePlayerIndex = 0;
    public int turn = 1;

    public Player ActivePlayer => players[activePlayerIndex];

    [Header("UI")]
    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI bannerText;
    public CanvasGroup turnBanner;
    public CanvasGroup endTurnButton;
    public CanvasGroup turnUI;
    public float fadeSpeed = 0.5f;

    [Header("Managers")]
    public CombatManager combatManager;

    [Header("Turn Order UI")]
    public Transform turnOrderPanel;       // Drag TurnOrderPanel here
    public GameObject turnOrderItemPrefab; // Drag the prefab here

    private List<GameObject> uiTurnItems = new List<GameObject>();


    [Header("Turn Order Logic")]
    public List<Unit> unitsInCombat = new List<Unit>();
    public List<Unit> turnOrder = new List<Unit>();
    public int turnOrderIndex = 0;

    private void Start()
    {
        if (combatManager == null)
            combatManager = FindAnyObjectByType<CombatManager>();

        if (combatManager == null)
        {
            Debug.LogError("TurnManager: CombatManager não encontrado na cena!");
            return;
        }

        // Inicializa combate com todas as Units
        combatManager.InitializeCombat(players);
        InitializeTurnOrder();

        // Configura UI
        turnDisplay.text = $"Turn: {turn}";
        endTurnButton.alpha = 0;
        endTurnButton.interactable = false;
        turnUI.alpha = 0;
        turnBanner.alpha = 0;
    }

    private void Update()
    {
        // Fade do banner
        if (turnBanner.alpha > 0)
            turnBanner.alpha -= fadeSpeed * Time.deltaTime;
    }

    // ===================================================================
    //  COMBAT MODE
    // ===================================================================

    public void SetCombatMode(bool enabled)
    {
        endTurnButton.alpha = enabled ? 1 : 0;
        endTurnButton.interactable = enabled;

        turnUI.alpha = enabled ? 1 : 0;

        if (enabled)
            combatManager.EnterCombat(ActivePlayer);
        else
            combatManager.ExitCombat();
    }

    // ===================================================================
    //  SELEÇÃO DE UNIDADE
    // ===================================================================

    public void OnUnitSelected(Player player, Unit unit)
    {
        // Apenas redireciona para o CombatManager
        combatManager.OnUnitSelected(player, unit);
    }

    // ===================================================================
    //  TURNOS
    // ===================================================================

    public void EndTurn()
    {
        // reseta unidades do jogador atual
        ActivePlayer.ResetUnits();

        // avança para a próxima unidade na ordem de turno
        turnOrderIndex = (turnOrderIndex + 1) % turnOrder.Count;

        // identifica qual unit é do próximo jogador
        Unit nextUnit = turnOrder[turnOrderIndex];

        // encontra o Player dono dessa unit
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].playerUnits.Contains(nextUnit))
            {
                activePlayerIndex = i;
                players[i].ChangeSelectUnit(nextUnit);
                break;
            }
        }

        // novo turno quando volta para o início da lista
        if (turnOrderIndex == 0)
        {
            turn++;
            turnDisplay.text = $"Turn: {turn}";
        }

        ShowTurnBanner();

        // Atualizar grid
        combatManager.RefreshCombat(ActivePlayer);

        // Atualizar HUD de ordem
        // --- ROTACIONAR LISTA VISUAL (modo FFT) ---
        List<Unit> rotated = new List<Unit>();

        for (int i = 0; i < turnOrder.Count; i++)
        {
            int idx = (turnOrderIndex + i) % turnOrder.Count;
            rotated.Add(turnOrder[idx]);
        }

        // Agora atualiza usando a lista rotacionada
        UpdateTurnOrderUI(rotated);
    }


    public void ShowTurnBanner()
    {
        bannerText.text = $"{ActivePlayer.playerName}'s Turn";
        turnBanner.alpha = 1;
    }

    // Call this whenever the turn order changes
    public void UpdateTurnOrderUI(List<Unit> unitsInOrder)
    {
        foreach (var item in uiTurnItems)
            Destroy(item);
        uiTurnItems.Clear();

        for (int i = 0; i < unitsInOrder.Count; i++)
        {
            Unit unit = unitsInOrder[i];
            GameObject obj = Instantiate(turnOrderItemPrefab, turnOrderPanel);
            obj.transform.localScale = Vector3.one;

            TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>();
            text.text = unit.unitName;

            // lista já está rotacionada, então o primeiro item é o ativo
            if (i == 0)
                text.color = Color.yellow;
            else
                text.color = Color.white;

            uiTurnItems.Add(obj);
        }
    }

    public void InitializeTurnOrder()
    {
        unitsInCombat.Clear();

        // juntar todas as units de todos os players
        foreach (var p in players)
            unitsInCombat.AddRange(p.playerUnits);

        // copiar para turnOrder
        turnOrder = new List<Unit>(unitsInCombat);

        // se não há atributo speed, simplesmente mantém a ordem original
        // (ou podemos embaralhar, se quiser)
        // turnOrder = turnOrder.OrderBy(u => Random.value).ToList(); // opcional

        turnOrderIndex = 0;

        // Atualiza HUD
        UpdateTurnOrderUI(turnOrder);
    }

}
