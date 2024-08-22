using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyAdditionalUnitsButton : TroopButtonUI, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] Troop troop;

    private Button buyAdditionalUnitsButton;

    protected void Awake() {
        buyAdditionalUnitsButton = GetComponent<Button>();

        buyAdditionalUnitsButton.onClick.AddListener(() => {
            int goldCost = troop.GetTroopSO().buyAdditionalUnitsCost;
            if ((PlayerGoldManager.Instance.CanSpendGold(goldCost, NetworkManager.Singleton.LocalClientId))) {
                PlayerGoldManager.Instance.SpendGold(troop.GetTroopSO().buyAdditionalUnitsCost, NetworkManager.Singleton.LocalClientId);
                troop.BuyAdditionalUnits();
            }
        });
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        foreach (Unit unit in troop.GetUnitsInAdditionalUnitsInTroopList()) {
            unit.GetUnitVisual().ShowAsAdditionalUnitToBuy();
            PlayerStateUI.Instance.SetPlayerGoldChangingUI(-troop.GetTroopSO().buyAdditionalUnitsCost);
        }

    }

    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        foreach (Unit unit in troop.GetUnitsInAdditionalUnitsInTroopList()) {
            unit.GetUnitVisual().HideAsAdditionalUnitToBuy();
            PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
        }
    }
}