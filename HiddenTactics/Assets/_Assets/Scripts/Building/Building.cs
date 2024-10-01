using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Building : NetworkBehaviour, IPlaceable, ITargetable {

    public event EventHandler OnBuildingPlaced;
    public event EventHandler OnBuildingDestroyed;
    public event EventHandler OnBuildingSelected;
    public event EventHandler OnBuildingUnselected;
    public event EventHandler OnBuildingHovered;
    public event EventHandler OnBuildingUnhovered;
    public event EventHandler OnBuildingSelled;
    public event EventHandler OnBuildingActivated;

    public static event EventHandler OnAnyBuildingPlaced;
    public static event EventHandler OnAnyBuildingSelled;
    public static event EventHandler OnAnyBuildingDestroyed;
    
    protected ulong ownerClientId;

    [SerializeField] protected Transform buildingCenterPoint;
    [SerializeField] protected List<Transform> projectileTargetList;
    [SerializeField] protected BuildingSO buildingSO;
    [SerializeField] private TroopTypeUI buildingTypeUI;
    [SerializeField] private BuildingUI buildingUI;

    private Troop garrisonedTroop;

    protected bool isPooled;
    protected bool isOwnedByPlayer;
    protected bool isPlaced;
    protected bool isDestroyed;

    protected GridPosition currentGridPosition;
    protected Transform battlefieldOwner;
    protected Vector3 battlefieldOffset;

    protected bool buildingIsOnlyVisual;

    private bool buildingWasPlacedThisPreparationPhase = true;
    private bool buildingHovered;
    private bool buildingSelected;
    private bool buildingSelled;

    protected virtual void Awake() {
        if (buildingSO.buildingBlocksUnitMovement) {
            GetComponent<Collider2D>().enabled = true;
        }
    }

    protected virtual void Start() {
        if(BattleManager.Instance != null) {
            BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        }
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        BattleManager.Instance.AddIPlaceableSpawned(NetworkObjectId);
        Debug.Log("AddIPlaceableSpawned " + this);
    }

    protected virtual void BattleManager_OnStateChanged(object sender, EventArgs e) {

        if (BattleManager.Instance.IsBattlePhaseStarting()) {

            if (!isOwnedByPlayer && buildingSelled) {
                SellBuilding();
            }

            buildingWasPlacedThisPreparationPhase = false;
        }
    }

    protected virtual void Update() {
        if (buildingIsOnlyVisual) return;
        if (!isPlaced) {
            HandlePositioningOnGrid();
            HandleIPlaceablePositionDuringPlacement();
        }
        else {
            HandleIPlaceablePosition();
        }
    }

    public void HandlePositioningOnGrid() {
        if (!isOwnedByPlayer) return;

        GridPosition newGridPosition = MousePositionManager.Instance.GetMouseGridPosition();

        // Grid position is not a valid position
        if (!BattleGrid.Instance.IsValidPlayerGridPosition(newGridPosition)) return;
        if (!PlayerAction_SpawnIPlaceable.LocalInstance.IsValidIPlaceableSpawningTarget(newGridPosition)) return;

        // building was not set at a grid position yet
        if (currentGridPosition == null) {
            currentGridPosition = MousePositionManager.Instance.GetMouseGridPosition();
            BattleGrid.Instance.AddIPlaceableAtGridPosition(currentGridPosition, this);
        }

        // Troop changed grid position
        if (newGridPosition != currentGridPosition) {
            BattleGrid.Instance.IPlaceableMovedGridPosition(this, currentGridPosition, newGridPosition);
            currentGridPosition = newGridPosition;
        }
    }

    public void HandleIPlaceablePositionDuringPlacement() {
        if (currentGridPosition == null) {
            if (!isOwnedByPlayer) return;
            transform.position = MousePositionManager.Instance.GetMousePositionWorldPoint() - buildingCenterPoint.localPosition;
        }
        else {
            transform.position = BattleGrid.Instance.GetWorldPosition(currentGridPosition) - buildingCenterPoint.localPosition;
        }
    }

    public void SetBuildingHovered(bool hovered) {
        if (this is Village) return;

        if (hovered) {
            buildingHovered = true;
            buildingTypeUI.SetUIHovered();
            BattlePhaseIPlaceablePanel.Instance.OpenIPlaceableCard(this);
            OnBuildingHovered?.Invoke(this, EventArgs.Empty);
        }
        else {
            if (buildingSelected) return;
            buildingHovered = false;
            buildingTypeUI.SetUIUnHovered();
            BattlePhaseIPlaceablePanel.Instance.CloseIPlaceableCard(this);
            OnBuildingUnhovered?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetBuildingSelected(bool selected) {
        if (this is Village) return;

        buildingTypeUI.SetUISelected(selected);

        if (selected) {
            OnBuildingSelected?.Invoke(this, EventArgs.Empty);
            buildingUI.ShowBuildingSelectedUI();
        }
        else {
            buildingSelected = false;
            buildingHovered = false;
            OnBuildingUnselected?.Invoke(this, EventArgs.Empty);
            buildingUI.HideBuildingSelectedUI();
            BattlePhaseIPlaceablePanel.Instance.CloseIPlaceableCard(this);
        }
    }

    public virtual void Die() {
        isDestroyed = true;
        StartCoroutine(DieCoroutine());
    }

    protected IEnumerator DieCoroutine() {
        GetComponent<Collider2D>().enabled = false;
        OnBuildingDestroyed?.Invoke(this, EventArgs.Empty);
        OnAnyBuildingDestroyed?.Invoke(this, EventArgs.Empty);
        BattleGrid.Instance.RemoveIPlaceableAtGridPosition(BattleGrid.Instance.GetGridPosition(transform.position), this);

        yield return new WaitForSeconds(1f);

        SetAsPooledIPlaceable();
    }

    public virtual void HandleIPlaceablePosition() {
        transform.position = battlefieldOwner.position + battlefieldOffset;
    }

    public virtual void PlaceIPlaceable() {
        if (buildingSelled) return;

        if (!isOwnedByPlayer) {
            gameObject.SetActive(true);
        }

        // Set placed building on grid object
        currentGridPosition = BattleGrid.Instance.GetGridPosition(buildingCenterPoint.position);
        BattleGrid.Instance.SetIPlaceableSpawnedAtGridPosition(this, currentGridPosition);
        SetIPlaceableGridPosition(currentGridPosition);
        battlefieldOffset = transform.position - battlefieldOwner.transform.position;

        isPlaced = true;
        BattleGrid.Instance.AddIPlaceableAtGridPosition(currentGridPosition, this);

        OnBuildingPlaced?.Invoke(this, EventArgs.Empty);
        OnAnyBuildingPlaced?.Invoke(this, EventArgs.Empty);

        // Reverse X symmetry if not owned by player
        if (!isOwnedByPlayer) {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        garrisonedTroop = BattleGrid.Instance.GetTroopAtGridPosition(currentGridPosition);
        garrisonedTroop.SetParentBuilding(this);
    }

    public void SetBuildingSelled() {
        SetBuildingSelledServerRpc();

        if (isOwnedByPlayer) {
            SellBuilding();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBuildingSelledServerRpc() {
        SetBuildingSelledClientRpc();
    }

    [ClientRpc]
    private void SetBuildingSelledClientRpc() {
        buildingSelled = true;
    }

    private void SellBuilding() {
        Debug.Log("SellBuilding");
        buildingSelled = true;
        OnBuildingUnselected?.Invoke(this, EventArgs.Empty);
        OnAnyBuildingSelled?.Invoke(this, EventArgs.Empty);
        OnBuildingSelled?.Invoke(this, EventArgs.Empty);

        SetAsPooledIPlaceable();
    }

    public void SetPlacingIPlaceable() {
        isPooled = false;
        buildingSelled = false;

        SetInitialGridPosition();

        if (isOwnedByPlayer) {
            gameObject.SetActive(true);
            OnBuildingActivated?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetAsPooledIPlaceable() {
        isPooled = true;
        isPlaced = false;

        BattleGrid.Instance.RemoveIPlaceableAtGridPosition(currentGridPosition, this);
        BattleGrid.Instance.ResetIPlaceableSpawnedAtGridPosition(currentGridPosition);
        gameObject.SetActive(false);
        PlayerAction_SpawnIPlaceable.LocalInstance.AddIPlaceabledToPoolList(this);
    }

    private void SetInitialGridPosition() {
        if (isOwnedByPlayer && !(this is Village)) {
            GridPosition newGridPosition = BattleGrid.Instance.GetFirstValidGridPosition();

            BattleGrid.Instance.IPlaceableMovedGridPosition(this, currentGridPosition, newGridPosition);
            currentGridPosition = newGridPosition;
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
    public void DeActivateIPlaceable() {
        gameObject.SetActive(false);
    }

    public virtual void SetIPlaceableGridPosition(GridPosition iPlaceableGridPosition) {
        Vector3 buildingWorldPosition = BattleGrid.Instance.GetWorldPosition(iPlaceableGridPosition);

        currentGridPosition = iPlaceableGridPosition;

        // Reverse X symmetry if not owned by player
        if (isOwnedByPlayer) {
            transform.position = new Vector2(buildingWorldPosition.x - buildingCenterPoint.localPosition.x, buildingWorldPosition.y - buildingCenterPoint.localPosition.y);
        } else {
            transform.position = new Vector2(buildingWorldPosition.x + buildingCenterPoint.localPosition.x, buildingWorldPosition.y - buildingCenterPoint.localPosition.y);
        }
    }

    public GridPosition GetIPlaceableGridPosition() {
        return currentGridPosition;
    }

    public bool IsOwnedByPlayer() {
        return isOwnedByPlayer;
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    public BuildingSO GetBuildingSO() {
        return buildingSO;
    }

    public bool GetIsDead() {
        return isDestroyed;
    }

    public bool BuildingWasPlacedThisPreparationPhase() {
        return buildingWasPlacedThisPreparationPhase;
    }

    public Transform GetProjectileTarget() {
        Transform projectileTarget = projectileTargetList[UnityEngine.Random.Range(0, projectileTargetList.Count)];

        return projectileTarget;
    }

    public GridPosition GetCurrentGridPosition() {
        return currentGridPosition;
    }

    public virtual ITargetable.TargetType GetTargetType() {
        return ITargetable.TargetType.building;
    }

    public IDamageable GetIDamageable() {
        return GetComponent<BuildingHP>();
    }

    public BuildingUI GetBuildingUI() {
        return buildingUI;
    }

    public void SetBuildingAsVisual(int sortingLayerID)
    {
        buildingIsOnlyVisual = true;
        GetComponentInChildren<BuildingVisual>().SetBuildingDeckSlotSpriteSortingOrder(sortingLayerID);
        GetComponent<BuildingHP>().enabled = false;
        GetComponent<NetworkObject>().enabled = false;
        transform.localScale = Vector3.one * .6f;
    }

    public bool GetBuildingIsOnlyVisual()
    {
        return buildingIsOnlyVisual;
    }

    public bool GetBuildingIsPlaced() {
        return isPlaced;
    }

    public Troop GetGarrisonedTroop() {
        return garrisonedTroop;
    }

    public Transform GetCenterPoint()
    {
        return buildingCenterPoint;
    }

    public bool GetSelected() {
        return buildingSelected;
    }
    public bool GetIsPlaced() {
        return isPlaced;
    }
    protected void InvokeOnBuildingPlaced() {
        OnBuildingPlaced?.Invoke(this, EventArgs.Empty);
    }
}
