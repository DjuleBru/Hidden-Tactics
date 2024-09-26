using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SellTroopButtonUI : TroopButtonUI, IPointerExitHandler, IPointerEnterHandler {

    private Button sellTroopUnitsButton;
    private int goldRedundValue;

    protected override void Awake() {
        base.Awake();
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
            }

        });
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        ShowSelling();
    }

    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        HideSelling();
    }

    private void ShowSelling() {

        if (building != null) {
            building.GetBuildingUI().ShowUnitAsSellingBuilding();
        }
        else {
            foreach (Unit unit in troop.GetBoughtUnitInTroopList()) {
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

    private void HideSelling() {

        if (building != null) {
            building.GetBuildingUI().HideBuildingTargetUI();
        }
        else {
            foreach (Unit unit in troop.GetUnitInTroopList()) {
                unit.GetUnitUI().HideUnitTargetUI();
            }
            PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
        }
    }

    protected override void CancelButtonShowVisuals() {
        if(pointerEntered) {
            HideSelling();
        }
    }

    protected void OnEnable() {
        buttonEnabled = true;
    }
}
