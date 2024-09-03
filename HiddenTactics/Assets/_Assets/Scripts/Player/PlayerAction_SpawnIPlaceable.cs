using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerAction_SpawnIPlaceable : NetworkBehaviour {

    public static PlayerAction_SpawnIPlaceable LocalInstance;

    private List<IPlaceable> iPlaceableToSpawnList;
    private List<IPlaceable> iPlaceableInstantiatedList;
    private List<IPlaceable> iPlaceablePlacedList;

    private Dictionary<int, IPlaceable> spawnedIPlaceablesDictionary;
    private Dictionary<int, GridPosition> spawnedIPlaceableGridPositions;
    private int troopDictionaryInt;
    private bool iPlaceableSpawnedOnClient;
    private bool playerIsTryingToPlaceIPlaceableList;

    private void Awake() {
        spawnedIPlaceablesDictionary = new Dictionary<int, IPlaceable>();
        spawnedIPlaceableGridPositions = new Dictionary<int, GridPosition>();

        iPlaceableToSpawnList = new List<IPlaceable>();
        iPlaceableInstantiatedList = new List<IPlaceable>();
        iPlaceablePlacedList = new List<IPlaceable>();
    }

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
        }
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {
        if(playerIsTryingToPlaceIPlaceableList) {
            if(iPlaceableSpawnedOnClient) {
                PlaceIPlaceableList();
            }
        }
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        // Destroy troop to spawn 
        if (iPlaceableToSpawnList.Count != 0) {
            CancelIPlaceablePlacement();
        };

        FetchAndSetSpawnedIPlaceablesFromServer();
    }

    private void FetchAndSetSpawnedIPlaceablesFromServer() {

        troopDictionaryInt = 0;

        if (BattleManager.Instance.IsBattlePhaseStarting()) {
            // Set & Fetch spawned troops data from server

            for (int i = 0; i < spawnedIPlaceablesDictionary.Count; i++) {
                IPlaceable spawnedIPLaceable = spawnedIPlaceablesDictionary[i];
                GridPosition spawnedIPlaceableGridPosition = spawnedIPlaceableGridPositions[i];

                if ((spawnedIPLaceable as MonoBehaviour) != null) {
                    NetworkObjectReference spawnedIPlaceableNetworkObject = (spawnedIPLaceable as MonoBehaviour).GetComponent<NetworkObject>();
                    SetIPlaceablePositionServerRpc(spawnedIPlaceableNetworkObject, spawnedIPlaceableGridPosition.x, spawnedIPlaceableGridPosition.y);
                }
            }

            spawnedIPlaceablesDictionary.Clear();
            spawnedIPlaceableGridPositions.Clear();
        }
    }

    public void SelectTroopToSpawn(int troopListSOIndex) {
        iPlaceableSpawnedOnClient = false;

        if (iPlaceableToSpawnList != null) {
            CancelIPlaceablePlacement();
        };

        TroopSO troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(troopListSOIndex);
        PlayerStateUI.Instance.SetPlayerGoldChangingUI(-troopToSpawnSO.spawnTroopCost);

        int iPlaceableID = Random.Range(0, 1000000);
        SpawnFakeVisualTroop(troopListSOIndex, iPlaceableID);
        SpawnTroopServerRpc(troopListSOIndex, NetworkManager.Singleton.LocalClientId, iPlaceableID);
    }

    public void SelectBuildingToSpawn(int buildingListSOIndex) {
        iPlaceableSpawnedOnClient = false;

        if (iPlaceableToSpawnList != null) {
            CancelIPlaceablePlacement();
        };

        BuildingSO buildingToSpawnSO = BattleDataManager.Instance.GetBuildingSOFromIndex(buildingListSOIndex);
        PlayerStateUI.Instance.SetPlayerGoldChangingUI(-buildingToSpawnSO.spawnBuildingCost);

        int iPlaceableID = Random.Range(0, 1000000);
        SpawnBuildingServerRpc(buildingListSOIndex, NetworkManager.Singleton.LocalClientId, iPlaceableID);
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
        if(iPlaceableSpawnedOnClient) {
            foreach (IPlaceable iPlaceable in iPlaceableToSpawnList) {
                iPlaceablePlacedList.Add(iPlaceable);

                iPlaceable.PlaceIPlaceable();
                spawnedIPlaceablesDictionary.Add(troopDictionaryInt, iPlaceable);
                spawnedIPlaceableGridPositions.Add(troopDictionaryInt, iPlaceable.GetIPlaceableGridPosition());

                troopDictionaryInt++;

                if (iPlaceable is Building) {
                    Building buildingSpawned = (Building)iPlaceable;
                    PlayerGoldManager.Instance.SpendGold(buildingSpawned.GetBuildingSO().spawnBuildingCost, NetworkManager.Singleton.LocalClientId);
                }

                if (iPlaceable is Troop) {
                    Troop troopSpawned = (Troop)iPlaceable;
                    PlayerGoldManager.Instance.SpendGold(troopSpawned.GetTroopSO().spawnTroopCost, NetworkManager.Singleton.LocalClientId);
                }
            }

            iPlaceableToSpawnList = new List<IPlaceable>();
            iPlaceableSpawnedOnClient = false;
            playerIsTryingToPlaceIPlaceableList = false;
            WaitingForServerUI.Instance.Hide();

        } else {
            playerIsTryingToPlaceIPlaceableList = true;
            WaitingForServerUI.Instance.Show();
        }

    }

    public void CancelIPlaceablePlacement() {
        foreach (IPlaceable iPlaceable in iPlaceableToSpawnList) {

            if(iPlaceable.GetIsSpawnedOnServer()) { 

            // For Network instantiated troops :
            NetworkObjectReference iPlaceableNetworkObjectReference = (iPlaceable as MonoBehaviour).GetComponent<NetworkObject>();
            DestroyServerIPlaceable(iPlaceableNetworkObjectReference);

            } else {

                DestroyLocalIPlaceable(iPlaceable);

            }
        }

        iPlaceableToSpawnList = new List<IPlaceable>();

        PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
    }

    public void DestroyLocalIPlaceable(IPlaceable iPlaceable) {
        // For Local instantiated troops:
        if ((iPlaceable as MonoBehaviour).TryGetComponent<Troop>(out Troop troop)) {
            foreach (Unit unit in troop.GetUnitInTroopList()) {
                unit.DestroySelf();
            }
        }
        Destroy((iPlaceable as MonoBehaviour).gameObject);
    }

    public void DestroyServerIPlaceable(NetworkObjectReference iPlaceableNetworkObjectReference) {
        //Deactivate network game object
        iPlaceableNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();
        (iPlaceableToSpawn as MonoBehaviour).gameObject.SetActive(false);

        HiddenTacticsMultiplayer.Instance.DestroyIPlaceable(iPlaceableNetworkObjectReference);
    }

    #region INSTANTIATE FAKE TROOPS, UNITS AND BUILDINGS

    public void SpawnFakeVisualTroop(int troopListSOIndex, int troopID) {

        TroopSO troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(troopListSOIndex);
        GameObject troopToSpawnGameObject = Instantiate(troopToSpawnSO.troopPrefab);
        Troop troopToSpawnTroop = troopToSpawnGameObject.GetComponent<Troop>();

        Debug.Log("local troop instantiated");

        iPlaceableInstantiatedList.Add(troopToSpawnTroop);
        iPlaceableToSpawnList.Add(troopToSpawnTroop);

        // Set Owner
        troopToSpawnTroop.SetIPlaceableOwnerClientId(NetworkManager.Singleton.LocalClientId);
        troopToSpawnTroop.SetIPlaceableBattlefieldOwner();
        troopToSpawnTroop.SetIPlaceableID(troopID);

        // Spawn units
        List<Transform> unitsToSpawnBasePositions = troopToSpawnTroop.GetBaseUnitPositions();
        List<Transform> unitsToSpawnAdditionalPositions = troopToSpawnTroop.GetAdditionalUnitPositions();
        List<Transform> units1ToSpawnBasePositions = troopToSpawnTroop.GetBaseUnit1Positions();
        List<Transform> units1ToSpawnAdditionalPositions = troopToSpawnTroop.GetAdditionalUnit1Positions();
        List<Transform> units2ToSpawnBasePositions = troopToSpawnTroop.GetBaseUnit2Positions();
        List<Transform> units2ToSpawnAdditionalPositions = troopToSpawnTroop.GetAdditionalUnit2Positions();
        List<Transform> spawnedUnitPositions = troopToSpawnTroop.GetSpawnedUnitsPositions();

        SpawnFakeUnits(troopToSpawnTroop, unitsToSpawnBasePositions, false, false, 0);
        SpawnFakeUnits(troopToSpawnTroop, unitsToSpawnAdditionalPositions, true, false, 0);

        if (units1ToSpawnBasePositions.Count > 0) {
            SpawnFakeUnits(troopToSpawnTroop, units1ToSpawnBasePositions, false, false, 1);
        }

        if (units1ToSpawnAdditionalPositions.Count > 0) {
            SpawnFakeUnits(troopToSpawnTroop, units1ToSpawnAdditionalPositions, true, false, 1);
        }

        if (units2ToSpawnBasePositions.Count > 0) {
            SpawnFakeUnits(troopToSpawnTroop, units2ToSpawnBasePositions, false, false, 2);
        }

        if (units2ToSpawnAdditionalPositions.Count > 0) {
            SpawnFakeUnits(troopToSpawnTroop, units2ToSpawnAdditionalPositions, true, false, 2);
        }

        if (spawnedUnitPositions.Count > 0) {
            SpawnFakeUnits(troopToSpawnTroop, spawnedUnitPositions, false, true, 3);
        }

    }

    public void SpawnFakeBuildingTroop(int buildingListSOIndex) {

    }

    private void SpawnFakeUnits(Troop troopSpawned, List<Transform> unitsToSpawnPositionList, bool isAdditionalUnits, bool isSpawnedUnit, int unitTypeNumberInTroop) {
        Transform troopToSpawnTransform = troopSpawned.transform;
        TroopSO troopToSpawnSO = troopSpawned.GetTroopSO();

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

            SetFakeUnitInitialConditions(unitToSpawnPrefab, troopToSpawnTransform, unitPositionTransform.position, isAdditionalUnits, isSpawnedUnit);
        }
    }
    
    private void SetFakeUnitInitialConditions(GameObject unitGO, Transform parentTroopTransform, Vector3 unitPosition, bool isAdditionalUnit, bool isSpawnedUnit) {
        Unit unitSpawned = unitGO.GetComponent<Unit>();
        Troop troopToSpawnTroop = parentTroopTransform.GetComponent<Troop>();

        //Set Parent Troop
        unitSpawned.SetParentTroop(troopToSpawnTroop);

        //Set Unit Local Position
        unitSpawned.SetLocalPosition(unitPosition, false);

        // Set as Fake Visual Unit
        unitSpawned.SetAsFakeVisualUnit();

        //Set Units As addional Unit
        if (isAdditionalUnit) {
            unitSpawned.SetUnitAsAdditionalUnit();
        }

        //Set Units As Spawned Units
        if (isSpawnedUnit) {
            unitSpawned.SetUnitAsSpawnedUnit();
        }

        // Set Unit AI to Idle
        unitSpawned.GetComponent<UnitAI>().SetIdleState();
    }

    public void RemoveFakeIPlaceable(int iPlaceableID) {

        List<IPlaceable> IPlaceableDestroyedList = new List<IPlaceable>();
        foreach(IPlaceable IPlaceable in iPlaceableInstantiatedList) {

            if(IPlaceable.GetIPlaceableID() == iPlaceableID) {
                DestroyLocalIPlaceable(IPlaceable);
                IPlaceableDestroyedList.Add(IPlaceable);
                Debug.Log("DestroyLocalIPlaceable");
            }

        }

        foreach(IPlaceable placeable in IPlaceableDestroyedList) {
            Debug.Log("removed placeable from iPlaceableInstantiatedList");
            iPlaceableInstantiatedList.Remove(placeable);
            iPlaceableToSpawnList.Remove(placeable);
        }

        iPlaceableInstantiatedList.Clear();
    }

    #endregion

    #region SPAWN TROOPS, UNITS AND BUILDINGS
    [ServerRpc(RequireOwnership = false)]
    private void SpawnTroopServerRpc(int troopSOIndex, ulong ownerClientId, int iPlaceableID) {

        TroopSO troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(troopSOIndex);
        GameObject troopToSpawnGameObject = Instantiate(troopToSpawnSO.troopPrefab);

        NetworkObject troopNetworkObject = troopToSpawnGameObject.GetComponent<NetworkObject>();
        troopNetworkObject.Spawn(true);
        Troop troopToSpawnTroop = troopToSpawnGameObject.GetComponent<Troop>();

        SetIPlaceableIDClientRPC(troopNetworkObject, ownerClientId, iPlaceableID);
        SpawnIPlaceableClientRpc(troopNetworkObject, ownerClientId, iPlaceableID);

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
        ReplaceLocalIPLaceableClientRPC(troopNetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBuildingServerRpc(int buildingSOIndex, ulong ownerClientId, int iPlaceableID) {

        BuildingSO buildingToSpawnSO = BattleDataManager.Instance.GetBuildingSOFromIndex(buildingSOIndex);
        GameObject buildingToSpawnGameObject = Instantiate(buildingToSpawnSO.buildingPrefab);

        NetworkObject buildingNetworkObject = buildingToSpawnGameObject.GetComponent<NetworkObject>();
        buildingNetworkObject.Spawn(true);

        SpawnIPlaceableClientRpc(buildingNetworkObject, ownerClientId, iPlaceableID);

        if(buildingToSpawnSO.hasGarrisonedTroop) {
            // Spawn garrisoned troop
            int troopIndex = BattleDataManager.Instance.GetTroopSOIndex(buildingToSpawnSO.garrisonedTroopSO);
            SpawnTroopServerRpc(troopIndex, ownerClientId, iPlaceableID+1);
        }

        DeActivateOpponentIPlaceableClientRpc(buildingNetworkObject);
        SetIPlaceableIDClientRPC(buildingNetworkObject, ownerClientId, iPlaceableID);
    }

    [ClientRpc]
    private void SpawnIPlaceableClientRpc(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference, ulong ownerClientId, int iPlaceableID) {
        Debug.Log("iplaceable spawned on client");

        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();

         // Check if player cancelled placement before spawning
        if (ownerClientId == NetworkManager.Singleton.LocalClientId) {

            bool iPlaceablePlacementHasBeenCancelled = true;

            Debug.Log("iPlaceableInstantiatedList.Count " + iPlaceableInstantiatedList.Count);
            foreach (IPlaceable iPlaceable in iPlaceableInstantiatedList) {
                if (iPlaceable.GetIPlaceableID() == iPlaceableID) {
                    iPlaceablePlacementHasBeenCancelled = false;
                }
            }

            if (iPlaceablePlacementHasBeenCancelled) {
                Debug.Log("player cancelled placement before spawning");
                DestroyServerIPlaceable(iPlaceableToSpawnNetworkObjectReference);
                return;
            }

            iPlaceableToSpawnList.Add(iPlaceableToSpawn);
        }


        // Set Owner
        iPlaceableToSpawn.SetIPlaceableOwnerClientId(ownerClientId);
        iPlaceableToSpawn.SetIPlaceableBattlefieldOwner();

        iPlaceableSpawnedOnClient = true;

        // Check if player placed iplaceable before spawning
        //if (ownerClientId == NetworkManager.Singleton.LocalClientId) {

        //    foreach (IPlaceable iPlaceable in iPlaceablePlacedList) {
        //        if (iPlaceable.GetIPlaceableID() == iPlaceableID) {
        //            Debug.Log("player placed iplaceable before spawning");
        //            iPlaceable.PlaceIPlaceable();
        //        }
        //    }

        //}

    }

    [ClientRpc]
    private void SetIPlaceableIDClientRPC(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference, ulong ownerClientId, int iPlaceableID) {
        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();

        // Set ID
        iPlaceableToSpawn.SetIPlaceableID(iPlaceableID);
    }
    
    [ClientRpc]
    private void ReplaceLocalIPLaceableClientRPC(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference) {

        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();

        iPlaceableToSpawn.ReplaceLocalIPleaceable();

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
