using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Troop : NetworkBehaviour
{
    public event EventHandler OnTroopPlaced;
    private ulong ownerClientId;

    private bool isPlaced;
    private bool changedGridPosition;
    private bool isOwnedByPlayer;
    private bool isVisibleToOpponent;

    [SerializeField] private TroopSO troopSO;
    [SerializeField] private Transform troopCenterPoint;

    private Unit[] unitsInTroop;

    private GridPosition currentGridPosition;

    private void Awake() {
        unitsInTroop = GetComponentsInChildren<Unit>();
    }

    public override void OnNetworkSpawn() {
        //BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {
        if(this == PlayerActionsManager.LocalInstance.GetTroopToSpawn()) {
            HandlePositionOnGrid();
            HandleTroopPlacement();
        }
    }

    public void HandlePositionOnGrid() {
        GridPosition newGridPosition = BattleGrid.Instance.GetGridPosition(MousePositionManager.Instance.GetMousePositionWorldPoint());

        // Grid position is not a valid position
        if (!BattleGrid.Instance.IsValidTroopGridPositioning(newGridPosition)) return;

        // Troop was not set at a grid position yet
        if(currentGridPosition == null) {
            currentGridPosition = BattleGrid.Instance.GetGridPosition(MousePositionManager.Instance.GetMousePositionWorldPoint());
            BattleGrid.Instance.AddTroopAtGridPosition(currentGridPosition, this);
        }

        // Troop changed grid position
        if (newGridPosition != currentGridPosition) {
            // Unit changed grid position
            changedGridPosition = true;
            BattleGrid.Instance.TroopMovedGridPosition(this, currentGridPosition, newGridPosition);
            currentGridPosition = newGridPosition;
        }
    }

    private void HandleTroopPlacement() {
        if(currentGridPosition == null) {
            transform.position = MousePositionManager.Instance.GetMousePositionWorldPoint() - troopCenterPoint.localPosition;
            return;
        }

        if(changedGridPosition) {
            transform.position = BattleGrid.Instance.GetWorldPosition(currentGridPosition) - troopCenterPoint.localPosition;
            changedGridPosition = false;
            return;
        }
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if (!HiddenTacticsMultiplayer.Instance.IsMultiplayer()) return;

        if (BattleManager.Instance.IsBattlePhase()) {
            if (!isVisibleToOpponent) {
                // Make troop visible to opponent
                OnTroopPlaced?.Invoke(this, null);
                //transform.position = troopPosition;
            }
            isVisibleToOpponent = true;
        }
    }

    public void SetTroopOwnerClientId(ulong clientId) {
        ownerClientId = clientId;
        isOwnedByPlayer = (ownerClientId == NetworkManager.Singleton.LocalClientId);
    }

    public void PlaceTroop() {
        OnTroopPlaced?.Invoke(this, null);
    }

    public void SetTroopPosition(Vector3 troopTransformPosition) {
        transform.position = troopTransformPosition;
    }

    public bool IsOwnedByPlayer() {
        return isOwnedByPlayer;
    }

    public TroopSO GetTroopSO() {
        return troopSO;
    }

}
