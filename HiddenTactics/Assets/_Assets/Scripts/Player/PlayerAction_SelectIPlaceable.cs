using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAction_SelectIPlaceable : NetworkBehaviour {

    public static PlayerAction_SelectIPlaceable LocalInstance;
    private Troop selectedTroop;
    private Building selectedBuilding;

    [SerializeField] private LayerMask UILayer;

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
        }
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {
        if (LocalInstance != this) return;

        if (Input.GetMouseButtonDown(1)) {
            DeselectIPlaceable();
        }

        // Do not allow troop selection is player is not in idle state
        if (PlayerActionsManager.LocalInstance.GetCurrentAction() != PlayerActionsManager.Action.Idle) return;

        //Do not allow troop selection during another phase than the preparation phase
        if (!BattleManager.Instance.IsPreparationPhase()) return;

        //Do not allow troop selection if pointer is over a UI game object
        if (EventSystem.current.IsPointerOverGameObject()) {
            if(PointerIsOverUIElement()) return;
        }

        if (Input.GetMouseButtonDown(0)) {
            if (!BattleGrid.Instance.IsValidGridPosition(MousePositionManager.Instance.GetMouseGridPosition())) return;
            // Player is clicking a valid grid position

            HandleIPlaceableSelection();
        }
    }

    private void HandleIPlaceableSelection() {
        Troop newSelectedTroop = BattleGrid.Instance.GetTroopAtGridPosition(MousePositionManager.Instance.GetMouseGridPosition());
        Building newSelectedBuilding = BattleGrid.Instance.GetBuildingAtGridPosition(MousePositionManager.Instance.GetMouseGridPosition());

        if (newSelectedTroop != null || newSelectedBuilding != null) {
            // Player clicked on a valid placeable

            if (selectedTroop != null || selectedBuilding != null) {
                // There was already a placeable selected

                if ((selectedTroop != null && selectedTroop == newSelectedTroop) || (selectedBuilding != null && selectedBuilding == newSelectedBuilding)) {
                    // Player clicked the same placeable again to deselect
                    DeselectIPlaceable();
                } else {
                    // Player clicked a new placeable with a previous troop selected
                    DeselectIPlaceable();
                    selectedTroop = newSelectedTroop;
                    selectedBuilding = newSelectedBuilding;
                    SelectIPlaceable();
                }
            } else {
                selectedTroop = newSelectedTroop;
                selectedBuilding = newSelectedBuilding;
                SelectIPlaceable();
            }
        } else {
            //Player clicked somewhere else 
            DeselectIPlaceable();
        }
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        // Deselect troop
        DeselectIPlaceable();
    }

    private bool PointerIsOverUIElement() {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        if (raycastResults.Count > 0) {
            foreach (var go in raycastResults) {
                int layerMask = 1 << go.gameObject.layer;
                if (layerMask == UILayer.value) {
                    return true;
                };
            }
        }

        return false;
    }

    public void DeselectIPlaceable() {
        if(selectedTroop != null) {
            selectedTroop.SetTroopSelected(false);
            selectedTroop = null;
        }

        if (selectedBuilding != null) {
            selectedBuilding.SetBuildingSelected(false);
            selectedBuilding = null;
        }
    }

    public void SelectIPlaceable() {
        if(selectedTroop != null) {
            selectedTroop.SetTroopSelected(true);
        }
        if(selectedBuilding != null) {
            selectedBuilding.SetBuildingSelected(true);
        }
    }

    public void SelectTroop(Troop troop) {

        if (selectedTroop != null) {
            selectedTroop.SetTroopSelected(false);
        }

        troop.SetTroopSelected(true);
        selectedTroop = troop;
    }

    public void SelectBuilding(Building building) {

        if (selectedBuilding != null) {
            selectedBuilding.SetBuildingSelected(false);
        }

        building.SetBuildingSelected(true);
        selectedBuilding = building;
    }

    public bool IsTroopSelected(Troop troop) {
        return selectedTroop == troop;
    }

    public bool IsBuildingSelected(Building building) {
        return selectedBuilding == building;
    }
}
