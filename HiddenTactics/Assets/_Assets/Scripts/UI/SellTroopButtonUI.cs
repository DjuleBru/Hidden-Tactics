using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SellTroopButtonUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler {

    [SerializeField] Troop troop;
    [SerializeField] Building building;

    private Button sellTroopUnitsButton;
    private int goldRedundValue;

    private void Awake() {
        sellTroopUnitsButton = GetComponent<Button>();
        sellTroopUnitsButton.onClick.AddListener(() => {

            if(troop != null) {
                if (troop.TroopWasPlacedThisPreparationPhase()) {
                    goldRedundValue = troop.GetTroopSO().spawnTroopCost;
                }
                else {
                    goldRedundValue = troop.GetTroopSO().spawnTroopCost / 3;
                }

                troop.SellTroop();
                PlayerGoldManager.Instance.EarnGold(goldRedundValue);
                sellTroopUnitsButton.enabled = false;
            }

            if(building != null) {
                if (building.BuildingWasPlacedThisPreparationPhase()) {
                    goldRedundValue = building.GetBuildingSO().spawnBuildingCost;
                }
                else {
                    goldRedundValue = building.GetBuildingSO().spawnBuildingCost / 3;
                }

                building.SellBuilding();
                PlayerGoldManager.Instance.EarnGold(goldRedundValue);
                sellTroopUnitsButton.enabled = false;
            }

        });
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if(building != null) {
            building.GetBuildingUI().ShowUnitAsSellingBuilding();
        } else {
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
    }

    public void OnPointerExit(PointerEventData eventData) {
        if(building != null) {
            building.GetBuildingUI().HideBuildingTargetUI();
        } else {
            foreach (Unit unit in troop.GetUnitInTroopList()) {
                unit.GetUnitUI().HideUnitTargetUI();
            }
            PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
        }
    }
}
