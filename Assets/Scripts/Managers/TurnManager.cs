using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public Player[] players;
    private Unit[] allUnits;
    public int activePlayerIndex = 0;
    public int turn = 1;

    public TextMeshProUGUI turnDisplay;
    public TextMeshProUGUI bannerText;
    public CanvasGroup turnBanner;

    public GridManager gridManager;

    public TilemapGridGenerator tilemapGenerator;

    public float fadeSpeed = 0.5f;

    public CanvasGroup endTurnButton; // Adicione esta linha
    public CanvasGroup turnUI; // Adicione esta linha

    public Player ActivePlayer => players[activePlayerIndex];


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
        tilemapGenerator = FindAnyObjectByType<TilemapGridGenerator>();

        // Pega todos as units na cena e salva em allUnits
        List<Unit> temp = new List<Unit>();

        for (int i = 0; i < players.Length; i++)
        {
            foreach (Unit u in players[i].playerUnits)
            {
                temp.Add(u);
            }
        }

        allUnits = temp.ToArray();

        
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

    public void EnterCombatMode()
    {
        if (gridManager == null || tilemapGenerator == null)
        {
            Debug.LogError("TurnManager: GridManager ou TilemapGridGenerator não encontrados!");
            return;
        }

        // 🔥 Destrói grid antigo (se houver)
        gridManager.DestroyGrid();

            Player activePlayer = players[activePlayerIndex];

        if (activePlayer.selectedUnit == null)
        {
            // Pegue a primeira unidade desse jogador
            if (activePlayer.playerUnits.Count > 0)
            {
                activePlayer.ChangeSelectUnit(activePlayer.playerUnits[0]);
            }
            else
            {
                Debug.LogError("ActivePlayer não possui nenhuma unidade!");
                return;
            }
        }

        // 🔥 Seleciona a unidade ativa
        Unit activeUnit = ActivePlayer.selectedUnit;
        if (activeUnit == null)
        {
            Debug.LogError("TurnManager: ActivePlayer não possui selectedUnit!");
            return;
        }

        // 🔥 Diz ao gerador qual é o centro do grid
        tilemapGenerator.player = activeUnit.transform;

        // 🔥 Recria o grid em volta do activeUnit
        tilemapGenerator.GenerateGridFromTilemap(allUnits);

        // 🔥 Reposiciona as unidades nos tiles
        SnapUnits(allUnits);

        // 🔥 Recalcula ocupação dos tiles
        gridManager.RecalculateTileOccupations(allUnits);

        // 🔥 Atualiza highlights
        gridManager.ResetGridHighlights();
        gridManager.HighlightRange(
            gridManager.GetTile(activeUnit.gridPosition),
            activeUnit.movementLeft,
            activeUnit.attackRange
        );

        // 🔥 Ativa inputs do jogador atual
        EnableUnitsOfActivePlayer();
    }
    public void ExitCombatMode()
    {
        // Oculta o grid
        gridManager.DestroyGrid();

        // Desativa inputs táticos
        foreach (Unit u in allUnits)
            u.EnableInput(false);
    }

    private void SnapUnits(Unit[] units)
    {
        foreach (Unit unit in units)
        {
            Tile tile = gridManager.GetTile(unit.transform.position);
            if (tile == null)
            {
                Debug.LogWarning($"SnapUnits: Não encontrei tile para {unit.name}");
                continue;
            }

            unit.gridPosition = tile.gridPosition;
            unit.transform.position = tile.transform.position;
            tile.isOccupied = true;
        }
    }

    public void OnUnitSelected(Player player, Unit unit)
    {
        // Só processa seleção durante combate
        // (assume que se o grid existe / modo combate está ativo, o gridManager já foi configurado)
        if (gridManager == null || unit == null) return;

        // Só mostra highlights se for a vez do player que chamou (evita seleção de outras equipes)
        if (players[activePlayerIndex] != player)
            return;

        // Reset e highlight ao redor da unidade selecionada
        gridManager.ResetGridHighlights();

        Tile tile = gridManager.GetTile(unit.gridPosition);
        if (tile == null) return;

        gridManager.HighlightRange(
            tile,
            unit.inCombatMode ? unit.movementLeft : int.MaxValue,
            unit.inCombatMode ? unit.attackRange : 0
        );
    }



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


        if (gridManager == null || tilemapGenerator == null)
        {
            Debug.LogError("TurnManager: GridManager ou TilemapGridGenerator não encontrados na cena!");
            return;
        }

        // ===============================
        // 🔥 DESTRUIR GRID ATUAL
        // ===============================
        gridManager.DestroyGrid();

        // ===============================
        // 🔥 DEFINIR A UNIDADE DO NOVO JOGADOR
        //    (ajuste dependendo da sua lógica)
        // ===============================

        Unit activeUnit = players[activePlayerIndex].selectedUnit;

        if (activeUnit == null)
        {
            Debug.LogError("TurnManager: Jogador não possui uma Unit ativa!");
            return;
        }

        // Passa a posição do novo player para o gerador
        tilemapGenerator.player = activeUnit.transform;

        // ===============================
        // 🔥 RECRIAR O GRID AO REDOR DA NOVA UNIDADE
        // ===============================
        tilemapGenerator.GenerateGridFromTilemap(allUnits);

        EnableUnitsOfActivePlayer();
    }

    // Adicione este método
    public void SetCombatMode(bool enabled)
        {
            // UI
            endTurnButton.alpha = enabled ? 1 : 0;
            endTurnButton.interactable = enabled;
            turnUI.alpha = enabled ? 1 : 0;

            if (enabled)
                EnterCombatMode();
            else
                ExitCombatMode();
        }

    public void ShowTurnBanner()
    {
        bannerText.text = $"{players[activePlayerIndex].playerName}'s Turn";
        turnBanner.alpha = 1;
    }

    public void EnableUnitsOfActivePlayer()
    {
        // Desativa todas as units de todos os players
        foreach (Player p in players)
        {
            foreach (Unit u in p.playerUnits)
            {
                u.EnableInput(false);
            }
        }

        // Ativa apenas as units do player atual
        foreach (Unit u in ActivePlayer.playerUnits)
        {
            u.EnableInput(true);
        }
    }

}