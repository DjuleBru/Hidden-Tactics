using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour {

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
        moveDir2D = new Vector2(moveDirMultiplier, 0);

        Vector3 moveDir3D = new Vector3(moveDir2D.x, moveDir2D.y, 0);

        rb.velocity = moveDir3D * unit.GetUnitSO().unitMoveSpeed * Time.fixedDeltaTime;
    }

    public void MoveToTarget(Vector3 targetPosition) {
        Vector3 moveDir3D = (targetPosition - transform.position).normalized;

        rb.velocity = moveDir3D * unit.GetUnitSO().unitMoveSpeed * Time.fixedDeltaTime;
    }

    public float GetDistanceToTarget(Vector3 targetPosition) {
        return Vector3.Distance(transform.position, targetPosition);
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
