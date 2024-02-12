using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Troop : NetworkBehaviour
{
    public event EventHandler OnTroopPlaced;

    private ulong ownerClientId;

    private bool isPositioned;
    private bool isOwnedByPlayer;
    private bool isVisibleToOpponent;

    [SerializeField] private Transform troopCenterPoint;

    private Unit[] unitsInTroop;

    private Vector3 troopPosition;

    private void Awake() {
        unitsInTroop = GetComponentsInChildren<Unit>();
    }
    public override void OnNetworkSpawn() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }


    private void Update() {
        HandleTroopPositioning();
    }

    private void HandleTroopPositioning() {
        if (!isPositioned) {
            if (isOwnedByPlayer) {
                transform.position = MousePositionManager.Instance.GetMousePositionWorldPoint() - troopCenterPoint.localPosition;
            }
        }
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if (BattleManager.Instance.IsBattlePhase()) {
            if (!isVisibleToOpponent) {
                // Make troop visible to opponent
                OnTroopPlaced?.Invoke(this, null);
                transform.position = troopPosition;
            }
            isVisibleToOpponent = true;
        }
    }

    public void SetTroopOwnerClientId(ulong clientId) {
        ownerClientId = clientId;
        isOwnedByPlayer = (ownerClientId == NetworkManager.Singleton.LocalClientId);
    }

    public void PlaceTroop() {
        PlaceTroopServerRpc(transform.position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceTroopServerRpc(Vector3 troopPosition) {
        PlaceTroopClientRpc(troopPosition);
    }

    [ClientRpc]
    private void PlaceTroopClientRpc(Vector3 troopPosition) {
        this.troopPosition = troopPosition;
        isPositioned = true;
        if (isOwnedByPlayer) {
            transform.position = troopPosition;
            OnTroopPlaced?.Invoke(this, null);
        }
    }

    public bool IsOwnedByPlayer() {
        return isOwnedByPlayer;
    }

}
