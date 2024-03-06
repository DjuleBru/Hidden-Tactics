using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerActionsManager : NetworkBehaviour {

    public static PlayerActionsManager LocalInstance;

    private enum Action {
        Idle,
        SelectingIPlaceableToSpawn,
        PreparationPhase,
        BattlePhase,
    }

    private Action currentAction;

    public override void OnNetworkSpawn() {
        if(IsOwner) {
            LocalInstance = this;
        }
    }

    private void Update() {
        if(currentAction == Action.SelectingIPlaceableToSpawn) {
            if(Input.GetMouseButtonDown(0)) {
                // Player is trying to place troop : check if troop placement conditions are met
                if(PlayerAction_SpawnTroop.LocalInstance.IsValidIPlaceableSpawningTarget()) {
                    PlayerAction_SpawnTroop.LocalInstance.PlaceIPlaceable();
                    currentAction = Action.Idle;
                }
            }
            if (Input.GetMouseButtonDown(1)) {
                // Cancel troop placement
                PlayerAction_SpawnTroop.LocalInstance.CancelIPlaceablePlacement();
                currentAction = Action.Idle;
            }
        }
    }


    public void SelectTroopToSpawn(int troopListSOIndex) {
        currentAction = Action.SelectingIPlaceableToSpawn;
        PlayerAction_SpawnTroop.LocalInstance.SelectTroopToSpawn(troopListSOIndex);
    }

    public void SelectBuildingToSpawn(int buildingListSOIndex) {
        currentAction = Action.SelectingIPlaceableToSpawn;
        PlayerAction_SpawnTroop.LocalInstance.SelectBuildingToSpawn(buildingListSOIndex);
    }

}
