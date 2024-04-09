using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Building : NetworkBehaviour, IPlaceable, ITargetable {


    public event EventHandler OnBuildingPlaced;
    public event EventHandler OnBuildingDestroyed;
    
    private ulong ownerClientId;

    [SerializeField] private Transform buildingCenterPoint;
    [SerializeField] private List<Transform> projectileTargetList;
    [SerializeField] private BuildingSO buildingSO;

    private bool isOwnedByPlayer;
    private bool isPlaced;
    private bool isDestroyed;

    private GridPosition currentGridPosition;
    private Transform battlefieldOwner;
    private Vector3 battlefieldOffset;

    private void Awake() {
        if(buildingSO.buildingBlocksUnitMovement) {
            GetComponent<Collider2D>().enabled = true;
        }
    }

    private void Update() {
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
        if (!PlayerAction_SpawnTroop.LocalInstance.IsValidIPlaceableSpawningTarget()) return;

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
    public void Die() {
        isDestroyed = true;
        StartCoroutine(DieCoroutine());
    }

    private IEnumerator DieCoroutine() {
        GetComponent<Collider2D>().enabled = false;
        OnBuildingDestroyed?.Invoke(this, EventArgs.Empty);
        BattleGrid.Instance.RemoveIPlaceableAtGridPosition(BattleGrid.Instance.GetGridPosition(transform.position), this);

        yield return new WaitForSeconds(1f);

        HiddenTacticsMultiplayer.Instance.DestroyIPlaceable(this.GetComponent<NetworkObject>());
    }

    public void HandleIPlaceablePosition() {
        transform.position = battlefieldOwner.position + battlefieldOffset;
    }

    public void PlaceIPlaceable() {
        OnBuildingPlaced?.Invoke(this, EventArgs.Empty);

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
    public void SetIPlaceableGridPosition(GridPosition iPlaceableGridPosition) {
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
}
