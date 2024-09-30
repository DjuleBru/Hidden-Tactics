using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerAction_SpawnIPlaceable : NetworkBehaviour {

    public static PlayerAction_SpawnIPlaceable LocalInstance;

    private List<IPlaceable> iPlaceableToPlaceList;
    private List<IPlaceable> iPlaceablePoolList;

    private Dictionary<int, IPlaceable> spawnedPlayerIPlaceablesDictionary;
    private Dictionary<int, GridPosition> spawnedPlayerIPlaceableGridPositions;
    private Dictionary<ulong, NetworkObject> despawnedObjects = new Dictionary<ulong, NetworkObject>();
    
    private int troopDictionaryInt;

    //private List<ulong> clientsConfirmedSpawn = new List<ulong>();
    private Dictionary<ulong, int> clientsConfirmedSpawn = new Dictionary<ulong, int>();
    private ulong totalClients = 2;

    private void Awake() {
        spawnedPlayerIPlaceablesDictionary = new Dictionary<int, IPlaceable>();
        spawnedPlayerIPlaceableGridPositions = new Dictionary<int, GridPosition>();

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

            Debug.Log("spawnedPlayerIPlaceablesDictionary.Count " + spawnedPlayerIPlaceablesDictionary.Count);

            for (int i = 0; i < spawnedPlayerIPlaceablesDictionary.Count; i++) {
                IPlaceable spawnedIPLaceable = spawnedPlayerIPlaceablesDictionary[i];
                GridPosition spawnedIPlaceableGridPosition = spawnedPlayerIPlaceableGridPositions[i];

                Debug.Log(i);

                if((spawnedIPLaceable as MonoBehaviour) != null) {
                    NetworkObjectReference spawnedIPlaceableNetworkObject = (spawnedIPLaceable as MonoBehaviour).GetComponent<NetworkObject>();
                    SetOpponentIPlaceablePositionServerRpc(spawnedIPlaceableNetworkObject, spawnedIPlaceableGridPosition.x, spawnedIPlaceableGridPosition.y);
                }
            }

            spawnedPlayerIPlaceablesDictionary.Clear();
            spawnedPlayerIPlaceableGridPositions.Clear();
        }

    }

    public void SelectTroopToSpawn(int troopListSOIndex) {
        if (iPlaceableToPlaceList != null) {
            CancelIPlaceablePlacement();
        };

        TroopSO troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(troopListSOIndex);
        PlayerStateUI.Instance.SetPlayerGoldChangingUI(-troopToSpawnSO.spawnTroopCost);

        ActivatePooledIPlaceable(troopListSOIndex, true, false);
    }

    public void SelectBuildingToSpawn(int buildingListSOIndex) {
        if (iPlaceableToPlaceList != null) {
            CancelIPlaceablePlacement();
        };

        BuildingSO buildingToSpawnSO = BattleDataManager.Instance.GetBuildingSOFromIndex(buildingListSOIndex);
        PlayerStateUI.Instance.SetPlayerGoldChangingUI(-buildingToSpawnSO.spawnBuildingCost);

        ActivatePooledIPlaceable(buildingListSOIndex, false, true);
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
            spawnedPlayerIPlaceablesDictionary.Add(troopDictionaryInt, iPlaceable);
            spawnedPlayerIPlaceableGridPositions.Add(troopDictionaryInt, iPlaceable.GetIPlaceableGridPosition());
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

        iPlaceableToPlaceList = new List<IPlaceable>();
    }

    public void CancelIPlaceablePlacement() {

        foreach (IPlaceable iPlaceable in iPlaceableToPlaceList) {
            iPlaceable.SetAsPooledIPlaceable();
        }

        iPlaceableToPlaceList = new List<IPlaceable>();

        PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
    }

    #region OBJECT POOLING

    public void SpawnPlayerIPlaceablePoolList(ulong clientID, Deck deck) {

        foreach (TroopSO troopSO in deck.troopsInDeck) {
            if(troopSO == null) continue;
            int troopSOIndex = BattleDataManager.Instance.GetTroopSOIndex(troopSO);

            //int troopAmountToSpawn = (int)Mathf.Round(PlayerGoldManager.Instance.GetPlayerInitialGold() / troopSO.spawnTroopCost);
            int troopAmountToSpawn = 1;
            for (int i = 0; i < troopAmountToSpawn; i++) {
                SpawnTroopServerRpc(troopSOIndex, clientID);
            }
        }

        foreach (BuildingSO buildingSO in deck.buildingsInDeck) {
            if (buildingSO == null) continue;
            int buildingSOIndex = BattleDataManager.Instance.GetBuildingSOIndex(buildingSO);

            int buildingAmountToSpawn = (int)Mathf.Round(PlayerGoldManager.Instance.GetPlayerInitialGold() / buildingSO.spawnBuildingCost);

            for (int i = 0; i < buildingAmountToSpawn; i++) {
                SpawnBuildingServerRpc(buildingSOIndex, clientID);
            }
        }

        iPlaceableToPlaceList.Clear();
    }

    public void ActivatePooledIPlaceable(int iPlaceableSOIndex, bool isTroop, bool isBuilding) {

        TroopSO troopToSpawnSO = null;
        BuildingSO buildingToSpawnSO = null;
        int iPlaceableOfTypeNumber = 0;

        // First check how many iplaceables of the same type in the pool list
        if (isTroop) {
            troopToSpawnSO = BattleDataManager.Instance.GetTroopSOFromIndex(iPlaceableSOIndex);
            iPlaceableOfTypeNumber = CountIPlaceablesOfTypeInObjectPool(troopToSpawnSO);
        }

        if (isBuilding) {
            buildingToSpawnSO = BattleDataManager.Instance.GetBuildingSOFromIndex(iPlaceableSOIndex);
            iPlaceableOfTypeNumber = CountIPlaceablesOfTypeInObjectPool(buildingToSpawnSO);
        }

        Debug.Log("there are " + iPlaceablePoolList.Count + " iPlaceables in pool");
        Debug.Log("there are " + iPlaceableOfTypeNumber + " iPlaceables in pool of type " + troopToSpawnSO);

        // Then activate the iPlaceable from the pool and remove it from the pool list
        List<IPlaceable> identifiedIPlaceableInPoolList = new List<IPlaceable>();

        if (isTroop) {
            identifiedIPlaceableInPoolList.Add(FindTroopInPool(troopToSpawnSO));
        }
        if (isBuilding) {
            identifiedIPlaceableInPoolList = FindPlaceableList(buildingToSpawnSO);
        }

        foreach(IPlaceable iPlaceableToActivate in identifiedIPlaceableInPoolList) {
            iPlaceableToActivate.SetPlacingIPlaceable();
            iPlaceablePoolList.Remove(iPlaceableToActivate);
            iPlaceableToPlaceList.Add(iPlaceableToActivate);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPooledIPlaceableServerRpc(ulong networkObjectId) {
        if (!IsServer) return;

        // Récupère l'objet depuis le dictionnaire des objets despawned
        if (despawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject)) {
            if (!networkObject.IsSpawned) {
                networkObject.Spawn();  // Re-spawn l'objet
            }
        }
        else {
            Debug.LogError($"Object with NetworkObjectId {networkObjectId} not found in despawned objects.");
        }
    }

    public void DespawnPooledObject(NetworkObject networkObject) {

        // Ajoute l'objet au dictionnaire avant de le despawner
        if(!despawnedObjects.ContainsKey(networkObject.NetworkObjectId)) {
            despawnedObjects[networkObject.NetworkObjectId] = networkObject;
        }

        DespawnPooledObjectServerRpc(networkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnPooledObjectServerRpc(NetworkObjectReference networkObjectReference) {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        networkObject.Despawn(false);  // Désactive sans le détruire
    }

    private int CountIPlaceablesOfTypeInObjectPool(TroopSO troopSO) {
        int iPlaceableOfTypeNumber = 0;

        foreach (IPlaceable iPlaceable in iPlaceablePoolList) {

            if (iPlaceable is Troop) {
                Troop troop = iPlaceable as Troop;

                if (troop.GetTroopSO() == troopSO && troop.IsOwnedByPlayer()) {
                    iPlaceableOfTypeNumber++;

                }
            }
        }

        return iPlaceableOfTypeNumber;
    }

    private int CountIPlaceablesOfTypeInObjectPool(BuildingSO buildingSO) {
        int iPlaceableOfTypeNumber = 0;

        foreach (IPlaceable iPlaceable in iPlaceablePoolList) {

            if (iPlaceable is Building) {
                Building building = iPlaceable as Building;

                if (building.GetBuildingSO() == buildingSO && building.IsOwnedByPlayer()) {
                    iPlaceableOfTypeNumber++;
                }
            }
        }

        return iPlaceableOfTypeNumber;
    }

    private IPlaceable FindTroopInPool(TroopSO troopSO) {
        IPlaceable identifiedIPlaceableInPoolList = null;

        foreach (IPlaceable iPlaceable in iPlaceablePoolList) {
            if (iPlaceable is Troop) {
                Troop troop = iPlaceable as Troop;
                if (troop.GetTroopSO() == troopSO && troop.IsOwnedByPlayer()) {
                    identifiedIPlaceableInPoolList = troop;
                }
            }
        }
        return identifiedIPlaceableInPoolList;
    }

    private List<IPlaceable> FindPlaceableList(BuildingSO buildingSO) {
        List<IPlaceable> identifiedIPlaceableInPoolList = new List<IPlaceable>();

        foreach (IPlaceable iPlaceable in iPlaceablePoolList) {
            if (iPlaceable is Building) {
                Building building = iPlaceable as Building;

                if (building.GetBuildingSO() == buildingSO && building.IsOwnedByPlayer()) {

                    if(buildingSO.hasGarrisonedTroop) {
                        TroopSO garrisonedTroopSO = buildingSO.garrisonedTroopSO; 
                        IPlaceable garrisonedTroopIPlaceable = FindTroopInPool(garrisonedTroopSO);
                        identifiedIPlaceableInPoolList.Add(garrisonedTroopIPlaceable);
                    }

                    identifiedIPlaceableInPoolList.Add(building);
                }
            }
        }
        return identifiedIPlaceableInPoolList;
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

        SpawnUnits(troopToSpawnGameObject, unitsToSpawnBasePositions, false, false, 0, ownerClientId);
        SpawnUnits(troopToSpawnGameObject, unitsToSpawnAdditionalPositions, true, false, 0, ownerClientId);

        if(units1ToSpawnBasePositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, units1ToSpawnBasePositions, false, false, 1, ownerClientId);
        }

        if (units1ToSpawnAdditionalPositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, units1ToSpawnAdditionalPositions, true, false, 1, ownerClientId);
        }

        if (units2ToSpawnBasePositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, units2ToSpawnBasePositions, false, false, 2, ownerClientId);
        }

        if (units2ToSpawnAdditionalPositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, units2ToSpawnAdditionalPositions, true, false, 2, ownerClientId);
        }

        if (spawnedUnitPositions.Count > 0) {
            SpawnUnits(troopToSpawnGameObject, spawnedUnitPositions, false, true, 3, ownerClientId);
        }

        SpawnObjectClientRpc(troopNetworkObject.NetworkObjectId, ownerClientId);
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

        //DeActivateIPlaceableClientRpc(buildingNetworkObject);
    }

    [ClientRpc]
    private void SpawnIPlaceableClientRpc(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference, ulong ownerClientId) {
        Debug.Log("SpawnIPlaceableClientRpc");
        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();

        if (ownerClientId == NetworkManager.Singleton.LocalClientId) {
            Debug.Log("added to local iPlaceablePoolList");
            iPlaceablePoolList.Add(iPlaceableToSpawn);
        }

        // Set Owner
        iPlaceableToSpawn.SetIPlaceableOwnerClientId(ownerClientId);
        iPlaceableToSpawn.SetIPlaceableBattlefieldOwner();
    }

    [ClientRpc]
    private void SpawnObjectClientRpc(ulong networkObjectId, ulong ownerClientId) {
        // Le client reçoit l'instruction de spawn
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject)) {
            // Logique supplémentaire après le spawn côté client (si nécessaire)
            Debug.Log("Troop spawned on client: " + networkObjectId);

            // Confirmer au serveur que l'objet a été spawné côté client
            ConfirmSpawnServerRpc(networkObjectId);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void ConfirmSpawnServerRpc(ulong networkObjectId) {
        Debug.Log("confirm spawn client RPC");
        // Chaque client envoie une confirmation au serveur
        OnClientConfirmedSpawn(networkObjectId);
    }

    public void OnClientConfirmedSpawn(ulong networkObjectId) {
        // Ajoute le client actuel à la liste des clients ayant confirmé
        if(!clientsConfirmedSpawn.ContainsKey(networkObjectId)) {
            clientsConfirmedSpawn[networkObjectId] = 0;
        }
        clientsConfirmedSpawn[networkObjectId] += 1;
        //clientsConfirmedSpawn.Add(NetworkManager.Singleton.LocalClientId);

        Debug.Log(clientsConfirmedSpawn[networkObjectId] + " clients confirmed spawn of object " + networkObjectId);
        // Si tous les clients ont confirmé le spawn

        if (clientsConfirmedSpawn[networkObjectId] == (int)totalClients) {

            // Désactivé ou despawn l'objet une fois toutes les confirmations reçues
            Debug.Log("All clients confirmed, despawning object.");
            StartCoroutine(DespawnAfterAllClientsConfirmed(networkObjectId));
            
        }
    }
    private IEnumerator DespawnAfterAllClientsConfirmed(ulong networkObjectId) {
        Debug.Log("Start Coroutine DespawnAfterAllClientsConfirmed");
        // Attendre 1 ou 2 secondes après les confirmations de spawn
        yield return new WaitForSeconds(2.0f);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject)) {
            networkObject.Despawn(false); // Désactive sans détruire
            Debug.Log($"Despawning object {networkObjectId} after waiting.");

            // Réinitialise la liste de confirmation
            clientsConfirmedSpawn[networkObjectId] = 0;
        }
    }

    [ClientRpc]
    private void DeActivateIPlaceableClientRpc(NetworkObjectReference iPlaceableToSpawnNetworkObjectReference) {
        iPlaceableToSpawnNetworkObjectReference.TryGet(out NetworkObject iPlaceableToSpawnNetworkObject);
        IPlaceable iPlaceableToSpawn = iPlaceableToSpawnNetworkObject.GetComponent<IPlaceable>();
    }

    private List<NetworkObject> SpawnUnits(NetworkObjectReference troopToSpawnNetworkObjectReference, List<Transform> unitsToSpawnPositionList, bool isAdditionalUnits, bool isSpawnedUnit, int unitTypeNumberInTroop, ulong ownerClientId) {
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

            SpawnObjectClientRpc(unitNetworkObject.NetworkObjectId, ownerClientId);
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
