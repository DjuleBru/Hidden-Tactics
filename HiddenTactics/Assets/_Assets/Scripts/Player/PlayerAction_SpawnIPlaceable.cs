using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerAction_SpawnIPlaceable : NetworkBehaviour {

    public static PlayerAction_SpawnIPlaceable LocalInstance;

    private List<IPlaceable> iPlaceableToPlaceList;
    private List<IPlaceable> iPlaceablePoolList;

    private Dictionary<int, IPlaceable> spawnedOpponentIPlaceablesDictionary;
    private Dictionary<int, GridPosition> spawnedOpponentIPlaceableGridPositions;
    private int troopDictionaryInt;


    private void Awake() {
        spawnedOpponentIPlaceablesDictionary = new Dictionary<int, IPlaceable>();
        spawnedOpponentIPlaceableGridPositions = new Dictionary<int, GridPosition>();

        iPlaceableToPlaceList = new List<IPlaceable>();
        iPlaceablePoolList = new List<IPlaceable>();
    }

    public override void OnNetworkSpawn() {

        if (IsOwner) {
            LocalInstance = this;
        }

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }


    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        // Destroy troop to spawn 
        if (iPlaceableToPlaceList.Count != 0) {
            CancelIPlaceablePlacement();
        };

        troopDictionaryInt = 0;

        if (BattleManager.Instance.IsBattlePhaseStarting()) {
            // Set & Fetch spawned troops data from server

            Debug.Log("spawnedOpponentIPlaceablesDictionary.Count " + spawnedOpponentIPlaceablesDictionary.Count);

            for (int i = 0; i < spawnedOpponentIPlaceablesDictionary.Count; i++) {
                IPlaceable spawnedIPLaceable = spawnedOpponentIPlaceablesDictionary[i];
                GridPosition spawnedIPlaceableGridPosition = spawnedOpponentIPlaceableGridPositions[i];

                if((spawnedIPLaceable as MonoBehaviour) != null) {
                    NetworkObjectReference spawnedIPlaceableNetworkObject = (spawnedIPLaceable as MonoBehaviour).GetComponent<NetworkObject>();
                    SetOpponentIPlaceablePositionServerRpc(spawnedIPlaceableNetworkObject, spawnedIPlaceableGridPosition.x, spawnedIPlaceableGridPosition.y);
                }
            }

            spawnedOpponentIPlaceablesDictionary.Clear();
            spawnedOpponentIPlaceableGridPositions.Clear();
        }

    }

    public void SelectTroopToSpawn(int troopListSOIndex) {
        if (iPlaceableToPlaceList != null) {
            CancelIPlaceablePlacement();
        };

        TroopSO troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(troopListSOIndex);
        PlayerStateUI.Instance.SetPlayerGoldChangingUI(-troopToSpawnSO.spawnTroopCost);

        ActivatePooledTroop(troopListSOIndex, NetworkManager.Singleton.LocalClientId);
    }

    public void SelectBuildingToSpawn(int buildingListSOIndex) {
        if (iPlaceableToPlaceList != null) {
            CancelIPlaceablePlacement();
        };

        BuildingSO buildingToSpawnSO = BattleDataManager.Instance.GetBuildingSOFromIndex(buildingListSOIndex);
        PlayerStateUI.Instance.SetPlayerGoldChangingUI(-buildingToSpawnSO.spawnBuildingCost);

        SpawnBuildingServerRpc(buildingListSOIndex, NetworkManager.Singleton.LocalClientId);
    }

    public bool IsMousePositionValidIPlaceableSpawningTarget() {
        GridPosition iPlaceableSpawnGridPosition = MousePositionManager.Instance.GetMouseGridPosition();

        // If it is a player grid position AND nothing is on the grid position
        return (BattleGrid.Instance.IsValidPlayerGridPosition(iPlaceableSpawnGridPosition) && BattleGrid.Instance.GetIPlaceableSpawnedAtGridPosition(iPlaceableSpawnGridPosition) == null && iPlaceableSpawnGridPosition.x != 0);
    }

    public bool IsValidIPlaceableSpawningTarget(GridPosition gridPosition) {
        // If it is a player grid position AND nothing is on the grid position
        return (BattleGrid.Instance.IsValidPlayerGridPosition(gridPosition) && BattleGrid.Instance.GetIPlaceableSpawnedAtGridPosition(gridPosition) == null && gridPosition.x != 0);
    }

    public void PlaceIPlaceableList() {

        foreach(IPlaceable iPlaceable in iPlaceableToPlaceList) {

            iPlaceable.PlaceIPlaceable();

            if (iPlaceable is Building) {
                Building buildingSpawned = (Building)iPlaceable;
                PlayerGoldManager.Instance.SpendGold(buildingSpawned.GetBuildingSO().spawnBuildingCost, NetworkManager.Singleton.LocalClientId);

                NetworkObject troopNetworkObject = buildingSpawned.GetComponent<NetworkObject>();
                AddIPlaceableToOpponentDictionaryServerRpc(troopNetworkObject, NetworkManager.Singleton.LocalClientId, buildingSpawned.GetIPlaceableGridPosition().x, buildingSpawned.GetIPlaceableGridPosition().y);
            }

            if (iPlaceable is Troop) {
                Troop troopSpawned = (Troop)iPlaceable;
                PlayerGoldManager.Instance.SpendGold(troopSpawned.GetTroopSO().spawnTroopCost, NetworkManager.Singleton.LocalClientId);

                NetworkObject troopNetworkObject = troopSpawned.GetComponent<NetworkObject>();
                AddIPlaceableToOpponentDictionaryServerRpc(troopNetworkObject, NetworkManager.Singleton.LocalClientId, troopSpawned.GetIPlaceableGridPosition().x, troopSpawned.GetIPlaceableGridPosition().y);
            }
        }

        iPlaceableToPlaceList = new List<IPlaceable>();
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIPlaceableToOpponentDictionaryServerRpc(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference, ulong ownerClientId, int gridPositionx, int gridPositiony) {
        AddIPlaceableToOpponentDictionaryClientRpc(iPlaceableToSpawnNetworkObjectReference, ownerClientId, gridPositionx, gridPositiony);
    }

    [ClientRpc]
    private void AddIPlaceableToOpponentDictionaryClientRpc(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference, ulong ownerClientId, int gridPositionx, int gridPositiony) {
        
        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceable = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();
        GridPosition spawnedIPlaceableGridPosition = new GridPosition(gridPositionx, gridPositiony);

        if (ownerClientId != NetworkManager.Singleton.LocalClientId) {
            spawnedOpponentIPlaceablesDictionary.Add(troopDictionaryInt, iPlaceable);
            spawnedOpponentIPlaceableGridPositions.Add(troopDictionaryInt, spawnedIPlaceableGridPosition);
            troopDictionaryInt++;

            Debug.Log("added to opponent dictionary");
        }
    }

    public void CancelIPlaceablePlacement() {

        foreach (IPlaceable iPlaceable in iPlaceableToPlaceList) {
            iPlaceable.SetAsPooledIPlaceable();
        }

        iPlaceableToPlaceList = new List<IPlaceable>();

        PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
    }


    #region OBJECT POOLING

    public void SpawnPlayerPoolTroopList(ulong clientID, Deck deck) {

        foreach (TroopSO troopSO in deck.troopsInDeck) {
            if(troopSO == null) continue;
            int troopSOIndex = BattleDataManager.Instance.GetTroopSOIndex(troopSO);
            SpawnTroopServerRpc(troopSOIndex, clientID);
        }

        iPlaceableToPlaceList.Clear();
    }

    public void ActivatePooledTroop(int troopSOIndex, ulong ownerClientId) {
        int iPlaceableOfTypeNumber = 0;
        TroopSO troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(troopSOIndex);


        // First check how many iplaceables of the same type in the pool list
        foreach (IPlaceable iPlaceable in iPlaceablePoolList) {
            if (iPlaceable is Troop) {
                Troop troop = iPlaceable as Troop;

                if (troop.GetTroopSO() == troopToSpawnSO && troop.IsOwnedByPlayer()) {
                    iPlaceableOfTypeNumber++;
                }
            }
        }

        Debug.Log("there are " + iPlaceablePoolList.Count + " troops in pool");
        Debug.Log("there are " + iPlaceableOfTypeNumber + " troops in pool of type " + troopToSpawnSO);

        // Then activate the troop from the pool and remove it from the pool list
        IPlaceable identifiedIPlaceableInPoolList = null;

        foreach(IPlaceable iPlaceable in iPlaceablePoolList) {
            if(iPlaceable is Troop) {
                Troop troop = iPlaceable as Troop;

                if (troop.GetTroopSO() == troopToSpawnSO && troop.IsOwnedByPlayer()) {
                    troop.SetPlacingIPlaceable();
                    identifiedIPlaceableInPoolList = troop;
                }
            }
        }

        iPlaceablePoolList.Remove(identifiedIPlaceableInPoolList);
        iPlaceableToPlaceList.Add(identifiedIPlaceableInPoolList);

    }

    public void ActivateOpponentIPlaceable(IPlaceable iPlaceable) {
        iPlaceablePoolList.Remove(iPlaceable);
    }

    public void AddIPlaceabledToPoolList(IPlaceable iPlaceable) {
        if(!iPlaceablePoolList.Contains(iPlaceable)) {
            iPlaceablePoolList.Add(iPlaceable); 
        }
    }

    #endregion

    #region SPAWN TROOPS, UNITS AND BUILDINGS
    [ServerRpc(RequireOwnership = false)]
    private void SpawnTroopServerRpc(int troopSOIndex, ulong ownerClientId) {

        TroopSO troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(troopSOIndex);
        GameObject troopToSpawnGameObject = Instantiate(troopToSpawnSO.troopPrefab);

        NetworkObject troopNetworkObject = troopToSpawnGameObject.GetComponent<NetworkObject>();
        troopNetworkObject.Spawn(true);
        Troop troopToSpawnTroop = troopToSpawnGameObject.GetComponent<Troop>();

        SpawnIPlaceableClientRpc(troopNetworkObject, ownerClientId);

        // Spawn units
        List<Transform> unitsToSpawnBasePositions = troopToSpawnTroop.GetBaseUnitPositions();
        List<Transform> unitsToSpawnAdditionalPositions = troopToSpawnTroop.GetAdditionalUnitPositions();
        List<Transform> units1ToSpawnBasePositions = troopToSpawnTroop.GetBaseUnit1Positions();
        List<Transform> units1ToSpawnAdditionalPositions = troopToSpawnTroop.GetAdditionalUnit1Positions();
        List<Transform> units2ToSpawnBasePositions = troopToSpawnTroop.GetBaseUnit2Positions();
        List<Transform> units2ToSpawnAdditionalPositions = troopToSpawnTroop.GetAdditionalUnit2Positions();
        List<Transform> spawnedUnitPositions = troopToSpawnTroop.GetSpawnedUnitsPositions();

        SpawnUnits(troopToSpawnGameObject, unitsToSpawnBasePositions, false, false, 0);
        SpawnUnits(troopToSpawnGameObject, unitsToSpawnAdditionalPositions, true, false, 0);

        if(units1ToSpawnBasePositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, units1ToSpawnBasePositions, false, false, 1);
        }

        if (units1ToSpawnAdditionalPositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, units1ToSpawnAdditionalPositions, true, false, 1);
        }

        if (units2ToSpawnBasePositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, units2ToSpawnBasePositions, false, false, 2);
        }

        if (units2ToSpawnAdditionalPositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, units2ToSpawnAdditionalPositions, true, false, 2);
        }

        if (spawnedUnitPositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, spawnedUnitPositions, false, true, 3);
        }

        DeActivateOpponentIPlaceableClientRpc(troopNetworkObject);
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
            SpawnTroopServerRpc(troopIndex, ownerClientId);
        }

        DeActivateOpponentIPlaceableClientRpc(buildingNetworkObject);

    }

    [ClientRpc]
    private void SpawnIPlaceableClientRpc(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference, ulong ownerClientId) {
        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();

        if (ownerClientId == NetworkManager.Singleton.LocalClientId) {
            iPlaceablePoolList.Add(iPlaceableToSpawn);
        }

        // Set Owner
        iPlaceableToSpawn.SetIPlaceableOwnerClientId(ownerClientId);
        iPlaceableToSpawn.SetIPlaceableBattlefieldOwner();
    }

    [ClientRpc]
    private void DeActivateOpponentIPlaceableClientRpc(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference) {
        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();

        iPlaceableToSpawn.DeActivateOpponentIPlaceable();
    }

    private List<NetworkObject> SpawnUnits(NetworkObjectReference troopToSpawnNetworkObjectReference, List<Transform> unitsToSpawnPositionList, bool isAdditionalUnits, bool isSpawnedUnit, int unitTypeNumberInTroop) {
        troopToSpawnNetworkObjectReference.TryGet(out NetworkObject troopToSpawnNetworkObject);
        Troop troopToSpawnTroop = troopToSpawnNetworkObject.GetComponent<Troop>();
        GameObject troopToSpawnGameObject = troopToSpawnTroop.gameObject;
        TroopSO troopToSpawnSO = troopToSpawnTroop.GetTroopSO();

        List<NetworkObject> unitsSpawnedNetworkObjectList = new List<NetworkObject>();

        foreach (Transform unitPositionTransform in unitsToSpawnPositionList) {
            GameObject unitToSpawnPrefab = null;

            if (unitTypeNumberInTroop == 0) {
                unitToSpawnPrefab = Instantiate(troopToSpawnSO.unitPrefab);
            }

            if (unitTypeNumberInTroop == 1) {
                unitToSpawnPrefab = Instantiate(troopToSpawnSO.additionalUnit1Prefab);
            }

            if (unitTypeNumberInTroop == 2) {
                unitToSpawnPrefab = Instantiate(troopToSpawnSO.additionalUnit2Prefab);
            }

            if (unitTypeNumberInTroop == 3) {
                unitToSpawnPrefab = Instantiate(troopToSpawnSO.spawnedUnitPrefab);
            }

            NetworkObject unitNetworkObject = unitToSpawnPrefab.GetComponent<NetworkObject>();
            unitNetworkObject.Spawn(true);
            unitNetworkObject.TrySetParent(troopToSpawnGameObject, true);
            SetUnitInitialConditionsClientRpc(unitNetworkObject, troopToSpawnNetworkObject, unitPositionTransform.position, isAdditionalUnits, isSpawnedUnit);

            unitsSpawnedNetworkObjectList.Add(unitNetworkObject);
        }

        return unitsSpawnedNetworkObjectList;
    }

    #endregion

    #region SET TROOP, UNIT AND BUILDINGS CONDITIONS
    [ClientRpc]
    private void SetUnitInitialConditionsClientRpc(NetworkObjectReference unitToSpawnNetworkObjectReference, NetworkObjectReference troopSpawnedNetworkObjectReference, Vector3 unitPosition, bool isAdditionalUnit, bool isSpawnedUnit) {
        unitToSpawnNetworkObjectReference.TryGet(out NetworkObject unitSpawnedNetworkObject);
        Unit unitSpawned = unitSpawnedNetworkObject.GetComponent<Unit>();

        troopSpawnedNetworkObjectReference.TryGet(out NetworkObject troopToSpawnNetworkObject);
        Troop troopToSpawnTroop = troopToSpawnNetworkObject.GetComponent<Troop>();

        //Set Parent Troop
        unitSpawned.SetParentTroop(troopToSpawnTroop);

        //Set Unit Local Position
        unitSpawned.SetLocalPosition(unitPosition, false);

        //Set Units As addional Unit
        if (isAdditionalUnit) {
            unitSpawned.SetUnitAsAdditionalUnit();
        }

        //Set Units As Spawned Units
        if(isSpawnedUnit) {
            unitSpawned.SetUnitAsSpawnedUnit();
        }

        // Set Unit AI to Idle
        unitSpawned.GetComponent<UnitAI>().SetIdleState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetOpponentIPlaceablePositionServerRpc(NetworkObjectReference IPlaceableNetworkObjectReference, int gridPositionx, int gridPositiony) {
        SetOpponentIPlaceablePositionClientRpc(IPlaceableNetworkObjectReference, gridPositionx, gridPositiony);
    }

    [ClientRpc]
    private void SetOpponentIPlaceablePositionClientRpc(NetworkObjectReference IPlaceableNetworkObjectReference, int gridPositionx, int gridPositiony) {
        IPlaceableNetworkObjectReference.TryGet(out NetworkObject IPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableSpawned = IPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();
        GridPosition spawnedIPlaceableGridPosition = new GridPosition(gridPositionx, gridPositiony);

        if (!iPlaceableSpawned.IsOwnedByPlayer()) {
            // IPlaceable is Opponent's 
            spawnedIPlaceableGridPosition = BattleGrid.Instance.TranslateOpponentGridPosition(spawnedIPlaceableGridPosition);

            // Set grid position on iPlaceable
            iPlaceableSpawned.SetIPlaceableGridPosition(spawnedIPlaceableGridPosition);

            // Carry out placed logic on iPlaceable
            iPlaceableSpawned.PlaceIPlaceable();
        }
    }
    #endregion

}
