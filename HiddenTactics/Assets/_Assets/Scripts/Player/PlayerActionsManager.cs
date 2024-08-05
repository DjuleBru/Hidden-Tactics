using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class PlayerActionsManager : NetworkBehaviour {

    public static PlayerActionsManager LocalInstance;
    private int troopSOIndexBeingSpawned;
    private int buildingSOIndexBeingSpawned;

    private bool placingTroop;
    private bool placingBuilding;

    public event EventHandler OnActionChanged;

    public enum Action {
        Idle,
        SelectingIPlaceableToSpawn,
    }

    private Action currentAction;

    public override void OnNetworkSpawn() {
        if(IsOwner) {
            LocalInstance = this;
            GridHoverManager.Instance.SubsribeToPlayerActions();
        }
    }

    private void Update() {
        
        switch(currentAction) {
            case Action.Idle:
                break;
            case Action.SelectingIPlaceableToSpawn:

                if (Input.GetMouseButtonDown(0)) {
                    // Player is trying to place troop : check if troop placement conditions are met
                    if (PlayerAction_SpawnIPlaceable.LocalInstance.IsMousePositionValidIPlaceableSpawningTarget()) {
                        PlayerAction_SpawnIPlaceable.LocalInstance.PlaceIPlaceableList();
                        PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
                        ChangeAction(Action.Idle);
                    }
                }

                if (Input.GetMouseButtonDown(1)) {
                    PlayerAction_SpawnIPlaceable.LocalInstance.CancelIPlaceablePlacement();
                    PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
                    ChangeAction(Action.Idle);
                }
                break;
        }
    }

    private IEnumerator ChangeActionAtFrameEnd(Action action) {
        yield return new WaitForEndOfFrame();
        ChangeAction(action);
    }

    private void ChangeAction(Action newAction) {
        if(newAction == Action.SelectingIPlaceableToSpawn) {
            // Deselect any selected troop
            PlayerAction_SelectIPlaceable.LocalInstance.DeselectIPlaceable();
        }

        if(newAction == Action.Idle) {
            // Cancel troop placement
            PlayerAction_SpawnIPlaceable.LocalInstance.CancelIPlaceablePlacement();
        }

        currentAction = newAction;
        OnActionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SelectTroopToSpawn(int troopListSOIndex) {
        ChangeAction(Action.SelectingIPlaceableToSpawn);
        troopSOIndexBeingSpawned = troopListSOIndex;
        PlayerAction_SpawnIPlaceable.LocalInstance.SelectTroopToSpawn(troopListSOIndex);
    }

    public void SelectBuildingToSpawn(int buildingListSOIndex) {
        ChangeAction(Action.SelectingIPlaceableToSpawn);
        buildingSOIndexBeingSpawned = buildingListSOIndex;
        PlayerAction_SpawnIPlaceable.LocalInstance.SelectBuildingToSpawn(buildingListSOIndex);
    }

    public Action GetCurrentAction() {
        return currentAction;
    }

    public int GetTroopSOIndexBeingSpawned() {
        return troopSOIndexBeingSpawned;
    }
    public int GetBuildingSOIndexBeingSpawned() {
        return buildingSOIndexBeingSpawned;
    }
}
