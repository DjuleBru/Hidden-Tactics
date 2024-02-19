using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerActionsManager : NetworkBehaviour {

    public static PlayerActionsManager LocalInstance;

    private enum Action {
        Idle,
        SelectingTroopToSpawn,
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
        if(currentAction == Action.SelectingTroopToSpawn) {
            if(Input.GetMouseButtonDown(0)) {
                // Player is trying to place troop : check if troop placement conditions are met
                if(PlayerAction_SpawnTroop.LocalInstance.IsValidTroopSpawningTarget()) {
                    PlayerAction_SpawnTroop.LocalInstance.PlaceTroop();
                    currentAction = Action.Idle;
                }
            }
            if (Input.GetMouseButtonDown(1)) {
                // Cancel troop placement
                PlayerAction_SpawnTroop.LocalInstance.CancelTroopPlacement();
                currentAction = Action.Idle;
            }
        }
    }


    public void SelectTroopToSpawn(int troopListSOIndex) {
        currentAction = Action.SelectingTroopToSpawn;
        PlayerAction_SpawnTroop.LocalInstance.SelectTroopToSpawn(troopListSOIndex);
    }

}
