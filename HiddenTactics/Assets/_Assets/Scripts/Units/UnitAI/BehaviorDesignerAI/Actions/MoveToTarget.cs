using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTarget : Action{

    private UnitMovement unitMovement;
    private UnitTargetingSystem targetingSystem;

    public override void OnAwake() {
        unitMovement = GetComponent<UnitMovement>();
        targetingSystem = GetComponent<UnitTargetingSystem>();
    }
    public override TaskStatus OnUpdate() {

        Collider2D targetCollider = (targetingSystem.GetMainAttackTarget() as MonoBehaviour).GetComponent<Collider2D>();
        Vector3 closestPointOnTargetCollider = targetCollider.ClosestPoint(transform.position);
        unitMovement.MoveToTarget(closestPointOnTargetCollider);

        return TaskStatus.Success;
    }
}
