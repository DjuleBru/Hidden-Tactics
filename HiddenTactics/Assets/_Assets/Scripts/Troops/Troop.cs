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
    private bool isOwnedByPlayer;

    private bool isVisibleToOpponent;

    [SerializeField] private bool debugMode;
    [SerializeField] private TroopSO troopSO;
    [SerializeField] private Transform troopCenterPoint;

    private Unit[] unitsInTroop;

    private GridPosition currentGridPosition;
    private Transform battlefieldOwner;
    private Vector3 battlefieldOffset;

    private void Awake() {
        unitsInTroop = GetComponentsInChildren<Unit>();
    }

    private void Start() {
        if (debugMode) {
            PlaceTroop();
            isOwnedByPlayer = false;
        }
    }

    public override void OnNetworkSpawn() {
        //BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {
        if(!isPlaced) {
            HandlePositioningOnGrid();
            HandleTroopPositionDuringPlacement();
        } else {
            HandleTroopPosition();
        }
    }

    public void HandlePositioningOnGrid() {
        if (!isOwnedByPlayer) return;

        GridPosition newGridPosition = MousePositionManager.Instance.GetMouseGridPosition();

        // Grid position is not a valid position
        if (!BattleGrid.Instance.IsValidPlayerGridPosition(newGridPosition)) return;

        // Troop was not set at a grid position yet
        if(currentGridPosition == null) {
            currentGridPosition = MousePositionManager.Instance.GetMouseGridPosition();
            BattleGrid.Instance.AddTroopAtGridPosition(currentGridPosition, this);
        }

        // Troop changed grid position
        if (newGridPosition != currentGridPosition) {
            // Unit changed grid position
            BattleGrid.Instance.TroopMovedGridPosition(this, currentGridPosition, newGridPosition);
            currentGridPosition = newGridPosition;
        }
    }

    private void HandleTroopPositionDuringPlacement() {
        if (currentGridPosition == null) {
            if (!isOwnedByPlayer) return;
            transform.position = MousePositionManager.Instance.GetMousePositionWorldPoint() - troopCenterPoint.localPosition;
        } else {
            transform.position = BattleGrid.Instance.GetWorldPosition(currentGridPosition) - troopCenterPoint.localPosition;
        }
    }

    private void HandleTroopPosition() {
        transform.position = battlefieldOwner.position + battlefieldOffset;
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
        currentGridPosition = BattleGrid.Instance.GetGridPosition(troopCenterPoint.position);

        isPlaced = true;

        SetTroopBattlefieldParent(currentGridPosition);
    }

    public void SetTroopGridPosition(GridPosition troopGridPosition) {
        Vector3 troopWorldPosition = BattleGrid.Instance.GetWorldPosition(troopGridPosition);

        currentGridPosition = troopGridPosition;
        transform.position = troopWorldPosition - troopCenterPoint.localPosition;
    }

    private void SetTroopBattlefieldParent(GridPosition troopGridPosition) {
        if (troopGridPosition.x >= 6) {
            battlefieldOwner = BattleGrid.Instance.GetOpponentGridOrigin();
        }
        else {
            battlefieldOwner = BattleGrid.Instance.GetPlayerGridOrigin();
        }
        battlefieldOffset = transform.position - battlefieldOwner.position;
    }

    public GridPosition GetTroopGridPosition() {
        return currentGridPosition;
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


}
