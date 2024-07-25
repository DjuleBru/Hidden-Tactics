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
        //unitMovement.MoveBehindTarget(closestPointOnTargetCollider, distanceBehindTarget);

        unitAI.ChangeState(UnitAI.State.jumping);

        unitAI.Jump(new Vector2(300000, 0));

        return TaskStatus.Success;
    }
}
