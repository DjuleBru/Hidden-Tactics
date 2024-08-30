using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomizationDataManager : MonoBehaviour
{

    public static PlayerCustomizationDataManager Instance { get; private set; }

    [SerializeField] List<PlayerIconSO> playerIconSOList;

    [SerializeField] private List<Sprite> playerIconSpriteList;
    [SerializeField] private List<GridTileVisualSO> gridTileVisualSOList;
    [SerializeField] private List<BattlefieldBaseSO> battlefieldBaseSOList;

    [SerializeField] private List<Sprite> villageSpriteList;
    [SerializeField] private List<FactionSO> factionSOList;


    private void Awake() {
        Instance = this;
    }

    public List<PlayerIconSO> GetplayerIconSOList() {
        return playerIconSOList;
    }

    public Sprite GetPlayerIconSpriteFromSpriteId(int iconSpriteId) {
        return playerIconSOList[iconSpriteId].iconSprite;
    }

    public PlayerIconSO GetPlayerIconSOFromSpriteId(int iconSpriteId) {
        return playerIconSOList[iconSpriteId];
    }

    public Sprite GetVillageSpriteFromSpriteId(int villageSpriteIt) {
        return villageSpriteList[villageSpriteIt];
    }

    public GridTileVisualSO GetPlayerGridTileVisualSOFromId(int gridTileId) {
        return gridTileVisualSOList[gridTileId];
    }

    public FactionSO GetFactionSOFromId(int factionSOID) {
        return factionSOList[factionSOID];
    }

    public int GetGridTileVisualSOID(GridTileVisualSO gridTileVisualSO) {
        return gridTileVisualSOList.IndexOf(gridTileVisualSO);
    }

    public int GetBattlefieldBaseSOID(BattlefieldBaseSO battlefieldBaseSO) {
        return battlefieldBaseSOList.IndexOf(battlefieldBaseSO);
    }

    public int GetVillageSpriteID(Sprite villageSprite) {
        return villageSpriteList.IndexOf(villageSprite);
    }

    public int GetFactionSOID(FactionSO factionSO) {
        return factionSOList.IndexOf(factionSO);
    }

    public BattlefieldBaseSO GetBattlefieldBaseSOFromId(int battlefieldBaseSpriteId) {
        return battlefieldBaseSOList[battlefieldBaseSpriteId];
    }

    public List<GridTileVisualSO> GetGridTileVisualSOList() { return gridTileVisualSOList; }
    public List<BattlefieldBaseSO> GetBattlefieldBaseSOList() {  return battlefieldBaseSOList; }
    public List<Sprite> GetVillageSpriteList() { return villageSpriteList; }
}
