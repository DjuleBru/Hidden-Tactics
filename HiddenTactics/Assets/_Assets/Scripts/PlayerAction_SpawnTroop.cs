using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAction_SpawnTroop : NetworkBehaviour {

    public static PlayerAction_SpawnTroop LocalInstance;

    private Troop troopToSpawn;

    private Dictionary<int, Troop> spawnedTroops;
    private Dictionary<int, GridPosition> spawnedTroopsGridPositions;
    private int troopDictionaryInt;


    private void Awake() {
        spawnedTroops = new Dictionary<int, Troop>();
        spawnedTroopsGridPositions = new Dictionary<int, GridPosition>();
    }
    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
        }
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        // Destroy troop to spawn 
        if (troopToSpawn != null) {
            CancelTroopPlacement();
        };

        troopDictionaryInt = 0;

        if (BattleManager.Instance.IsBattlePhaseStarting()) {
            // Set & Fetch spawned troops data from server

            for (int i = 0; i < spawnedTroops.Count; i++) {
                Troop spawnedTroop = spawnedTroops[i];
                GridPosition spawnedTroopGridPosition = spawnedTroopsGridPositions[i];

                NetworkObjectReference spawnedTroopNetworkObject = spawnedTroop.GetComponent<NetworkObject>();

                SetTroopPositionServerRpc(spawnedTroopNetworkObject, spawnedTroopGridPosition.x, spawnedTroopGridPosition.y);
            }

            spawnedTroops.Clear();
            spawnedTroopsGridPositions.Clear();
        }
    }

    public void SelectTroopToSpawn(int troopListSOIndex) {
        if (troopToSpawn != null) {
            CancelTroopPlacement();
        };

        SpawnTroopServerRpc(troopListSOIndex, NetworkManager.Singleton.LocalClientId);
    }

    public bool IsValidTroopSpawningTarget() {
        GridPosition troopSpawnGridPosition = MousePositionManager.Instance.GetMouseGridPosition();
        return BattleGrid.Instance.IsValidPlayerGridPosition(troopSpawnGridPosition);
    }

    public void PlaceTroop() {
        troopToSpawn.PlaceTroop();
        spawnedTroops.Add(troopDictionaryInt, troopToSpawn);
        spawnedTroopsGridPositions.Add(troopDictionaryInt, troopToSpawn.GetTroopGridPosition());
        troopDictionaryInt++;

        troopToSpawn = null;
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
    private void SetTroopPositionServerRpc(NetworkObjectReference troopNetworkObjectReference, int gridPositionx, int gridPositiony) {
        SetTroopPositionClientRpc(troopNetworkObjectReference, gridPositionx, gridPositiony);
    }

    [ClientRpc]
    private void SetTroopPositionClientRpc(NetworkObjectReference troopNetworkObjectReference, int gridPositionx, int gridPositiony) {
        troopNetworkObjectReference.TryGet(out NetworkObject troopToSpawnNetworkObject);
        Troop troopSpawned = troopToSpawnNetworkObject.GetComponent<Troop>();

        GridPosition spawnedTroopGridPosition = new GridPosition(gridPositionx, gridPositiony);

        if (!troopSpawned.IsOwnedByPlayer()) {
            spawnedTroopGridPosition = BattleGrid.Instance.TranslateOpponentGridPosition(spawnedTroopGridPosition);
        }

        troopSpawned.SetTroopGridPosition(spawnedTroopGridPosition);
        troopSpawned.PlaceTroop();
    }


    public void CancelTroopPlacement() {
        NetworkObjectReference troopNetworkObjectReference = troopToSpawn.GetComponent<NetworkObject>();
        HiddenTacticsMultiplayer.Instance.DestroyTroop(troopNetworkObjectReference);
        troopToSpawn = null;
    }

    public Troop GetTroopToSpawn() {
        return troopToSpawn;
    }

}
