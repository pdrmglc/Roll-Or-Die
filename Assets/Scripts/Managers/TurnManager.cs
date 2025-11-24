using UnityEngine;
using TMPro;

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

    [Header("Managers")]
    public CombatManager combatManager;

    public float fadeSpeed = 0.5f;

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
        // Reseta movimentos/ataques do jogador atual
        ActivePlayer.ResetUnits();

        // Próximo player
        activePlayerIndex = (activePlayerIndex + 1) % players.Length;

        // Novo turno completo quando volta ao player 0
        if (activePlayerIndex == 0)
        {
            turn++;
            turnDisplay.text = $"Turn: {turn}";
        }

        // Atualiza banner
        ShowTurnBanner();

        // Avise o CombatManager para atualizar grid + highlights
        combatManager.RefreshCombat(ActivePlayer);
    }

    public void ShowTurnBanner()
    {
        bannerText.text = $"{ActivePlayer.playerName}'s Turn";
        turnBanner.alpha = 1;
    }
}
