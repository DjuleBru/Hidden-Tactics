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

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        unit = GetComponent<Unit>();
    }


    private void Start() {
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
    }

    public void MoveForwards() {
        Vector3 moveDir3D = new Vector3(moveDirMultiplier, 0, 0);
        MoveServerRpc(moveDir3D);
    }

    public void MoveToTarget(Vector3 targetPosition) {
        Vector3 moveDir3D = (targetPosition - transform.position).normalized;
        MoveServerRpc(moveDir3D);
    }


    [ServerRpc]
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
        rb.velocity = Vector3.zero;
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
