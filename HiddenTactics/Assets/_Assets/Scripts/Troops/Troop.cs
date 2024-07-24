using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Troop : NetworkBehaviour, IPlaceable {

    public event EventHandler OnTroopPlaced;
    public static event EventHandler OnAnyTroopPlaced;
    public static event EventHandler OnAnyTroopSelled;

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

    private List<Unit> allUnitsInTroop;
    private List<Unit> additionalUnitsInTroop;

    [SerializeField] private List<Transform> baseUnitPositions = new List<Transform>();
    [SerializeField] private List<Transform> additionalUnitPositions = new List<Transform>();
    [SerializeField] private List<Transform> baseUnit1Positions = new List<Transform>();
    [SerializeField] private List<Transform> additionalUnit1Positions = new List<Transform>();
    [SerializeField] private List<Transform> baseUnit2Positions = new List<Transform>();
    [SerializeField] private List<Transform> additionalUnit2Positions = new List<Transform>();
    [SerializeField] private Transform allUnitsMiddlePoint;

    private GridPosition currentGridPosition;
    private Transform battlefieldOwner;
    private Vector3 battlefieldOffset;

    private bool troopWasPlacedThisPreparationPhase = true;

    private void Awake() {
        allUnitsInTroop = new List<Unit>();
        additionalUnitsInTroop = new List<Unit>();

        foreach (Transform position in baseUnitPositions) {
            position.gameObject.SetActive(false);
        }
        foreach (Transform position in additionalUnitPositions) {
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
            HandleAllUnitsMiddlePoint();
        }
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseStarting()) {
            UpdateTroopServerRpc();
            troopWasPlacedThisPreparationPhase = false;
        }

        if(BattleManager.Instance.IsPreparationPhase()) {
            troopHP = maxTroopHP;
            OnTroopHPChanged?.Invoke(this, EventArgs.Empty);

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

    private void HandleAllUnitsMiddlePoint() {
        if(BattleManager.Instance.IsBattlePhase()) {
            Vector3 allUnitsSum = Vector3.zero;
            int allUnitsCount = 0;

            foreach (Unit unit in allUnitsInTroop) {
                if (unit.GetUnitIsBought() && !unit.GetIsDead()) {
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
        OnAnyTroopSelled?.Invoke(this, EventArgs.Empty);
        StartCoroutine(SellTroopCoroutine());
    }


    private IEnumerator SellTroopCoroutine() {

        foreach (Unit unit in additionalUnitsInTroop) {
            unit.SellUnit();
        }

        BattleGrid.Instance.RemoveIPlaceableAtGridPosition(currentGridPosition, this);
        BattleGrid.Instance.ResetIPlaceableSpawnedAtGridPosition(currentGridPosition);

        yield return new WaitForSeconds(.1f);

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

            if (!additionalUnitsInTroop.Contains(unit)) {
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

    private void UnitHP_OnHealthChanged(object sender, UnitHP.OnHealthChangedEventArgs e) {
        float healthChange = e.newHealth - e.previousHealth;
        troopHP += healthChange;

        OnTroopHPChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetTroopHPNormalized() {
        return troopHP/maxTroopHP;
    }

    public override void OnDestroy() {
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }
}
