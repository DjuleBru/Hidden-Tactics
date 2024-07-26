using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : Action {
    public float distanceBehindTarget;
    private UnitAI_Jump unitAI;
    private UnitMovement unitMovement;
    private UnitTargetingSystem targetingSystem;

    public override void OnAwake() {
        unitAI = GetComponent<UnitAI_Jump>();
        unitMovement = GetComponent<UnitMovement>();
        targetingSystem = GetComponent<UnitTargetingSystem>();
    }


    public override TaskStatus OnUpdate() {

        Vector3 closestPointOnTargetCollider = Vector3.zero;
        Collider2D targetCollider = (targetingSystem.GetSpecialAttackTarget() as MonoBehaviour).GetComponent<Collider2D>();
        closestPointOnTargetCollider = targetCollider.ClosestPoint(transform.position);

        unitAI.ChangeState(UnitAI.State.jumping);

        Vector3 jumpDir = (closestPointOnTargetCollider - transform.position).normalized;
        unitAI.Jump(jumpDir);

        return TaskStatus.Success;
    }
}
