using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Troop : NetworkBehaviour, IPlaceable {
    public event EventHandler OnTroopPlaced;
    private ulong ownerClientId;

    private bool isPlaced;
    private bool isOwnedByPlayer;
    private bool additionalUnitsHaveBeenBought;

    [SerializeField] private bool debugMode;
    [SerializeField] private TroopSO troopSO;
    [SerializeField] private Transform troopCenterPoint;
    [SerializeField] private TroopUI troopUI;

    private List<Unit> allUnitsInTroop;
    private List<Unit> additionalUnitsInTroop;

    [SerializeField] private List<Transform> baseUnitPositions = new List<Transform>();
    [SerializeField] private List<Transform> additionalUnitPositions = new List<Transform>();

    private GridPosition currentGridPosition;
    private Transform battlefieldOwner;
    private Vector3 battlefieldOffset;

    private void Awake() {
        allUnitsInTroop = new List<Unit>();
        additionalUnitsInTroop = new List<Unit>();

        foreach (Transform position in baseUnitPositions) {
            position.gameObject.SetActive(false);
        }
        foreach (Transform position in additionalUnitPositions) {
            position.gameObject.SetActive(false);
        }
    }

    private void Start() {
        if (debugMode) {
            isOwnedByPlayer = false;
            PlaceIPlaceable();
            Unit[] unitArray = GetComponentsInChildren<Unit>();

            foreach (Unit unit in unitArray) {
                //Set Parent Troop
                unit.SetParentTroop(this);

                //Set Unit Local Position
                unit.SetPosition(unit.transform.position);
                unit.DebugModeStartFunction();  
            }
        }
    }

    public override void OnNetworkSpawn() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

    }

    private void Update() {
        if(!isPlaced) {
            HandlePositioningOnGrid();
            HandleIPlaceablePositionDuringPlacement();
        } else {
            HandleIPlaceablePosition();
        }
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseStarting()) {
            UpdateTroopServerRpc();
        }
    }

    public void HandlePositioningOnGrid() {
        if (!isOwnedByPlayer) return;

        GridPosition newGridPosition = MousePositionManager.Instance.GetMouseGridPosition();

        // Grid position is not a valid position
        if (!BattleGrid.Instance.IsValidPlayerGridPosition(newGridPosition)) return;
        if (!PlayerAction_SpawnTroop.LocalInstance.IsValidIPlaceableSpawningTarget()) return;

        // Troop was not set at a grid position yet
        if (currentGridPosition == null) {
            currentGridPosition = MousePositionManager.Instance.GetMouseGridPosition();
            BattleGrid.Instance.AddIPlaceableAtGridPosition(currentGridPosition, this);
        }

        // Troop changed grid position
        if (newGridPosition != currentGridPosition) {
            // Unit changed grid position
            BattleGrid.Instance.IPlaceableMovedGridPosition(this, currentGridPosition, newGridPosition);
            currentGridPosition = newGridPosition;
        }
    }

    public void HandleIPlaceablePositionDuringPlacement() {
        if (currentGridPosition == null) {
            if (!isOwnedByPlayer) return;
            transform.position = MousePositionManager.Instance.GetMousePositionWorldPoint() - troopCenterPoint.localPosition;
        } else {
            transform.position = BattleGrid.Instance.GetWorldPosition(currentGridPosition) - troopCenterPoint.localPosition;
        }
    }

    public void HandleIPlaceablePosition() {
        //transform.position = BattleGrid.Instance.GetWorldPosition(currentGridPosition) - troopCenterPoint.transform.localPosition;
        transform.position = battlefieldOwner.position + battlefieldOffset;
    }

    public void SetIPlaceableOwnerClientId(ulong clientId) {
        ownerClientId = clientId;
        isOwnedByPlayer = (ownerClientId == NetworkManager.Singleton.LocalClientId);
    }

    public void SetIPlaceableBattlefieldOwner() {
        if (isOwnedByPlayer) {
            battlefieldOwner = BattleGrid.Instance.GetPlayerGridOrigin();
        }
        else {
            battlefieldOwner = BattleGrid.Instance.GetOpponentGridOrigin();
        }
    }

    public void DeActivateOpponentIPlaceable() {
        if (!isOwnedByPlayer) {
            gameObject.SetActive(false);
        }
    }

    public void UpgradeTroop() {
        foreach(Unit unit in allUnitsInTroop) {
            unit.UpgradeUnit();
        }
    }

    public void BuyAdditionalUnits() {
        BuyAdditionalUnitsServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void BuyAdditionalUnitsServerRpc() {
        BuyAdditionalUnitsClientRpc();
    }

    [ClientRpc]
    private void BuyAdditionalUnitsClientRpc() {
        if (isOwnedByPlayer) {
            foreach (Unit unit in additionalUnitsInTroop) {
                unit.ActivateAdditionalUnit(); 
            }
            additionalUnitsInTroop.Clear();
        }

        additionalUnitsHaveBeenBought = true;
    }

    [ServerRpc(RequireOwnership =false)]
    private void UpdateTroopServerRpc() {
        UpdateTroopClientRpc();
        
    }

    [ClientRpc]
    private void UpdateTroopClientRpc() {
        if (!isOwnedByPlayer && additionalUnitsHaveBeenBought == true) {
            foreach (Unit unit in additionalUnitsInTroop) {
                unit.ActivateAdditionalUnit();
            }
            additionalUnitsInTroop.Clear();
        }
    }

    public void PlaceIPlaceable() {
        if (!isOwnedByPlayer) {
            gameObject.SetActive(true);
        }

        // Set placed troop on grid object
        BattleGrid.Instance.SetIPlaceableSpawnedAtGridPosition(this, currentGridPosition);
        SetIPlaceableGridPosition(currentGridPosition);
        battlefieldOffset = transform.position - battlefieldOwner.transform.position;

        // Set base units as bought (not in additionalUnitsInTroop) AND Set unit initial grid positions
        foreach(Unit unit in allUnitsInTroop) {
            unit.SetInitialGridPosition(currentGridPosition);

            if (!additionalUnitsInTroop.Contains(unit)) {
                unit.ActivateAdditionalUnit();
            }
        }

        OnTroopPlaced?.Invoke(this, null);
        isPlaced = true;
    }

    public void SetIPlaceableGridPosition(GridPosition troopGridPosition) {
        Vector3 troopWorldPosition = BattleGrid.Instance.GetWorldPosition(troopGridPosition);

        currentGridPosition = troopGridPosition;
        transform.position = troopWorldPosition - troopCenterPoint.localPosition;

    }
    public void AddUnitToUnitInTroopList(Unit unit) {
        allUnitsInTroop.Add(unit);
    }

    public List<Unit> GetUnitInTroopList() {
        return allUnitsInTroop;
    }

    public void AddUnitToAdditionalUnitsInTroopList(Unit unit) {
        additionalUnitsInTroop.Add(unit);
    }

    public List<Unit> GetUnitsInAdditionalUnitsInTroopList() {
        return additionalUnitsInTroop;
    }

    public GridPosition GetIPlaceableGridPosition() {
        return currentGridPosition;
    }

    public Vector3 GetTroopCenterPoint() {
        return troopCenterPoint.position;
    }

    public TroopUI GetTroopUI() {
        return troopUI;
    }
         
    public bool IsOwnedByPlayer() {
        return isOwnedByPlayer;
    }

    public TroopSO GetTroopSO() {
        return troopSO;
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    public List<Transform> GetBaseUnitPositions() {
        return baseUnitPositions;
    }

    public List<Transform> GetAdditionalUnitPositions() {
        return additionalUnitPositions;
    }

    public override void OnDestroy() {
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }
}
