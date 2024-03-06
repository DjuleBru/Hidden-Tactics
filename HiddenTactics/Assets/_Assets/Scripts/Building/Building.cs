using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Building : NetworkBehaviour, IPlaceable {


    public event EventHandler OnBuildingPlaced;
    private ulong ownerClientId;

    [SerializeField] private Transform buildingCenterPoint;

    private bool isOwnedByPlayer;
    private bool isPlaced;

    private GridPosition currentGridPosition;
    private Transform battlefieldOwner;
    private Vector3 battlefieldOffset;

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

    public void HandleIPlaceablePosition() {
        transform.position = battlefieldOwner.position + battlefieldOffset;
    }

    public void PlaceIPlaceable() {
        OnBuildingPlaced?.Invoke(this, EventArgs.Empty);

        currentGridPosition = BattleGrid.Instance.GetGridPosition(buildingCenterPoint.position);

        isPlaced = true;

        SetIPlaceableBattlefieldParent(currentGridPosition);
    }

    public void SetIPlaceableOwnerClientId(ulong clientId) {
        ownerClientId = clientId;
        isOwnedByPlayer = (ownerClientId == NetworkManager.Singleton.LocalClientId);
    }

    public void SetIPlaceableGridPosition(GridPosition iPlaceableGridPosition) {
        Vector3 troopWorldPosition = BattleGrid.Instance.GetWorldPosition(iPlaceableGridPosition);

        currentGridPosition = iPlaceableGridPosition;
        transform.position = troopWorldPosition - buildingCenterPoint.localPosition;
    }

    public void SetIPlaceableBattlefieldParent(GridPosition iPlaceableGridPosition) {
        if (iPlaceableGridPosition.x >= 6) {
            battlefieldOwner = BattleGrid.Instance.GetOpponentGridOrigin();
        }
        else {
            battlefieldOwner = BattleGrid.Instance.GetPlayerGridOrigin();
        }
        battlefieldOffset = transform.position - battlefieldOwner.position;
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
}
