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
    bool isOwnedByPlayer;
    [SerializeField] private Transform troopCenterPoint;

    private Unit[] unitsInTroop;

    private void Awake() {
        unitsInTroop = GetComponentsInChildren<Unit>();
    }

    private void Update() {
        if (!isPositioned) {
            if(isOwnedByPlayer) {
                transform.position = MousePositionManager.Instance.GetMousePositionWorldPoint() - troopCenterPoint.localPosition;
            }
        }
    }

    public void SetTroopOwnerClientId(ulong clientId) {
        ownerClientId = clientId;
        isOwnedByPlayer = (ownerClientId == NetworkManager.Singleton.LocalClientId);
    }

    public void SetIsPositioned() {
        isPositioned = true;
    }

    public void PlaceTroop() {
        PlaceTroopServerRpc(transform.position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceTroopServerRpc(Vector3 troopPosition) {
        Debug.Log("placetroopserverRpc");
        PlaceTroopClientRpc(troopPosition);
    }

    [ClientRpc]
    private void PlaceTroopClientRpc(Vector3 troopPosition) {
        Debug.Log("placetroopclientRpc");
        transform.position = troopPosition;
        isPositioned = true;
        OnTroopPlaced?.Invoke(this, null);
    }

}
