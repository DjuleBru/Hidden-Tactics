using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAction_SpawnIPlaceable : NetworkBehaviour {

    public static PlayerAction_SpawnIPlaceable LocalInstance;

    private List<IPlaceable> iPlaceableToPlaceList;
    private List<IPlaceable> iPlaceablePoolList;

    private Dictionary<int, IPlaceable> spawnedIPlaceablesDictionary;
    private Dictionary<int, GridPosition> spawnedIPlaceableGridPositions;
    private int troopDictionaryInt;


    private void Awake() {
        spawnedIPlaceablesDictionary = new Dictionary<int, IPlaceable>();
        spawnedIPlaceableGridPositions = new Dictionary<int, GridPosition>();

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

            for (int i = 0; i < spawnedIPlaceablesDictionary.Count; i++) {
                IPlaceable spawnedIPLaceable = spawnedIPlaceablesDictionary[i];
                GridPosition spawnedIPlaceableGridPosition = spawnedIPlaceableGridPositions[i];

                if((spawnedIPLaceable as MonoBehaviour) != null) {
                    NetworkObjectReference spawnedIPlaceableNetworkObject = (spawnedIPLaceable as MonoBehaviour).GetComponent<NetworkObject>();
                    SetIPlaceablePositionServerRpc(spawnedIPlaceableNetworkObject, spawnedIPlaceableGridPosition.x, spawnedIPlaceableGridPosition.y);
                }
            }

            spawnedIPlaceablesDictionary.Clear();
            spawnedIPlaceableGridPositions.Clear();
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
            spawnedIPlaceablesDictionary.Add(troopDictionaryInt, iPlaceable);
            spawnedIPlaceableGridPositions.Add(troopDictionaryInt, iPlaceable.GetIPlaceableGridPosition());
            troopDictionaryInt++;

            if(iPlaceable is Building) {
                Building buildingSpawned = (Building)iPlaceable;
                PlayerGoldManager.Instance.SpendGold(buildingSpawned.GetBuildingSO().spawnBuildingCost, NetworkManager.Singleton.LocalClientId);
            }

            if (iPlaceable is Troop) {
                Troop troopSpawned = (Troop)iPlaceable;
                PlayerGoldManager.Instance.SpendGold(troopSpawned.GetTroopSO().spawnTroopCost, NetworkManager.Singleton.LocalClientId);
            }
        }

        iPlaceableToPlaceList = new List<IPlaceable>();
    }

    public void CancelIPlaceablePlacement() {

        foreach (IPlaceable iPlaceable in iPlaceableToPlaceList) {
            //NetworkObjectReference iPlaceableNetworkObjectReference = (iPlaceable as MonoBehaviour).GetComponent<NetworkObject>();
            //HiddenTacticsMultiplayer.Instance.DestroyIPlaceable(iPlaceableNetworkObjectReference);

            iPlaceable.SetAsPooledIPlaceable();
            iPlaceablePoolList.Add(iPlaceable);
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

                if (troop.GetTroopSO() == troopToSpawnSO) {
                    iPlaceableOfTypeNumber++;
                }
            }
        }

        // Then activated the troop from the pool and remove it from the pool list
        IPlaceable identifiedIPlaceableInPoolList = null;

        foreach(IPlaceable iPlaceable in iPlaceablePoolList) {
            if(iPlaceable is Troop) {
                Troop troop = iPlaceable as Troop;

                if (troop.GetTroopSO() == troopToSpawnSO) {
                    troop.SetPlacingIPlaceable();
                    identifiedIPlaceableInPoolList = troop;
                }
            }
        }

        iPlaceablePoolList.Remove(identifiedIPlaceableInPoolList);
        iPlaceableToPlaceList.Add(identifiedIPlaceableInPoolList);
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
        iPlaceablePoolList.Add(troopToSpawnTroop);
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

        //if (ownerClientId == NetworkManager.Singleton.LocalClientId) {
        //    iPlaceableToPlaceList.Add(iPlaceableToSpawn);
        //}

        // Set Owner
        iPlaceableToSpawn.SetIPlaceableOwnerClientId(ownerClientId);
        iPlaceableToSpawn.SetIPlaceableBattlefieldOwner();
    }

    [ClientRpc]
    private void DeActivateOpponentIPlaceableClientRpc(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference) {
        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();

        //Set parent 
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
    private void SetIPlaceablePositionServerRpc(NetworkObjectReference IPlaceableNetworkObjectReference, int gridPositionx, int gridPositiony) {
        SetIPlaceablePositionClientRpc(IPlaceableNetworkObjectReference, gridPositionx, gridPositiony);
    }

    [ClientRpc]
    private void SetIPlaceablePositionClientRpc(NetworkObjectReference IPlaceableNetworkObjectReference, int gridPositionx, int gridPositiony) {
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
