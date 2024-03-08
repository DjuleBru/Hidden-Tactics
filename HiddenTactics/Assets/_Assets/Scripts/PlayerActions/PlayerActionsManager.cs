using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerActionsManager : NetworkBehaviour {

    public static PlayerActionsManager LocalInstance;

    public enum Action {
        Idle,
        SelectingIPlaceableToSpawn,
    }

    private Action currentAction;

    public override void OnNetworkSpawn() {
        if(IsOwner) {
            LocalInstance = this;
        }
    }

    private void Update() {
        
        switch(currentAction) {
            case Action.Idle:
                break;
            case Action.SelectingIPlaceableToSpawn:

                if (Input.GetMouseButtonDown(0)) {
                    // Player is trying to place troop : check if troop placement conditions are met
                    if (PlayerAction_SpawnTroop.LocalInstance.IsValidIPlaceableSpawningTarget()) {
                        PlayerAction_SpawnTroop.LocalInstance.PlaceIPlaceableList();
                        currentAction = Action.Idle;
                    }
                }

                if (Input.GetMouseButtonDown(1)) {
                    ChangeAction(Action.Idle);
                }
                break;

        }
    }

    private void ChangeAction(Action newAction) {
        if(newAction == Action.SelectingIPlaceableToSpawn) {
            // Deselect any selected troop
            PlayerAction_SelectTroop.LocalInstance.DeselectTroop();
        }

        if(newAction == Action.Idle) {
            // Cancel troop placement
            PlayerAction_SpawnTroop.LocalInstance.CancelIPlaceablePlacement();
        }

        currentAction = newAction;
    }

    public void SelectTroopToSpawn(int troopListSOIndex) {
        ChangeAction(Action.SelectingIPlaceableToSpawn);
        PlayerAction_SpawnTroop.LocalInstance.SelectTroopToSpawn(troopListSOIndex);
    }

    public void SelectBuildingToSpawn(int buildingListSOIndex) {
        ChangeAction(Action.SelectingIPlaceableToSpawn);
        PlayerAction_SpawnTroop.LocalInstance.SelectBuildingToSpawn(buildingListSOIndex);
    }

    public Action GetCurrentAction() {
        return currentAction;
    }

}
