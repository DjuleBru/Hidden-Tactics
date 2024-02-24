using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitMovement : NetworkBehaviour {

    private Unit unit;
    private Rigidbody2D rb;
    private Vector3 moveDir2D;
    private int moveDirMultiplier;

    private bool dazed;

    private Vector3 moveDir;
    private Vector3 watchDir;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        unit = GetComponent<Unit>();
    }

    private void Start() {
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
    }

    private void LateUpdate() {
        if (IsServer) {
            if (!dazed) {
                MoveServerRpc(moveDir);
            }
        }
    }

    public void MoveForwards() {
        moveDir = new Vector3(moveDirMultiplier, 0, 0);
    }

    public void MoveToTarget(Vector3 targetPosition) {
        moveDir = (targetPosition - transform.position).normalized;
    }


    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(Vector3 moveDir3DNormalized) {
        rb.velocity = moveDir3DNormalized * unit.GetUnitSO().unitMoveSpeed * Time.fixedDeltaTime;
        MoveClientRpc(moveDir3DNormalized);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector3 moveDir3DNormalized) {
        if (!IsServer) {
            // Mirror movedir 
            moveDir3DNormalized.x = -moveDir3DNormalized.x;
        }

        moveDir2D = new Vector2(moveDir3DNormalized.x, moveDir3DNormalized.y);
    }

    public void StopMoving() {
        moveDir = Vector3.zero;
    }

    public void SetDazed(bool dazed) {
        this.dazed = dazed;
    }

    public void SetWatchDir(Transform targetTransform) {
        Vector3 watchDir = (targetTransform.position - transform.position).normalized;
        SetWatchDirServerRpc(watchDir);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetWatchDirServerRpc(Vector3 watchDir3DNormalized) {
        SetWatchDirClientRpc(watchDir3DNormalized);
    }

    [ClientRpc]
    private void SetWatchDirClientRpc(Vector3 watchDir3DNormalized) {
        if (!IsServer) {
            // Mirror watchdir 
            watchDir3DNormalized.x = -watchDir3DNormalized.x;
        }

        watchDir = new Vector2(watchDir3DNormalized.x, watchDir3DNormalized.y);
    }

    public Vector2 GetWatchDir2D() {
        return watchDir;
    }

    public Vector2 GetMoveDir2D() {
        return moveDir2D;
    }


    private void Unit_OnUnitPlaced(object sender, EventArgs e) {
        if (unit.GetParentTroop().IsOwnedByPlayer()) {
            moveDirMultiplier = 1;
        }
        else {
            moveDirMultiplier = -1;
        }
    }

}
