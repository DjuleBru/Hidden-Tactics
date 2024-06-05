using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridVisual : MonoBehaviour
{

    public static BattleGridVisual Instance { get; private set; }

    [SerializeField] List<Sprite> playerGridSprites;
    [SerializeField] List<Sprite> opponentGridSprites;

    [SerializeField] List<Sprite> playerVillageSprites;
    [SerializeField] List<Sprite> opponentVillageSprites;

    [SerializeField] List<Sprite> playerSettlementSprites;
    [SerializeField] List<Sprite> opponentSettlementSprites;

    [SerializeField] private bool isLobbyScene;

    private BattleGrid battleGrid;

    private void Awake() {
        Instance = this;
        battleGrid = GetComponentInParent<BattleGrid>();
    }

    private void Start() {

        if (BattleManager.Instance != null) {
            // Battle scene : create grid for both player AND opponent
            battleGrid.GetGridSystem().SetGridObjectVisualSprites(playerGridSprites, opponentGridSprites, playerSettlementSprites, opponentSettlementSprites, playerVillageSprites, opponentVillageSprites);
        } else {
            // Lobby scene : create grid for only player
            battleGrid.GetGridSystem().SetGridObjectVisualSprites(playerGridSprites, playerSettlementSprites, playerVillageSprites);
        }

        DeckManager.LocalInstance.OnDeckChanged += DeckManager_OnDeckChanged;
    }

    private void DeckManager_OnDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        string factionGridTilesSpriteKey = e.selectedDeck.deckFactionSO.ToString() + "_battlefieldGridTiles";
        List<Sprite> defaultFactionGridTilesSpriteList = e.selectedDeck.deckFactionSO.factionDefaultGridTileVisualSO.gridSpriteList;

        string factionSettlementSpriteKey = e.selectedDeck.deckFactionSO.ToString() + "_battlefieldSettlements";
        List<Sprite> defaultFactionSettlementSpriteList = e.selectedDeck.deckFactionSO.factionDefaultGridTileVisualSO.settlementSpriteList;

        playerGridSprites = ES3.Load(factionGridTilesSpriteKey, defaultValue: defaultFactionGridTilesSpriteList);
        playerSettlementSprites = ES3.Load(factionSettlementSpriteKey, defaultValue: defaultFactionSettlementSpriteList);

        RefreshPlayerBattlefieldVisualSprites();
    }

    public void SetPlayerGridTileSprites(List<Sprite> playerGridTileSprites) {
        playerGridSprites = playerGridTileSprites;

        RefreshPlayerBattlefieldVisualSprites();

        string factionGridTilesSpriteKey = DeckManager.LocalInstance.GetDeckSelected().deckFactionSO.ToString() + "_battlefieldGridTiles";
        ES3.Save(factionGridTilesSpriteKey, playerGridSprites);
    }

    public void SetPlayerSettlementSprites(List<Sprite> playerSettlementSprites) {
        this.playerSettlementSprites = playerSettlementSprites;

        RefreshPlayerBattlefieldVisualSprites();

        string factionSettlementSpriteKey = DeckManager.LocalInstance.GetDeckSelected().deckFactionSO.ToString() + "_battlefieldSettlements";
        ES3.Save(factionSettlementSpriteKey, playerSettlementSprites);
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
