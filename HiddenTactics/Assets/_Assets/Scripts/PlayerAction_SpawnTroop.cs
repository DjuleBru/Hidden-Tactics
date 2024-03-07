using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAction_SpawnTroop : NetworkBehaviour {

    public static PlayerAction_SpawnTroop LocalInstance;

    private List<IPlaceable> iPlaceableToSpawnList;

    private Dictionary<int, IPlaceable> spawnedIPlaceablesDictionary;
    private Dictionary<int, GridPosition> spawnedIPlaceableGridPositions;
    private int troopDictionaryInt;


    private void Awake() {
        spawnedIPlaceablesDictionary = new Dictionary<int, IPlaceable>();
        spawnedIPlaceableGridPositions = new Dictionary<int, GridPosition>();

        iPlaceableToSpawnList = new List<IPlaceable>();
    }

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
        }
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        // Destroy troop to spawn 
        if (iPlaceableToSpawnList.Count != 0) {
            CancelIPlaceablePlacement();
        };

        troopDictionaryInt = 0;

        if (BattleManager.Instance.IsBattlePhaseStarting()) {
            // Set & Fetch spawned troops data from server

            for (int i = 0; i < spawnedIPlaceablesDictionary.Count; i++) {
                IPlaceable spawnedIPLaceable = spawnedIPlaceablesDictionary[i];
                GridPosition spawnedIPlaceableGridPosition = spawnedIPlaceableGridPositions[i];

                NetworkObjectReference spawnedIPlaceableNetworkObject = (spawnedIPLaceable as MonoBehaviour).GetComponent<NetworkObject>();

                SetIPlaceablePositionServerRpc(spawnedIPlaceableNetworkObject, spawnedIPlaceableGridPosition.x, spawnedIPlaceableGridPosition.y);
            }

            spawnedIPlaceablesDictionary.Clear();
            spawnedIPlaceableGridPositions.Clear();
        }
    }

    public void SelectTroopToSpawn(int troopListSOIndex) {
        if (iPlaceableToSpawnList != null) {
            CancelIPlaceablePlacement();
        };

        SpawnTroopServerRpc(troopListSOIndex, NetworkManager.Singleton.LocalClientId);
    }

    public void SelectBuildingToSpawn(int buildingListSOIndex) {
        if (iPlaceableToSpawnList != null) {
            CancelIPlaceablePlacement();
        };

        SpawnBuildingServerRpc(buildingListSOIndex, NetworkManager.Singleton.LocalClientId);
    }

    public bool IsValidIPlaceableSpawningTarget() {
        GridPosition iPlaceableSpawnGridPosition = MousePositionManager.Instance.GetMouseGridPosition();
        return BattleGrid.Instance.IsValidPlayerGridPosition(iPlaceableSpawnGridPosition);
    }

    public void PlaceIPlaceableList() {
        foreach(IPlaceable iPlaceable in iPlaceableToSpawnList) {

            iPlaceable.PlaceIPlaceable();
            spawnedIPlaceablesDictionary.Add(troopDictionaryInt, iPlaceable);
            spawnedIPlaceableGridPositions.Add(troopDictionaryInt, iPlaceable.GetIPlaceableGridPosition());
            troopDictionaryInt++;
        }
        iPlaceableToSpawnList = new List<IPlaceable>();
    }

    public void CancelIPlaceablePlacement() {
        foreach (IPlaceable iPlaceable in iPlaceableToSpawnList) {
            NetworkObjectReference iPlaceableNetworkObjectReference = (iPlaceable as MonoBehaviour).GetComponent<NetworkObject>();
            HiddenTacticsMultiplayer.Instance.DestroyIPlaceable(iPlaceableNetworkObjectReference);
        }
        iPlaceableToSpawnList = new List<IPlaceable>();
    }

    #region SPAWN TROOPS, UNITS AND BUILDINGS
    [ServerRpc(RequireOwnership = false)]
    private void SpawnTroopServerRpc(int troopSOIndex, ulong ownerClientId) {

        TroopSO troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(troopSOIndex);
        GameObject troopToSpawnGameObject = Instantiate(troopToSpawnSO.troopPrefab);

        NetworkObject troopNetworkObject = troopToSpawnGameObject.GetComponent<NetworkObject>();
        troopNetworkObject.Spawn(true);

        SpawnIPlaceableClientRpc(troopNetworkObject, ownerClientId);

        // Spawn units
        Troop troopToSpawnTroop = troopToSpawnGameObject.GetComponent<Troop>();
        List<Transform> unitsToSpawnBasePositions = troopToSpawnTroop.GetBaseUnitPositions();
        List<Transform> unitsToSpawnAdditionalPositions = troopToSpawnTroop.GetAdditionalUnitPositions();

        SpawnUnits(troopToSpawnGameObject, unitsToSpawnBasePositions, false);
        SpawnUnits(troopToSpawnGameObject, unitsToSpawnAdditionalPositions, true);
    }


    [ServerRpc(RequireOwnership = false)]
    private void SpawnBuildingServerRpc(int buildingSOIndex, ulong ownerClientId) {

        BuildingSO buildingToSpawnSO = BattleDataManager.Instance.GetBuildingSOFromIndex(buildingSOIndex);
        GameObject buildingToSpawnGameObject = Instantiate(buildingToSpawnSO.buildingPrefab);

        NetworkObject buildingNetworkObject = buildingToSpawnGameObject.GetComponent<NetworkObject>();
        buildingNetworkObject.Spawn(true);

        SpawnIPlaceableClientRpc(buildingNetworkObject, ownerClientId);

        if(buildingToSpawnSO.hasGarrisonedTroop) {
            // Spawn garrisoned troop
            int troopIndex = BattleDataManager.Instance.GetTroopSOIndex(buildingToSpawnSO.garrisonedTroopSO);
            SpawnTroopServerRpc(troopIndex, NetworkManager.Singleton.LocalClientId);
        }

    }

    [ClientRpc]
    private void SpawnIPlaceableClientRpc(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference, ulong ownerClientId) {
        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();

        if (ownerClientId == NetworkManager.Singleton.LocalClientId) {
            iPlaceableToSpawnList.Add(iPlaceableToSpawn);
        }

        iPlaceableToSpawn.SetIPlaceableOwnerClientId(ownerClientId);
    }

    private List<NetworkObject> SpawnUnits(NetworkObjectReference troopToSpawnNetworkObjectReference, List<Transform> unitsToSpawnPositionList, bool isAdditionalUnits) {
        troopToSpawnNetworkObjectReference.TryGet(out NetworkObject troopToSpawnNetworkObject);
        Troop troopToSpawnTroop = troopToSpawnNetworkObject.GetComponent<Troop>();
        GameObject troopToSpawnGameObject = troopToSpawnTroop.gameObject;
        TroopSO troopToSpawnSO = troopToSpawnTroop.GetTroopSO();

        List<NetworkObject> unitsSpawnedNetworkObjectList = new List<NetworkObject>();

        foreach (Transform unitPositionTransform in unitsToSpawnPositionList) {

            GameObject unitToSpawnPrefab = Instantiate(troopToSpawnSO.unitPrefab);
            NetworkObject unitNetworkObject = unitToSpawnPrefab.GetComponent<NetworkObject>();
            unitNetworkObject.Spawn(true);
            unitNetworkObject.TrySetParent(troopToSpawnGameObject, true);
            SetUnitInitialConditionsClientRpc(unitNetworkObject, troopToSpawnNetworkObject, unitPositionTransform.position, isAdditionalUnits);

            unitsSpawnedNetworkObjectList.Add(unitNetworkObject);
        }

        return unitsSpawnedNetworkObjectList;
    }
    #endregion

    #region SET TROOP, UNIT AND BUILDINGS CONDITIONS
    [ClientRpc]
    private void SetUnitInitialConditionsClientRpc(NetworkObjectReference unitToSpawnNetworkObjectReference, NetworkObjectReference troopSpawnedNetworkObjectReference, Vector3 unitPosition, bool isAdditionalUnit) {
        unitToSpawnNetworkObjectReference.TryGet(out NetworkObject unitSpawnedNetworkObject);
        Unit unitSpawned = unitSpawnedNetworkObject.GetComponent<Unit>();

        troopSpawnedNetworkObjectReference.TryGet(out NetworkObject troopToSpawnNetworkObject);
        Troop troopToSpawnTroop = troopToSpawnNetworkObject.GetComponent<Troop>();

        //Set Parent Troop
        unitSpawned.SetParentTroop(troopToSpawnTroop);

        //Set Unit Local Position
        unitSpawned.SetPosition(unitPosition);

        //Set Parent As addional Unit
        if (isAdditionalUnit) {
            unitSpawned.SetUnitAsAdditionalUnit();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetIPlaceablePositionServerRpc(NetworkObjectReference IPlaceableNetworkObjectReference, int gridPositionx, int gridPositiony) {
        SetIPlaceablePositionClientRpc(IPlaceableNetworkObjectReference, gridPositionx, gridPositiony);
    }

    [ClientRpc]
    private void SetIPlaceablePositionClientRpc(NetworkObjectReference IPlaceableNetworkObjectReference, int gridPositionx, int gridPositiony) {
        IPlaceableNetworkObjectReference.TryGet(out NetworkObject IPlaceableToSpawnNetworkObject);

        IPlaceable iPlaceableSpawned = IPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();

        GridPosition spawnedIPlaceableGridPosition = new GridPosition(gridPositionx, gridPositiony);

        if (!iPlaceableSpawned.IsOwnedByPlayer()) {
            spawnedIPlaceableGridPosition = BattleGrid.Instance.TranslateOpponentGridPosition(spawnedIPlaceableGridPosition);
        }

        iPlaceableSpawned.SetIPlaceableGridPosition(spawnedIPlaceableGridPosition);
        iPlaceableSpawned.PlaceIPlaceable();
    }
    #endregion

}
