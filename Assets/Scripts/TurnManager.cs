using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public Player[] players;
    public int activePlayerIndex = 0;
    public int turn = 1;

    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI bannerText;
    public CanvasGroup turnBanner;

    public GridManager gridManager;

    public float fadeSpeed = 0.5f;

    public CanvasGroup endTurnButton; // Adicione esta linha
    public CanvasGroup turnUI; // Adicione esta linha

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
        
        turnDisplay.text = $"Turn: {turn}";
        
        // Esconde UI de turno inicialmente
        endTurnButton.alpha = 0;
        endTurnButton.interactable = false;
        turnUI.alpha = 0;
        turnBanner.alpha = 0; // Garante que o banner começa invisível
    }

    // Update is called once per frame
    void Update()
    {
        if(turnBanner.alpha > 0)
        {
            turnBanner.alpha -= fadeSpeed * Time.deltaTime;
        }
    }

    // Ends the current player's turn, resets their units, and moves to the next player
    public void EndTurn()
    {
        players[activePlayerIndex].ResetUnits();
        activePlayerIndex = (activePlayerIndex + 1) % players.Length;

        if (activePlayerIndex == 0)
        {
            turn++;
            turnDisplay.text = $"Turn: {turn}";
        }

        bannerText.text = $"{players[activePlayerIndex].playerName}'s Turn";
        turnBanner.alpha = 1;
        gridManager.ResetGridHighlights();
    }

    // Adicione este método
    public void SetCombatMode(bool enabled)
    {
        endTurnButton.alpha = enabled ? 1 : 0;
        endTurnButton.interactable = enabled;
        turnUI.alpha = enabled ? 1 : 0;
    }

    public void ShowTurnBanner()
    {
        bannerText.text = $"{players[activePlayerIndex].playerName}'s Turn";
        turnBanner.alpha = 1;
    }
}