using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerAction_SelectTroop : NetworkBehaviour {

    public static PlayerAction_SelectTroop LocalInstance;
    private Troop selectedTroop;

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
        }
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {
        // Do not allow troop selection is player is not in idle state
        if (PlayerActionsManager.LocalInstance.GetCurrentAction() != PlayerActionsManager.Action.Idle) return;

        if (Input.GetMouseButtonDown(1)) {
            CancelTroopSelection();
        }

        if (Input.GetMouseButtonDown(0)) {
            if (!BattleGrid.Instance.IsValidGridPosition(MousePositionManager.Instance.GetMouseGridPosition())) return;
            // Player attempts click on valid grid position
            HandleTroopSelection();
        }
    }

    private void CancelTroopSelection() {
        if (selectedTroop != null) {
            selectedTroop.GetTroopUI().HideTroopSelectedUI();
            selectedTroop = null;
        }
    }

    private void HandleTroopSelection() {

        if (selectedTroop != null) {
            // Deselect previously selected troop
            selectedTroop.GetTroopUI().HideTroopSelectedUI();
        }

        Troop newSelectedTroop = BattleGrid.Instance.GetTroopAtGridPosition(MousePositionManager.Instance.GetMouseGridPosition());
        if (newSelectedTroop != null) {
            // Player clicked on a valid troop

            if (selectedTroop != null) {
                // There was already a troop selected
                if (selectedTroop == newSelectedTroop) {
                    // Player clicked the same troop again to deselect
                    selectedTroop.GetTroopUI().HideTroopSelectedUI();
                    selectedTroop = null;
                }
                else {
                    // Player clicked a new troop with a previous troop selected
                    selectedTroop = newSelectedTroop;
                    selectedTroop.GetTroopUI().ShowTroopSelectedUI();
                }
            }
            else {
                selectedTroop = newSelectedTroop;
                selectedTroop.GetTroopUI().ShowTroopSelectedUI();
            }
        }
    }
    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        // Deselect troop
    }

    public void DeselectTroop() {
        if(selectedTroop != null) {
            selectedTroop.GetTroopUI().HideTroopSelectedUI();
        }
    }

}
