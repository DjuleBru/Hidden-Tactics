using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlefieldVisualGridTemplate : MonoBehaviour
{
    private Button button;

    [SerializeField] private Image gridImage;
    [SerializeField] private Image gridOutlineImage;
    private GridTileVisualSO gridTileVisualSO;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material cleanMaterial;

    public static event EventHandler OnAnyBattlefieldGridSelected;
    private bool selected;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            SetBattlefieldVisualGridTile();
        });
    }

    private void SetBattlefieldVisualGridTile() {
        BattleGridVisual.Instance.SetPlayerGridTileVisualSO(gridTileVisualSO);
        BattlefieldVisual.Instance.SetBattlefieldOutlineSprite(gridTileVisualSO.battlefieldOutlineSprite);

        OnAnyBattlefieldGridSelected += BattlefieldVisualGridTemplate_OnAnyBattlefieldGridSelected;
        OnAnyBattlefieldGridSelected?.Invoke(this, EventArgs.Empty);
        SetGridTileSelected(true);
    }

    private void BattlefieldVisualGridTemplate_OnAnyBattlefieldGridSelected(object sender, EventArgs e) {
        if(sender as  BattlefieldVisualGridTemplate != this) {
            SetGridTileSelected(false);
        } 
    }

    public void SetGridTileVisualSO(GridTileVisualSO gridTileVisualSO) {
        this.gridTileVisualSO = gridTileVisualSO;

        gridImage.sprite = gridTileVisualSO.gridSpriteList[0];
    }

    public void SetGridTileSelected(bool selected) {
        this.selected = selected;;
        if (selected) {
            gridOutlineImage.material = selectedMaterial;
        }
        else {
            gridOutlineImage.material = cleanMaterial;
        }
    }

    public void OnDestroy() {
        OnAnyBattlefieldGridSelected -= BattlefieldVisualGridTemplate_OnAnyBattlefieldGridSelected;
    }

}
