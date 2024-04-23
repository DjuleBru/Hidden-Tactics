using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAction_SelectTroop : NetworkBehaviour {

    public static PlayerAction_SelectTroop LocalInstance;
    private Troop selectedTroop;

    [SerializeField] private LayerMask UILayer;

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
        }
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {

        if (PlayerActionsManager.LocalInstance == null) return;

        if (Input.GetMouseButtonDown(1)) {
            DeselectTroop();
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

            HandleTroopSelection();
        }
    }

    private void HandleTroopSelection() {
        Troop newSelectedTroop = BattleGrid.Instance.GetTroopAtGridPosition(MousePositionManager.Instance.GetMouseGridPosition());

        if (newSelectedTroop != null) {
            // Player clicked on a valid troop

            if (selectedTroop != null) {
                // There was already a troop selected

                if (selectedTroop == newSelectedTroop) {
                    // Player clicked the same troop again to deselect
                    DeselectTroop();
                    selectedTroop = null;
                }
                else {
                    // Player clicked a new troop with a previous troop selected
                    DeselectTroop();
                    selectedTroop = newSelectedTroop;
                    SelectTroop();
                }
            }
            else {
                selectedTroop = newSelectedTroop;
                SelectTroop();
            }
        } else {
            //Player clicked somewhere else 
            DeselectTroop();
        }
    }
    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        // Deselect troop
        DeselectTroop();
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

    public void DeselectTroop() {
        if(selectedTroop != null) {
            selectedTroop.GetTroopUI().HideTroopSelectedUI();
            foreach (Unit unit in selectedTroop.GetUnitInTroopList()) {
                unit.GetUnitVisual().SetUnitSelected(false);
            }
            selectedTroop = null;
        }
    }

    public void SelectTroop() {
        selectedTroop.GetTroopUI().ShowTroopSelectedUI();
        foreach(Unit unit in selectedTroop.GetUnitInTroopList()) {
            unit.GetUnitVisual().SetUnitSelected(true);
        }
    }

    public bool IsTroopSelected(Troop troop) {
        return selectedTroop == troop;
    }
}
