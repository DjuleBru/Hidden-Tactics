using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomizationData : MonoBehaviour
{

    public static PlayerCustomizationData Instance { get; private set; }

    [SerializeField] List<PlayerIconSO> playerIconSOList;

    [SerializeField] private List<Sprite> playerIconSpriteList;
    [SerializeField] private List<GridTileVisualSO> gridTileVisualSOList;
    [SerializeField] private List<Sprite> battlefieldBaseSpriteList;


    private void Awake() {
        Instance = this;
    }

    public Sprite GetPlayerIconSpriteFromSpriteId(int iconSpriteId) {
        return playerIconSpriteList[iconSpriteId];
    }

    public GridTileVisualSO GetPlayerGridTileVisualSOFromId(int gridTileId) {
        return gridTileVisualSOList[gridTileId];
    }

    public int GetGridTileVisualSOID(GridTileVisualSO gridTileVisualSO) {
        return gridTileVisualSOList.IndexOf(gridTileVisualSO);
    }

    public int GetBattlefieldBaseSpriteID(Sprite battlefieldBaseSprite) {
        return battlefieldBaseSpriteList.IndexOf(battlefieldBaseSprite);
    }

    public Sprite GetBattlefieldBaseSpriteFromId(int battlefieldBaseSpriteId) {
        return battlefieldBaseSpriteList[battlefieldBaseSpriteId];
    }

    public List<GridTileVisualSO> GetGridTileVisualSOList() { return gridTileVisualSOList; }
    public List<Sprite> GetBattlefieldBaseSpriteList() {  return battlefieldBaseSpriteList; }

}
