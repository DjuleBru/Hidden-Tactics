using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlefieldVisualBaseTemplate : MonoBehaviour
{
    [SerializeField] private Image battlefieldBaseImage;
    [SerializeField] private Image battlefieldBaseShadowImage;
    [SerializeField] private Image battlefieldBaseOutlineImage;
    [SerializeField] private TextMeshProUGUI battlefieldBaseName;
    private BattlefieldBaseSO battlefieldBaseSO;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material cleanMaterial;

    public static event EventHandler OnAnyBattlefieldVisualBaseSelected;
    private bool selected;

    private Button button;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            HiddenTacticsMultiplayer.Instance.SetPlayerBattlefieldBaseSO(PlayerCustomizationDataManager.Instance.GetBattlefieldBaseSOID(battlefieldBaseSO));
            BattlefieldVisual.Instance.SetBattlefieldBaseSO(battlefieldBaseSO);
            OnAnyBattlefieldVisualBaseSelected?.Invoke(this, EventArgs.Empty);
            SetBattlefieldBaseSOSelected(true);
        });
    }

    private void Start() {
        OnAnyBattlefieldVisualBaseSelected += BattlefieldVisualBaseTemplate_OnAnyBattlefieldVisualBaseSelected;
    }

    private void BattlefieldVisualBaseTemplate_OnAnyBattlefieldVisualBaseSelected(object sender, EventArgs e) {
        if ((sender as BattlefieldVisualBaseTemplate) != this)
        {
            SetBattlefieldBaseSOSelected(false);
        }
    }

    public void SetBattlefieldBaseSO(BattlefieldBaseSO battlefieldBaseSO) {
        this.battlefieldBaseSO = battlefieldBaseSO;
        battlefieldBaseImage.sprite = battlefieldBaseSO.battlefieldBaseMenuSprite;
        battlefieldBaseShadowImage.sprite = battlefieldBaseSO.battlefieldBaseMenuSprite;
        battlefieldBaseName.text = battlefieldBaseSO.baseName;
    }

    public void SetBattlefieldBaseSOSelected(bool selected) {
        this.selected = selected;
        if (selected) {
            battlefieldBaseOutlineImage.material = selectedMaterial;
        }
        else {
            battlefieldBaseOutlineImage.material = cleanMaterial;
        }
    }

    public void OnDestroy() {
        OnAnyBattlefieldVisualBaseSelected -= BattlefieldVisualBaseTemplate_OnAnyBattlefieldVisualBaseSelected;
    }
}
