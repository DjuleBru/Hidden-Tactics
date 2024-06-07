using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridVisual : MonoBehaviour
{

    public static BattleGridVisual Instance { get; private set; }

    List<Sprite> playerGridSprites;
    List<Sprite> opponentGridSprites;

    List<Sprite> playerVillageSprites;
    List<Sprite> opponentVillageSprites;

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
        LoadPlayerBattleGridSprites(DeckManager.LocalInstance.GetDeckSelected());

        if (BattleManager.Instance != null) {

            // Battle scene : create grid for both player AND opponent
            LoadOpponentSprites();
            RefreshPlayerAndOpponentBattlefieldVisualSprites();

        }  else {

            RefreshPlayerBattlefieldVisualSprites();
        }
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {

        if (BattleManager.Instance == null) {
            // Lobby scene : create grid only for player 

            LoadPlayerBattleGridSprites(e.selectedDeck);
            RefreshPlayerBattlefieldVisualSprites();
        }
    }

    private void LoadPlayerBattleGridSprites(Deck deck) {
        // Load player grid tile SO  from selected deck (local machine)
        Deck selectedDeck = DeckManager.LocalInstance.GetDeckSelected();
        GridTileVisualSO playerGridTileVisualSO = SavingManager.Instance.LoadGridTileVisualSO(selectedDeck);

        List<Sprite> loadedVillageSpriteList = SavingManager.Instance.LoadVillageSpriteList(selectedDeck);

        playerGridSprites = playerGridTileVisualSO.gridSpriteList;
        playerSettlementSprites = playerGridTileVisualSO.settlementSpriteList;
        Debug.Log(playerSettlementSprites[0]);
        playerVillageSprites = loadedVillageSpriteList;

        //Save GridTiles To PlayerData
        HiddenTacticsMultiplayer.Instance.SetPlayerGridVisualSO(PlayerCustomizationDataManager.Instance.GetGridTileVisualSOID(playerGridTileVisualSO));
    }

    private void LoadOpponentSprites() {
        PlayerCustomizationData opponentCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData();

        opponentGridSprites = PlayerCustomizationDataManager.Instance.GetPlayerGridTileVisualSOFromId(opponentCustomizationData.gridVisualSOId).gridSpriteList;
        opponentSettlementSprites = PlayerCustomizationDataManager.Instance.GetPlayerGridTileVisualSOFromId(opponentCustomizationData.gridVisualSOId).settlementSpriteList;

        opponentVillageSprites = new List<Sprite>();

        Debug.Log("opponent village sprites number = " + opponentCustomizationData.villageSpriteNumber);

        if (opponentCustomizationData.villageSpriteNumber >= 1) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(opponentCustomizationData.villageSprite0Id);
            opponentVillageSprites.Add(villageSprite);
        }
        if (opponentCustomizationData.villageSpriteNumber >= 2) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(opponentCustomizationData.villageSprite1Id);
            opponentVillageSprites.Add(villageSprite);
        }
        if (opponentCustomizationData.villageSpriteNumber >= 3) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(opponentCustomizationData.villageSprite2Id);
            opponentVillageSprites.Add(villageSprite);
        }
        if (opponentCustomizationData.villageSpriteNumber >= 4) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(opponentCustomizationData.villageSprite3Id);
            opponentVillageSprites.Add(villageSprite);
        }
        if (opponentCustomizationData.villageSpriteNumber >= 5) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(opponentCustomizationData.villageSprite4Id);
            opponentVillageSprites.Add(villageSprite);
        }

        if (opponentCustomizationData.villageSpriteNumber >= 6) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(opponentCustomizationData.villageSprite5Id);
            opponentVillageSprites.Add(villageSprite);
        }

        foreach(Sprite sprite in opponentVillageSprites) {
            Debug.Log(sprite);
        }
    }

    private void LoadPlayerSprites() {
        PlayerCustomizationData playerCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalPlayerCustomizationData();

        Debug.Log("player gridSO id " + playerCustomizationData.gridVisualSOId);

        playerGridSprites = PlayerCustomizationDataManager.Instance.GetPlayerGridTileVisualSOFromId(playerCustomizationData.gridVisualSOId).gridSpriteList;
        playerSettlementSprites = PlayerCustomizationDataManager.Instance.GetPlayerGridTileVisualSOFromId(playerCustomizationData.gridVisualSOId).settlementSpriteList;

        if (playerCustomizationData.villageSpriteNumber >= 1) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(playerCustomizationData.villageSprite0Id);
            opponentSettlementSprites.Add(villageSprite);
        }
        if (playerCustomizationData.villageSpriteNumber >= 2) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(playerCustomizationData.villageSprite1Id);
            opponentSettlementSprites.Add(villageSprite);
        }
        if (playerCustomizationData.villageSpriteNumber >= 3) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(playerCustomizationData.villageSprite2Id);
            opponentSettlementSprites.Add(villageSprite);
        }
        if (playerCustomizationData.villageSpriteNumber >= 4) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(playerCustomizationData.villageSprite3Id);
            opponentSettlementSprites.Add(villageSprite);
        }
        if (playerCustomizationData.villageSpriteNumber >= 5) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(playerCustomizationData.villageSprite4Id);
            opponentSettlementSprites.Add(villageSprite);
        }

        if (playerCustomizationData.villageSpriteNumber >= 6) {
            Sprite villageSprite = PlayerCustomizationDataManager.Instance.GetVillageSpriteFromSpriteId(playerCustomizationData.villageSprite5Id);
            opponentSettlementSprites.Add(villageSprite);
        }
    }

    public void SetPlayerGridTileVisualSO(GridTileVisualSO gridTileVisualSO) {
        playerGridSprites = gridTileVisualSO.gridSpriteList;
        playerSettlementSprites = gridTileVisualSO.settlementSpriteList;

        // Save GridTileVisualSO To Local Machine
        SavingManager.Instance.SavePlayerGridTileVisualSO(gridTileVisualSO);

        RefreshPlayerBattlefieldVisualSprites();

        //Set GridTiles To PlayerData
        HiddenTacticsMultiplayer.Instance.SetPlayerGridVisualSO(PlayerCustomizationDataManager.Instance.GetGridTileVisualSOID(gridTileVisualSO));
    }

    public void SetPlayerVillageSprites(List<Sprite> playerVillageSprites) {
        this.playerVillageSprites = playerVillageSprites;

        // Save VillageSprites To Local Machine
        SavingManager.Instance.SaveVillageSprites(playerVillageSprites);

        RefreshPlayerBattlefieldVisualSprites();

        // Set VillageSprite To PlayerData
        List<int> villageSpriteIdList = new List<int>();    
        foreach(Sprite sprite in playerVillageSprites) {
            villageSpriteIdList.Add(PlayerCustomizationDataManager.Instance.GetVillageSpriteID(sprite));
        }

        HiddenTacticsMultiplayer.Instance.SetPlayerVillageSprites(villageSpriteIdList);
    }

    public bool TryAddPlayerVillageSprites(Sprite playerVillageSprite) {
        if(!playerVillageSprites.Contains(playerVillageSprite)) {
            playerVillageSprites.Add(playerVillageSprite);
            SetPlayerVillageSprites(playerVillageSprites);
            return true;
        }
        return false;
    }

    public bool TryRemovePlayerVillageSprites(Sprite playerVillageSprite) {

        if(playerVillageSprites.Count > 1) {
            playerVillageSprites.Remove(playerVillageSprite);
            SetPlayerVillageSprites(playerVillageSprites);
            return true;
        }
        return false;

    }

    private void RefreshPlayerBattlefieldVisualSprites() {
        battleGrid.GetGridSystem().SetGridObjectVisualSprites(playerGridSprites, playerSettlementSprites, playerVillageSprites);
    }

    private void RefreshPlayerAndOpponentBattlefieldVisualSprites() {
        battleGrid.GetGridSystem().SetGridObjectVisualSprites(playerGridSprites, opponentGridSprites, playerSettlementSprites, opponentSettlementSprites);
    }

    public Sprite GetRandomPlayerVillageSprite() {
        return playerVillageSprites[Random.Range(0, playerVillageSprites.Count)];
    }

    public Sprite GetRandomOpponentVillageSprite() {
        return opponentVillageSprites[Random.Range(0, opponentVillageSprites.Count)];
    }

}
