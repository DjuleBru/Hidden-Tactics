using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Troop : NetworkBehaviour, IPlaceable {

    public event EventHandler OnTroopPlaced;
    public static event EventHandler OnAnyTroopPlaced;
    public static event EventHandler OnAnyTroopSelled;
    public event EventHandler OnTroopSelected;
    public event EventHandler OnTroopUnselected;
    public event EventHandler OnTroopSelled;

    public event EventHandler OnTroopHPChanged;
    public float maxTroopHP;
    public float troopHP;

    private ulong ownerClientId;

    private bool isPlaced;
    private bool isOwnedByPlayer;
    private bool additionalUnitsHaveBeenBought;

    [SerializeField] private bool debugMode;
    [SerializeField] private TroopSO troopSO;
    [SerializeField] private Transform troopCenterPoint;
    [SerializeField] private TroopUI troopUI;
    [SerializeField] private TroopTypeUI troopTypeUI;

    private List<Unit> allUnitsInTroop;
    private List<Unit> additionalUnitsInTroop;
    private List<Unit> spawnedUnitsInTroop;
    private Building parentBuilding;
    private int spawnedUnitActivatedIndex;

    [SerializeField] private List<Transform> baseUnitPositions = new List<Transform>();
    [SerializeField] private List<Transform> additionalUnitPositions = new List<Transform>();
    [SerializeField] private List<Transform> baseUnit1Positions = new List<Transform>();
    [SerializeField] private List<Transform> additionalUnit1Positions = new List<Transform>();
    [SerializeField] private List<Transform> baseUnit2Positions = new List<Transform>();
    [SerializeField] private List<Transform> additionalUnit2Positions = new List<Transform>();
    [SerializeField] private List<Transform> spawnedUnitPositions = new List<Transform>();
    [SerializeField] private Transform allUnitsMiddlePoint;

    private GridPosition currentGridPosition;
    private Transform battlefieldOwner;
    private Vector3 battlefieldOffset;

    private bool troopWasPlacedThisPreparationPhase = true;
    private bool troopHovered;
    private bool troopSelected;
    private bool troopSelled;

    private void Awake() {
        allUnitsInTroop = new List<Unit>();
        additionalUnitsInTroop = new List<Unit>();
        spawnedUnitsInTroop = new List<Unit>();

        foreach (Transform position in baseUnitPositions) {
            position.gameObject.SetActive(false);
        }
        foreach (Transform position in additionalUnitPositions) {
            position.gameObject.SetActive(false);
        }
        foreach (Transform position in additionalUnit1Positions) {
            position.gameObject.SetActive(false);
        }
        foreach (Transform position in additionalUnit2Positions) {
            position.gameObject.SetActive(false);
        }
        foreach (Transform position in spawnedUnitPositions) {
            position.gameObject.SetActive(false);
        }

        if (debugMode) {
            isOwnedByPlayer = false;
            SetIPlaceableBattlefieldOwner();
            currentGridPosition = BattleGrid.Instance.GetGridPosition(transform.position);
            Unit[] unitArray = GetComponentsInChildren<Unit>();

            foreach (Unit unit in unitArray) {
                //Set Parent Troop
                unit.SetParentTroop(this);

                //Set Unit Local Position
                unit.SetPosition(unit.transform.position, true);
            }
        }
    }

    private void Start() {
        if (debugMode) {
            PlaceIPlaceable();
            Unit[] unitArray = GetComponentsInChildren<Unit>();
            BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

            foreach (Unit unit in unitArray) {
                unit.DebugModeStartFunction();
            }
        }
        if(isOwnedByPlayer) {
            GridPosition newGridPosition = BattleGrid.Instance.GetFirstValidGridPosition();
            BattleGrid.Instance.IPlaceableMovedGridPosition(this, currentGridPosition, newGridPosition);
            currentGridPosition = newGridPosition;
        }
    }

    public override void OnNetworkSpawn() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {
        HandleAllUnitsMiddlePoint();
        if (!isPlaced) {
            HandlePositioningOnGrid();
            HandleIPlaceablePositionDuringPlacement();
        } else {
            HandleIPlaceablePosition();
        }
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {

        if(BattleManager.Instance.IsBattlePhaseStarting()) {

            if (troopSelled) {
                DestroyTroop();
            }

            UpdateTroopServerRpc();

            troopWasPlacedThisPreparationPhase = false;
        }

        if (BattleManager.Instance.IsBattlePhaseEnding()) {
            DeactivateAllDynamicallySpawnedUnits();
        }

        if (BattleManager.Instance.IsPreparationPhase()) {
            troopHP = maxTroopHP;
            OnTroopHPChanged?.Invoke(this, EventArgs.Empty);
        } 
    }

    public void HandlePositioningOnGrid() {
        if (!isOwnedByPlayer) return;

        GridPosition newGridPosition = MousePositionManager.Instance.GetMouseGridPosition();

        // Grid position is not a valid position
        if (!BattleGrid.Instance.IsValidPlayerGridPosition(newGridPosition)) return;
        if (!PlayerAction_SpawnIPlaceable.LocalInstance.IsMousePositionValidIPlaceableSpawningTarget()) return;

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

    private void HandleAllUnitsMiddlePoint() {
        if(BattleManager.Instance.IsBattlePhase()) {
            Vector3 allUnitsSum = Vector3.zero;
            int allUnitsCount = 0;

            foreach (Unit unit in allUnitsInTroop) {
                if (unit.GetUnitIsBought() && !unit.GetIsDead() && !unit.GetUnitIsDynamicallySpawnedUnit()) {
                    allUnitsSum += unit.transform.position;
                    allUnitsCount++;
                }
            }

            if (allUnitsCount > 0) {
                allUnitsSum = allUnitsSum / allUnitsCount;
                allUnitsMiddlePoint.position = allUnitsSum;
            }
        } else {
            allUnitsMiddlePoint.position = troopCenterPoint.position;
        }
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
        Debug.Log("BuyAdditionalUnits");
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

                // Update troop HP
                troopHP += unit.GetComponent<UnitHP>().GetHP();
                maxTroopHP = troopHP;
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

    public void SellTroop() {
        troopSelled = true;
        OnTroopUnselected?.Invoke(this, EventArgs.Empty);
        OnAnyTroopSelled?.Invoke(this, EventArgs.Empty);
        OnTroopSelled?.Invoke(this, EventArgs.Empty);

        foreach (Unit unit in allUnitsInTroop) {
            unit.SellUnit();
        }

        BattleGrid.Instance.RemoveIPlaceableAtGridPosition(currentGridPosition, this);
        BattleGrid.Instance.ResetIPlaceableSpawnedAtGridPosition(currentGridPosition);
    }

    private void DestroyTroop() {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        HiddenTacticsMultiplayer.Instance.DestroyIPlaceable(networkObject);
    }

    public void PlaceIPlaceable() {
        if (!isOwnedByPlayer) {
            gameObject.SetActive(true);
        }

        // Set placed troop on grid object
        BattleGrid.Instance.SetIPlaceableSpawnedAtGridPosition(this, currentGridPosition);
        SetIPlaceableGridPosition(currentGridPosition);
        battlefieldOffset = transform.position - battlefieldOwner.transform.position;

        // Set base units as bought (not in additionalUnitsInTroop), Set unit initial grid positions, Set Troop max HP
        foreach(Unit unit in allUnitsInTroop) {
            unit.SetInitialUnitPosition(currentGridPosition);

            if (!additionalUnitsInTroop.Contains(unit) && !spawnedUnitsInTroop.Contains(unit)) {
                unit.ActivateAdditionalUnit();
                troopHP += unit.GetComponent<UnitHP>().GetHP();
                maxTroopHP = troopHP;
            }
        }

        isPlaced = true;
        OnTroopPlaced?.Invoke(this, null);
        OnAnyTroopPlaced?.Invoke(this, EventArgs.Empty);
    }

    public void SetIPlaceableGridPosition(GridPosition troopGridPosition) {
        Vector3 troopWorldPosition = BattleGrid.Instance.GetWorldPosition(troopGridPosition);

        currentGridPosition = troopGridPosition;
        transform.position = troopWorldPosition - troopCenterPoint.localPosition;

    }

    public void SetTroopHovered(bool hovered) {

        if(hovered) {
            troopHovered = true;
            troopTypeUI.SetUIHovered();
            BattlePhaseIPlaceablePanel.Instance.OpenIPlaceableCard(this);
        } else {
            if (troopSelected) return;
            troopHovered = false;
            troopTypeUI.SetUIUnHovered();
            BattlePhaseIPlaceablePanel.Instance.CloseIPlaceableCard(this);
        }

        foreach (Unit unit in allUnitsInTroop) {
            unit.SetUnitHovered(hovered);

            if (unit.GetComponent<SupportUnit>() != null) {
                if(hovered) {
                    unit.GetComponent<SupportUnit>().HideBuffedUnitBuffs();
                } else {
                    unit.GetComponent<SupportUnit>().ShowBuffedUnitBuffs();
                }
            }
        }
    }

    public void SetTroopSelected(bool selected) {
        troopTypeUI.SetUISelected(selected);

        if (selected) {
            troopSelected = true;
            OnTroopSelected?.Invoke(this, EventArgs.Empty);
            if (BattleManager.Instance.IsPreparationPhase() && !troopSO.isGarrisonedTroop) {
                troopUI.ShowTroopSelectedUI();
            }

            foreach (Unit unit in allUnitsInTroop) {
                unit.SetUnitSelected(selected);
            }

        } else {
            troopSelected = false;
            troopHovered = false;
            OnTroopUnselected?.Invoke(this, EventArgs.Empty);
            troopUI.HideTroopSelectedUI();
            BattlePhaseIPlaceablePanel.Instance.CloseIPlaceableCard(this);

            foreach (Unit unit in allUnitsInTroop) {
                unit.SetUnitSelected(false);
            }
        }
    }

    public void ActivateNextSpawnedUnit(Vector3 spawnPosition) {

        if (!BattleManager.Instance.IsBattlePhase()) {
            return;
        }

        if (spawnedUnitActivatedIndex < spawnedUnitsInTroop.Count) {
            spawnedUnitsInTroop[spawnedUnitActivatedIndex].transform.position = spawnPosition;
            spawnedUnitsInTroop[spawnedUnitActivatedIndex].ActivateSpawnedUnit();
            spawnedUnitActivatedIndex++;
        } else {
            Debug.LogWarning("no more units to spawn");
        }
    }

    public void DeactivateAllDynamicallySpawnedUnits() {
        spawnedUnitActivatedIndex = 0;

        foreach (Unit unit in spawnedUnitsInTroop) {
            unit.DeactivateDynamicallySpawnedUnit();
            unit.transform.localPosition = Vector3.zero;
        }
    }

    public void AddUnitToUnitInTroopList(Unit unit) {
        allUnitsInTroop.Add(unit);

        //Subscribe to unit HP events
        unit.GetComponent<UnitHP>().OnHealthChanged += UnitHP_OnHealthChanged;
    }

    public List<Unit> GetUnitInTroopList() {
        return allUnitsInTroop;
    }

    public void AddUnitToAdditionalUnitsInTroopList(Unit unit) {
        additionalUnitsInTroop.Add(unit);
    }

    public void AddUnitToSpawnedUnitsInTroopList(Unit unit) {
        if(!spawnedUnitsInTroop.Contains(unit)) {
            spawnedUnitsInTroop.Add(unit);
        }
    }

    public void SetParentBuilding(Building building) {
        parentBuilding = building;
        parentBuilding.OnBuildingDestroyed += ParentBuilding_OnBuildingDestroyed;
        parentBuilding.OnBuildingSelled += ParentBuilding_OnBuildingSelled;
    }

    private void ParentBuilding_OnBuildingSelled(object sender, EventArgs e) {
        SellTroop();
    }

    private void ParentBuilding_OnBuildingDestroyed(object sender, EventArgs e) {
        HiddenTacticsMultiplayer.Instance.DestroyIPlaceable(GetComponent<NetworkObject>());
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

    public bool TroopWasPlacedThisPreparationPhase() {
        return troopWasPlacedThisPreparationPhase;
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

    public List<Transform> GetBaseUnit1Positions() {
        return baseUnit1Positions;
    }

    public List<Transform> GetAdditionalUnit1Positions() {
        return additionalUnit1Positions;
    }

    public List<Transform> GetBaseUnit2Positions() {
        return baseUnit2Positions;
    }

    public List<Transform> GetAdditionalUnit2Positions() {
        return additionalUnit2Positions;
    }
    public List<Transform> GetSpawnedUnitsPositions() {
        return spawnedUnitPositions;
    }

    private void UnitHP_OnHealthChanged(object sender, UnitHP.OnHealthChangedEventArgs e) {
        float healthChange = e.newHealth - e.previousHealth;
        troopHP += healthChange;

        OnTroopHPChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetTroopHPNormalized() {
        return troopHP/maxTroopHP;
    }

    public bool GetSelected() {
        return troopSelected;
    }

    public override void OnDestroy() {
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }
}
