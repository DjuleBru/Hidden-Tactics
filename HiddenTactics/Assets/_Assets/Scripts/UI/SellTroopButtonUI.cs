using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SellTroopButtonUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler {
    [SerializeField] Troop troop;

    private Button sellTroopUnitsButton;
    private int goldRedundValue;

    private void Awake() {
        sellTroopUnitsButton = GetComponent<Button>();
        sellTroopUnitsButton.onClick.AddListener(() => {

            if(troop.TroopWasPlacedThisPreparationPhase()) {
                goldRedundValue = troop.GetTroopSO().spawnTroopCost;
            } else {
                goldRedundValue = troop.GetTroopSO().spawnTroopCost/3;
            }

            PlayerGoldManager.Instance.EarnGold(goldRedundValue);
            troop.SellTroop();
            sellTroopUnitsButton.enabled = false;
        });
    }

    public void OnPointerEnter(PointerEventData eventData) {
        foreach (Unit unit in troop.GetUnitInTroopList()) {
            if (!unit.GetUnitIsBought()) continue;
            unit.GetUnitUI().ShowUnitAsSellingUnit();

            if (troop.TroopWasPlacedThisPreparationPhase()) {
                goldRedundValue = troop.GetTroopSO().spawnTroopCost;
            }
            else {
                goldRedundValue = troop.GetTroopSO().spawnTroopCost / 3;
            }

            PlayerStateUI.Instance.SetPlayerGoldChangingUI(goldRedundValue);
        }

    }

    public void OnPointerExit(PointerEventData eventData) {
        foreach (Unit unit in troop.GetUnitInTroopList()) {
            unit.GetUnitUI().HideUnitTargetUI();
        }
        PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
    }
}
