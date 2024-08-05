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
    public static event EventHandler OnAnyBuildingPlaced;
    public static event EventHandler OnAnyBuildingDestroyed;
    
    protected ulong ownerClientId;

    [SerializeField] protected Transform buildingCenterPoint;
    [SerializeField] protected List<Transform> projectileTargetList;
    [SerializeField] protected BuildingSO buildingSO;
    [SerializeField] private TroopTypeUI buildingTypeUI;

    protected bool isOwnedByPlayer;
    protected bool isPlaced;
    protected bool isDestroyed;

    protected GridPosition currentGridPosition;
    protected Transform battlefieldOwner;
    protected Vector3 battlefieldOffset;

    protected bool buildingIsOnlyVisual;

    private bool buildingHovered;
    private bool buildingSelected;

    protected void Awake() {
        if(buildingSO.buildingBlocksUnitMovement) {
            GetComponent<Collider2D>().enabled = true;
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
        if (!PlayerAction_SpawnTroop.LocalInstance.IsValidIPlaceableSpawningTarget(newGridPosition)) return;

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

        if (hovered) {
            buildingHovered = true;
            buildingTypeUI.SetUIHovered();
            BattlePhaseIPlaceablePanel.Instance.OpenIPlaceableCard(this);
        }
        else {
            if (buildingSelected) return;
            buildingHovered = false;
            buildingTypeUI.SetUIUnHovered();
            BattlePhaseIPlaceablePanel.Instance.CloseIPlaceableCard(this);
        }

        //foreach (Unit unit in allUnitsInTroop) {
        //    unit.SetUnitHovered(hovered);

        //    if (unit.GetComponent<SupportUnit>() != null) {
        //        if (hovered) {
        //            unit.GetComponent<SupportUnit>().HideBuffedUnitBuffs();
        //        }
        //        else {
        //            unit.GetComponent<SupportUnit>().ShowBuffedUnitBuffs();
        //        }
        //    }
        //}
    }

    public void SetBuildingSelected(bool selected) {
        buildingTypeUI.SetUISelected(selected);

        if (selected) {
            OnBuildingSelected?.Invoke(this, EventArgs.Empty);

            //if (BattleManager.Instance.IsPreparationPhase()) {
            //    troopUI.ShowTroopSelectedUI();
            //}

            //foreach (Unit unit in allUnitsInTroop) {
            //    unit.SetUnitSelected(selected);
            //}

        }
        else {
            buildingSelected = false;
            buildingHovered = false;
            OnBuildingUnselected?.Invoke(this, EventArgs.Empty);
            //troopUI.HideTroopSelectedUI();
            BattlePhaseIPlaceablePanel.Instance.CloseIPlaceableCard(this);

            //foreach (Unit unit in allUnitsInTroop) {
            //    unit.SetUnitSelected(false);
            //}
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

        HiddenTacticsMultiplayer.Instance.DestroyIPlaceable(this.GetComponent<NetworkObject>());
    }

    public virtual void HandleIPlaceablePosition() {
        transform.position = battlefieldOwner.position + battlefieldOffset;
    }

    public virtual void PlaceIPlaceable() {
        OnBuildingPlaced?.Invoke(this, EventArgs.Empty);
        OnAnyBuildingPlaced?.Invoke(this, EventArgs.Empty);

        currentGridPosition = BattleGrid.Instance.GetGridPosition(buildingCenterPoint.position);

        isPlaced = true;
        BattleGrid.Instance.AddIPlaceableAtGridPosition(currentGridPosition, this);

        // Reverse X symmetry if not owned by player
        if (!isOwnedByPlayer) {
            transform.localScale = new Vector3(-1,1,1);
        }

        // Set placed troop on grid object
        BattleGrid.Instance.SetIPlaceableSpawnedAtGridPosition(this, currentGridPosition);
        SetIPlaceableGridPosition(currentGridPosition);
        battlefieldOffset = transform.position - battlefieldOwner.transform.position;
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

    public Transform GetProjectileTarget() {
        Transform projectileTarget = projectileTargetList[UnityEngine.Random.Range(0, projectileTargetList.Count)];

        return projectileTarget;
    }

    public GridPosition GetCurrentGridPosition() {
        return currentGridPosition;
    }

    public ITargetable.TargetType GetTargetType() {
        return ITargetable.TargetType.building;
    }

    public IDamageable GetIDamageable() {
        return GetComponent<BuildingHP>();
    }

    public void SetBuildingAsVisual()
    {
        buildingIsOnlyVisual = true;
        GetComponent<BuildingHP>().enabled = false;
        GetComponent<NetworkObject>().enabled = false;
        transform.localScale = Vector3.one * .6f;
    }

    public bool GetBuildingIsOnlyVisual()
    {
        return buildingIsOnlyVisual;
    }

    public Transform GetCenterPoint()
    {
        return buildingCenterPoint;
    }

    public bool GetSelected() {
        return buildingSelected;
    }
}
