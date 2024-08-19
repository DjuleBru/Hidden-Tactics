using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpawnIPlaceableButtonDebug : SpawnIPlaceableButton, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected TroopSO debugTroopToSpawnSO;
    [SerializeField] protected BuildingSO debugBuildingToSpawnSO;

    protected bool pointerEntered;
    protected bool cardOpen;
    public static bool lastHoveredItemTemplateUI;

    protected void Start() {
        spawnIPlaceableButton = GetComponent<Button>();
        GameInput.Instance.OnLeftClickPerformed += GameInput_OnLeftClickPerformed;
        GameInput.Instance.OnRightClickPerformed += GameInput_OnRightClickPerformed;

        spawnIPlaceableButton.onClick.AddListener(() => {
            if(debugTroopToSpawnSO != null) {
                SpawnTroopButton();
            }
            if(debugBuildingToSpawnSO != null) {
                SpawnBuildingButton();
            }
        });
    }

    private void Instance_OnRightClickPerformed(object sender, System.EventArgs e) {
        throw new System.NotImplementedException();
    }

    protected override void SpawnTroopButton() {
        int troopIndex = BattleDataManager.Instance.GetTroopSOIndex(debugTroopToSpawnSO);

        PlayerActionsManager.LocalInstance.SelectTroopToSpawn(troopIndex);
    }

    protected override void SpawnBuildingButton() {
        int buildingIndex = BattleDataManager.Instance.GetBuildingSOIndex(debugBuildingToSpawnSO);
        PlayerActionsManager.LocalInstance.SelectBuildingToSpawn(buildingIndex);
    }

    private void GameInput_OnLeftClickPerformed(object sender, System.EventArgs e) {
        if (!pointerEntered && cardOpen && lastHoveredItemTemplateUI == this) {
            cardOpen = false;
            IPlaceableDescriptionSlotTemplate.Instance.Hide();
        }
    }

    private void GameInput_OnRightClickPerformed(object sender, System.EventArgs e) {
        if (pointerEntered) {
            if (debugTroopToSpawnSO != null) {
                cardOpen = true;
                IPlaceableDescriptionSlotTemplate.Instance.Show();
                IPlaceableDescriptionSlotTemplate.Instance.SetDescriptionSlot(debugTroopToSpawnSO, debugTroopToSpawnSO.unitPrefab.GetComponent<Unit>().GetUnitSO());
            }
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        pointerEntered = true;
        lastHoveredItemTemplateUI = this;

        if (cardOpen) {
            if (debugTroopToSpawnSO != null) {
                IPlaceableDescriptionSlotTemplate.Instance.SetDescriptionSlot(debugTroopToSpawnSO, debugTroopToSpawnSO.unitPrefab.GetComponent<Unit>().GetUnitSO());
            }
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        pointerEntered = false;
    }
}
