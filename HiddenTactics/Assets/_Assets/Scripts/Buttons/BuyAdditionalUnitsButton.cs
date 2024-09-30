using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyAdditionalUnitsButton : TroopButtonUI, IPointerEnterHandler, IPointerExitHandler {

    private Button buyAdditionalUnitsButton;

    protected override void Awake() {
        base.Awake();
        buyAdditionalUnitsButton = GetComponent<Button>();

        buyAdditionalUnitsButton.onClick.AddListener(() => {

            int goldCost = troop.GetTroopSO().buyAdditionalUnitsCost;
            if ((PlayerGoldManager.Instance.CanSpendGold(goldCost, NetworkManager.Singleton.LocalClientId))) {
                buttonEnabled = false;
                buyAdditionalUnitsButton.interactable = false;
                PlayerGoldManager.Instance.SpendGold(troop.GetTroopSO().buyAdditionalUnitsCost, NetworkManager.Singleton.LocalClientId);
                troop.BuyAdditionalUnits();
            }

        });
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        if (!buttonEnabled) return;
        ShowAddingUnits();

    }

    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        if (!buttonEnabled) return;
        HideAdingUnits();
    }

    private void ShowAddingUnits() {
        if(troop.GetSingleUnitUpgradeNewPosition() != null) {
            foreach (Unit unit in troop.GetBoughtUnitInTroopList()) {
                unit.GetUnitVisual().ShowAsAdditionalUnitToBuy();
                unit.transform.position = troop.GetSingleUnitUpgradeNewPosition().position;
            }
        }

        foreach (Unit unit in troop.GetUnitsInAdditionalUnitsInTroopList()) {
            unit.GetUnitVisual().ShowAsAdditionalUnitToBuy();
            PlayerStateUI.Instance.SetPlayerGoldChangingUI(-troop.GetTroopSO().buyAdditionalUnitsCost);
        }
    }

    private void HideAdingUnits() {

        if (troop.GetSingleUnitUpgradeNewPosition() != null) {
            foreach (Unit unit in troop.GetBoughtUnitInTroopList()) {
                unit.GetUnitVisual().ResetUnitVisuals();
                unit.transform.position = troop.GetBaseUnitPositions()[0].position;
            }
        }

        foreach (Unit unit in troop.GetUnitsInAdditionalUnitsInTroopList()) {
            unit.GetUnitVisual().HideAsAdditionalUnitToBuy();
            PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
        }
    }

    protected override void CancelButtonShowVisuals() {
        HideAdingUnits();
    }
}