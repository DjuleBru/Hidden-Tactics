using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerActionsManager : NetworkBehaviour {

    public static PlayerActionsManager LocalInstance;

    private Troop troopToSpawn;

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
                if(IsValidTroopSpawningTarget()) {
                    PlaceTroop();
                }
            }
        }
    }

    public void SelectTroopToSpawn(int troopListSOIndex, ulong ownerClientId) {
        currentAction = Action.SelectingTroopToSpawn;
        if (troopToSpawn != null) {
            Destroy(troopToSpawn.gameObject);
            troopToSpawn = null;
        };

        SpawnTroopServerRpc(troopListSOIndex, ownerClientId);
    }

    public bool IsValidTroopSpawningTarget() {
        return true;
    }

    private void PlaceTroop() {
        troopToSpawn.PlaceTroop();
        troopToSpawn = null;
        currentAction = Action.Idle;
    }


    [ServerRpc(RequireOwnership = false)]
    private void SpawnTroopServerRpc(int troopSOIndex, ulong ownerClientId) {

        TroopSO troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(troopSOIndex);

        GameObject troopToSpawnGameObject = Instantiate(troopToSpawnSO.troopPrefab);

        NetworkObject troopNetworkObject = troopToSpawnGameObject.GetComponent<NetworkObject>();
        troopNetworkObject.Spawn(true);

        SpawnTroopClientRpc(troopNetworkObject, ownerClientId);
    }

    [ClientRpc]
    private void SpawnTroopClientRpc(NetworkObjectReference troopToSpawnNetworkObjectReference, ulong ownerClientId) {
        troopToSpawnNetworkObjectReference.TryGet(out NetworkObject troopToSpawnNetworkObject);
        Troop troopToSpawn = troopToSpawnNetworkObject.GetComponent<Troop>();
        this.troopToSpawn = troopToSpawn;

        troopToSpawn.SetTroopOwnerClientId(ownerClientId);
    }
}
