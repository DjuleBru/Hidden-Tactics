using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SavingManager : MonoBehaviour
{
    [SerializeField] private int saveSlot;
    [SerializeField] private Sprite battlefieldBaseDefaultSprite;
    [SerializeField] private FactionSO defaultFactionSO;

    public static SavingManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }


    #region SAVE PLAYER CUSTOMIZATION DATA
    public void SavePlayerName(string playerName) {
        string playerNameKey = saveSlot.ToString() + PlayerSaveConstString.PLAYER_NAME_MULTIPLAYER;

        ES3.Save(playerNameKey, playerName);
    }

    public void SavePlayerIconSpriteId(int playerIconId) {
        ES3.Save(saveSlot.ToString() + PlayerSaveConstString.PLAYER_ICON_SPRITE_MULTIPLAYER, playerIconId);
    }

    public void SavePlayerFactionId(int factionID) {
        ES3.Save(saveSlot.ToString() + PlayerSaveConstString.PLAYER_FACTION_ID, factionID);
    }

    public void SavePlayerGridVisualSOId(int playerGridVisualSOId) {
        ES3.Save(saveSlot.ToString() + PlayerSaveConstString.PLAYER_BATTLEFIELD_GRIDTILEVISUAL_MULTIPLAYER, playerGridVisualSOId);
    }

    public void SavePlayerBattlefieldBaseSpriteId(int playerBattlefieldBaseSpriteId) {
        ES3.Save(saveSlot.ToString() + PlayerSaveConstString.PLAYER_BATTLEFIELD_BASE_MULTIPLAYER, playerBattlefieldBaseSpriteId);
    }

    public void SavePlayerVillagesSpriteIdList(List<int> playerBattlefieldVillageSpriteIdList) {
        ES3.Save(saveSlot.ToString() + PlayerSaveConstString.PLAYER_VILLAGES_MULTIPLAYER, playerBattlefieldVillageSpriteIdList);
    }

    public void SavePlayerGridTileVisualSO(GridTileVisualSO gridTileVisualSO) {
        // Save GridTileVisualSO To Local Machine

        string factionGridTilesSpriteKey = saveSlot.ToString() + DeckManager.LocalInstance.GetDeckSelected().deckFactionSO.ToString() + "_gridTileVisualSO";
        ES3.Save(factionGridTilesSpriteKey, gridTileVisualSO);
    }

    public void SaveBattlefieldBaseSprite(Sprite battlefieldBaseSprite) {
        string battlefieldBaseSpriteKey = saveSlot.ToString() + DeckManager.LocalInstance.GetDeckSelected().deckFactionSO.ToString() + "_battlefieldBaseSprite";
        ES3.Save(battlefieldBaseSpriteKey, battlefieldBaseSprite);
    }

    public void SaveVillageSprites(List<Sprite> villageSpriteList) {
        string villageSpriteListKey = saveSlot.ToString() + DeckManager.LocalInstance.GetDeckSelected().deckFactionSO.ToString() + "_villageSpriteList";
        ES3.Save(villageSpriteListKey, villageSpriteList);
    }


    #endregion

    #region SAVE PLAYER DECKS
    public void SaveFactionSO(FactionSO factionSO) {
        string factionSOKey = saveSlot.ToString() + "DeckFactionSelected";
        ES3.Save(factionSOKey, factionSO);
    }

    public void SaveDeck(Deck deck, int deckNumber) {
        string deckId = saveSlot.ToString() + "deck_" + deck.deckFactionSO.ToString() + "_" + deckNumber.ToString();
        string deckSelectedId = saveSlot.ToString() + "DeckSelected";

        ES3.Save(deckId, deck);
        ES3.Save(deckSelectedId, deck);
    }
    #endregion


    #region LOAD PLAYER CUSTOMIZATION DATA
    public Sprite LoadBattlefieldBaseSprite(Deck deck) {
        string battlefieldBaseSpriteKey = saveSlot.ToString() + deck.deckFactionSO.ToString() + "_battlefieldBaseSprite";

        return ES3.Load(battlefieldBaseSpriteKey, defaultValue: battlefieldBaseDefaultSprite);
    }

    public List<Sprite> LoadVillageSpriteList(Deck deck) {
        string villageListKey = saveSlot.ToString() + deck.deckFactionSO.ToString() + "_villageSpriteList";

        Sprite defaultFactionVillageSprite = deck.deckFactionSO.factionDefaultGridTileVisualSO.defaultVillageSprite;
        List<Sprite> defaultFactionVillageSpriteList = new List<Sprite>();
        defaultFactionVillageSpriteList.Add(defaultFactionVillageSprite);

        return ES3.Load(villageListKey, defaultValue: defaultFactionVillageSpriteList);
    }

    public GridTileVisualSO LoadGridTileVisualSO(Deck deck) {
        // Load GridTileVisualSO from Local Machine

        string factionGridTileVisualSOKey = saveSlot.ToString() + deck.deckFactionSO.ToString() + "_gridTileVisualSO";
        GridTileVisualSO defaultFactionGridTileVisualSO = deck.deckFactionSO.factionDefaultGridTileVisualSO;

        GridTileVisualSO loadedGridTileVisualSO = ES3.Load(factionGridTileVisualSOKey, defaultValue: defaultFactionGridTileVisualSO);
        return loadedGridTileVisualSO;
    }

    public string LoadPlayerName() {
        string playerNameKey = saveSlot.ToString() + PlayerSaveConstString.PLAYER_NAME_MULTIPLAYER;
        return ES3.Load(playerNameKey, defaultValue: "Player#" + UnityEngine.Random.Range(0, 1000));
    }

    public int LoadPlayerIconSpriteId() {
        return ES3.Load(saveSlot.ToString() + PlayerSaveConstString.PLAYER_ICON_SPRITE_MULTIPLAYER, defaultValue: 0);
    }

    public int LoadPlayerFactionId() {
        return ES3.Load(saveSlot.ToString() + PlayerSaveConstString.PLAYER_FACTION_ID, defaultValue: 0);
    }

    public int LoadPlayerGridVisualSOId() {
        return ES3.Load(saveSlot.ToString() + PlayerSaveConstString.PLAYER_BATTLEFIELD_GRIDTILEVISUAL_MULTIPLAYER, 0);
    }

    public int LoadPlayerBattlefieldBaseSpriteId() {
        return ES3.Load(saveSlot.ToString() + PlayerSaveConstString.PLAYER_BATTLEFIELD_BASE_MULTIPLAYER, 0);
    }

    public List<int> LoadPlayerVillageSpriteIdList() {
        List<int> defaultFactionVillageSpriteIdList = new List<int>();
        defaultFactionVillageSpriteIdList.Add(0);

        return ES3.Load(saveSlot.ToString() + PlayerSaveConstString.PLAYER_VILLAGES_MULTIPLAYER, defaultFactionVillageSpriteIdList);
    }

    #endregion

    #region LOAD PLAYER DECKS
    public Deck LoadDeck() {
        Deck defaultDeckOnStartup = new Deck(defaultFactionSO, 1);

        string deckKey = saveSlot.ToString() + "DeckSelected";

        return ES3.Load(deckKey, defaultDeckOnStartup);
    }

    public Deck LoadDeck(FactionSO factionSO, int deckNumber) {
        Deck defaultDeckOnStartup = new Deck(factionSO, deckNumber);

        string deckKey = saveSlot.ToString() + "deck_" + factionSO.ToString() + "_" + deckNumber.ToString();
        return ES3.Load(deckKey, defaultDeckOnStartup);
    }

    public FactionSO LoadFactionSO() {
        string factionSOKey = saveSlot.ToString() + "DeckFactionSelected";
        return ES3.Load(factionSOKey, defaultValue: defaultFactionSO);
    }
    #endregion

}
