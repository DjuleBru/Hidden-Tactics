using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyAdditionalUnitsButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] Troop troop;

    private Button buyAdditionalUnitsButton;

    private void Awake() {
        buyAdditionalUnitsButton = GetComponent<Button>();

        buyAdditionalUnitsButton.onClick.AddListener(() => {
            Debug.Log("buy additional units button ");
            int goldCost = troop.GetTroopSO().buyAdditionalUnitsCost;
            if ((PlayerGoldManager.Instance.CanSpendGold(goldCost, NetworkManager.Singleton.LocalClientId))) {
                PlayerGoldManager.Instance.SpendGold(troop.GetTroopSO().buyAdditionalUnitsCost, NetworkManager.Singleton.LocalClientId);
                troop.BuyAdditionalUnits();
            }
        });
    }

    public void OnPointerEnter(PointerEventData eventData) {
        foreach (Unit unit in troop.GetUnitsInAdditionalUnitsInTroopList()) {
            unit.GetUnitVisual().ShowAsAdditionalUnitToBuy();
            PlayerStateUI.Instance.SetPlayerGoldChangingUI(-troop.GetTroopSO().buyAdditionalUnitsCost);
        }

    }

    public void OnPointerExit(PointerEventData eventData) {
        foreach (Unit unit in troop.GetUnitsInAdditionalUnitsInTroopList()) {
            unit.GetUnitVisual().HideAsAdditionalUnitToBuy();
            PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
        }
    }
}