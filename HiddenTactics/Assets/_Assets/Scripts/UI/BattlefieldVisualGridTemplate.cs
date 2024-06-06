using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlefieldVisualGridTemplate : MonoBehaviour
{
    private Button button;

    [SerializeField] private Image gridImage;
    private GridTileVisualSO gridTileVisualSO;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            SetBattlefieldVisualGridTile();
        });
    }

    private void SetBattlefieldVisualGridTile() {
        BattleGridVisual.Instance.SetPlayerGridTileVisualSO(gridTileVisualSO);
        BattlefieldVisual.Instance.SetBattlefieldOutlineSprite(gridTileVisualSO.battlefieldOutlineSprite);
    }

    public void SetGridTileVisualSO(GridTileVisualSO gridTileVisualSO) {
        this.gridTileVisualSO = gridTileVisualSO;

        gridImage.sprite = gridTileVisualSO.gridSpriteList[0];
    }
}
