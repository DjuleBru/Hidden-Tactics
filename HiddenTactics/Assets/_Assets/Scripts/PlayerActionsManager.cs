using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerActionsManager : NetworkBehaviour {

    public static PlayerActionsManager LocalInstance;

    private Troop troopToSpawn;

    private Dictionary<int, Troop> spawnedTroops;
    private Dictionary<int, Vector3> spawnedTroopPositions;

    private int troopDictionaryInt;
    private enum Action {
        Idle,
        SelectingTroopToSpawn,
        PreparationPhase,
        BattlePhase,
    }

    private Action currentAction;

    private void Awake() {
        spawnedTroops = new Dictionary<int, Troop>();
        spawnedTroopPositions = new Dictionary<int, Vector3>();
    }

    public override void OnNetworkSpawn() {
        if(IsOwner) {
            LocalInstance = this;
        }
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }


    private void Update() {
        if(currentAction == Action.SelectingTroopToSpawn) {
            if(Input.GetMouseButtonDown(0)) {
                // Player is trying to place troop : check if troop placement conditions are met
                if(IsValidTroopSpawningTarget()) {
                    PlaceTroop();
                }
            }
            if (Input.GetMouseButtonDown(1)) {
                // Cancel troop placement
                Destroy(troopToSpawn.gameObject);
                currentAction = Action.Idle;
            }
        }
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        troopToSpawn = null;
        troopDictionaryInt = 0;

        if (BattleManager.Instance.IsBattlePhase()) {
            // Set & Fetch spawned troops data from server

            for(int i = 0; i < spawnedTroops.Count; i++) {
                Troop spawnedTroop = spawnedTroops[i];
                Vector3 spawnedTroopPosition = spawnedTroopPositions[i];

                NetworkObjectReference spawnedTroopNetworkObject = spawnedTroop.GetComponent<NetworkObject>();

                SetTroopPositionServerRpc(spawnedTroopNetworkObject, spawnedTroopPosition);
            }
            spawnedTroops.Clear();
            spawnedTroopPositions.Clear();
        }
    }

    public void SelectTroopToSpawn(int troopListSOIndex) {
        currentAction = Action.SelectingTroopToSpawn;
        if (troopToSpawn != null) {
            Destroy(troopToSpawn.gameObject);
            troopToSpawn = null;
        };

        Debug.Log(NetworkManager.Singleton.LocalClientId);
        SpawnTroopServerRpc(troopListSOIndex, NetworkManager.Singleton.LocalClientId);
    }

    public bool IsValidTroopSpawningTarget() {
        return true;
    }

    private void PlaceTroop() {

        troopToSpawn.PlaceTroop();
        spawnedTroops.Add(troopDictionaryInt, troopToSpawn);
        spawnedTroopPositions.Add(troopDictionaryInt, troopToSpawn.transform.position);
        troopDictionaryInt++;

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

        if (ownerClientId == NetworkManager.Singleton.LocalClientId) {
            this.troopToSpawn = troopToSpawn;
        }

        troopToSpawn.SetTroopOwnerClientId(ownerClientId);
    }


    [ServerRpc(RequireOwnership = false)]
    private void SetTroopPositionServerRpc(NetworkObjectReference troopNetworkObjectReference, Vector3 troopPosition) {
        SetTroopPositionClientRpc(troopNetworkObjectReference, troopPosition);
    }

    [ClientRpc]
    private void SetTroopPositionClientRpc(NetworkObjectReference troopNetworkObjectReference, Vector3 troopPosition) {
        troopNetworkObjectReference.TryGet(out NetworkObject troopToSpawnNetworkObject);
        Troop troopSpawned = troopToSpawnNetworkObject.GetComponent<Troop>();

        troopSpawned.SetTroopPosition(troopPosition);
        troopSpawned.PlaceTroop();
    }

    public Troop GetTroopToSpawn() {
        return troopToSpawn;
    }

}
