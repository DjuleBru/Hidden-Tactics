using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomizationDataManager : MonoBehaviour
{

    public static PlayerCustomizationDataManager Instance { get; private set; }

    [SerializeField] List<PlayerIconSO> playerIconSOList;

    [SerializeField] private List<Sprite> playerIconSpriteList;
    [SerializeField] private List<GridTileVisualSO> gridTileVisualSOList;
    [SerializeField] private List<Sprite> battlefieldBaseSpriteList;

    [SerializeField] private List<Sprite> villageSpriteList;
    [SerializeField] private List<FactionSO> factionSOList;


    private void Awake() {
        Instance = this;
    }

    public Sprite GetPlayerIconSpriteFromSpriteId(int iconSpriteId) {
        return playerIconSpriteList[iconSpriteId];
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

    public int GetBattlefieldBaseSpriteID(Sprite battlefieldBaseSprite) {
        return battlefieldBaseSpriteList.IndexOf(battlefieldBaseSprite);
    }

    public int GetVillageSpriteID(Sprite villageSprite) {
        return villageSpriteList.IndexOf(villageSprite);
    }

    public int GetFactionSOID(FactionSO factionSO) {
        return factionSOList.IndexOf(factionSO);
    }

    public Sprite GetBattlefieldBaseSpriteFromId(int battlefieldBaseSpriteId) {
        return battlefieldBaseSpriteList[battlefieldBaseSpriteId];
    }

    public List<GridTileVisualSO> GetGridTileVisualSOList() { return gridTileVisualSOList; }
    public List<Sprite> GetBattlefieldBaseSpriteList() {  return battlefieldBaseSpriteList; }
    public List<Sprite> GetVillageSpriteList() { return villageSpriteList; }
}
