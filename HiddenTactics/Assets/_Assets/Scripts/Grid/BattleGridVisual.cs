using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridVisual : MonoBehaviour
{

    public static BattleGridVisual Instance { get; private set; }

    List<Sprite> playerGridSprites;
    List<Sprite> opponentGridSprites;

    [SerializeField] List<Sprite> playerVillageSprites;
    [SerializeField] List<Sprite> opponentVillageSprites;

    List<Sprite> playerSettlementSprites;
    List<Sprite> opponentSettlementSprites;

    [SerializeField] private bool isLobbyScene;

    private BattleGrid battleGrid;

    private void Awake() {
        Instance = this;
        battleGrid = GetComponentInParent<BattleGrid>();
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;

        if (BattleManager.Instance != null) {
            // Battle scene : create grid for both player AND opponent
            LoadOpponentSprites();
            LoadPlayerSprites();
            battleGrid.GetGridSystem().SetGridObjectVisualSprites(playerGridSprites, opponentGridSprites, playerSettlementSprites, opponentSettlementSprites, playerVillageSprites, opponentVillageSprites);
        }
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {

        if (BattleManager.Instance == null) {
            // Lobby scene : create grid only for player 

            LoadPlayerBattleGridSprites(e);
            RefreshPlayerBattlefieldVisualSprites();

        }
    }

    private void LoadPlayerBattleGridSprites(DeckManager.OnDeckChangedEventArgs e) {
        // Load player grid tile SO  from selected deck (local machine)
        Deck selectedDeck = DeckManager.LocalInstance.GetDeckSelected();
        GridTileVisualSO playerGridTileVisualSO = SavingManager.Instance.LoadGridTileVisualSO(selectedDeck);

        playerGridSprites = playerGridTileVisualSO.gridSpriteList;
        playerSettlementSprites = playerGridTileVisualSO.settlementSpriteList;

        //Save GridTiles To PlayerData
        HiddenTacticsMultiplayer.Instance.SetPlayerGridVisualSO(PlayerCustomizationData.Instance.GetGridTileVisualSOID(playerGridTileVisualSO));
    }

    private void LoadOpponentSprites() {
        PlayerData opponentData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData();

        Debug.Log("opponent gridSO id " + opponentData.gridVisualSOId);
        opponentGridSprites = PlayerCustomizationData.Instance.GetPlayerGridTileVisualSOFromId(opponentData.gridVisualSOId).gridSpriteList;
        opponentSettlementSprites = PlayerCustomizationData.Instance.GetPlayerGridTileVisualSOFromId(opponentData.gridVisualSOId).settlementSpriteList;
    }

    private void LoadPlayerSprites() {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetLocalPlayerData();

        Debug.Log("player gridSO id " + playerData.gridVisualSOId);

        playerGridSprites = PlayerCustomizationData.Instance.GetPlayerGridTileVisualSOFromId(playerData.gridVisualSOId).gridSpriteList;
        playerSettlementSprites = PlayerCustomizationData.Instance.GetPlayerGridTileVisualSOFromId(playerData.gridVisualSOId).settlementSpriteList;
    }

    public void SetPlayerGridTileVisualSO(GridTileVisualSO gridTileVisualSO) {
        playerGridSprites = gridTileVisualSO.gridSpriteList;
        playerSettlementSprites = gridTileVisualSO.settlementSpriteList;

        // Save GridTileVisualSO To Local Machine
        SavingManager.Instance.SavePlayerGridTileVisualSO(gridTileVisualSO);

        RefreshPlayerBattlefieldVisualSprites();

        //Set GridTiles To PlayerData
        HiddenTacticsMultiplayer.Instance.SetPlayerGridVisualSO(PlayerCustomizationData.Instance.GetGridTileVisualSOID(gridTileVisualSO));
    }

    public void SetPlayerVillageSprites(List<Sprite> playerVillageSprites) {
        this.playerVillageSprites = playerVillageSprites;
        RefreshPlayerBattlefieldVisualSprites();
    }

    private void RefreshPlayerBattlefieldVisualSprites() {
        battleGrid.GetGridSystem().SetGridObjectVisualSprites(playerGridSprites, playerSettlementSprites, playerVillageSprites);
    }

    public Sprite GetRandomPlayerVillageSprite() {
        return playerVillageSprites[Random.Range(0, playerVillageSprites.Count)];
    }

    public Sprite GetRandomOpponentVillageSprite() {
        return opponentVillageSprites[Random.Range(0, playerVillageSprites.Count)];
    }

}
