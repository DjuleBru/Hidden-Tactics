using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : NetworkBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private GameObject unitHPBarGameObject;
    [SerializeField] private Image unitHPBarImage;

    public override void OnNetworkSpawn() {
        unit.OnHealthChanged += Unit_OnHealthChanged;
        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitReset += Unit_OnUnitReset;
        unit.OnUnitSetAsAdditionalUnit += Unit_OnUnitSetAsAdditionalUnit;
    }


    private void Unit_OnUnitDied(object sender, System.EventArgs e) {
        unitHPBarGameObject.SetActive(false);
    }

    private void Unit_OnHealthChanged(object sender, System.EventArgs e) {
        UpdateUnitHPBar();
    }

    private void Unit_OnUnitSetAsAdditionalUnit(object sender, System.EventArgs e) {
        unitHPBarGameObject.SetActive(false);
    }

    private void Unit_OnUnitReset(object sender, System.EventArgs e) {
        if (!unit.UnitIsBought()) return;
        unitHPBarGameObject.SetActive(true);
    }

    private void UpdateUnitHPBar() {
        unitHPBarImage.fillAmount = unit.GetUnitHPNormalized();
    }

}
