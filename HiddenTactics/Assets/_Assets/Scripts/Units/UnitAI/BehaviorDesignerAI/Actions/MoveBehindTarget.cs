using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehindTarget : Action
{
    private UnitMovement unitMovement;
    private UnitTargetingSystem targetingSystem;
    public UnitTargetingSystem.AttackMode attackMode;

    public float distanceBehindTarget;

    public override void OnAwake() {
        unitMovement = GetComponent<UnitMovement>();
        targetingSystem = GetComponent<UnitTargetingSystem>();
    }

    public override TaskStatus OnUpdate() {
        Vector3 closestPointOnTargetCollider = Vector3.zero;

        if (attackMode == UnitTargetingSystem.AttackMode.mainAttack) {
            Collider2D targetCollider = (targetingSystem.GetMainAttackTarget() as MonoBehaviour).GetComponent<Collider2D>();
            closestPointOnTargetCollider = targetCollider.ClosestPoint(transform.position);
        }

        if (attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
            Collider2D targetCollider = (targetingSystem.GetSideAttackTarget() as MonoBehaviour).GetComponent<Collider2D>();
            closestPointOnTargetCollider = targetCollider.ClosestPoint(transform.position);
        }

        if (attackMode == UnitTargetingSystem.AttackMode.specialAttack) {
            Collider2D targetCollider = (targetingSystem.GetSpecialAttackTarget() as MonoBehaviour).GetComponent<Collider2D>();
            closestPointOnTargetCollider = targetCollider.ClosestPoint(transform.position);
        }

        unitMovement.MoveBehindTarget(closestPointOnTargetCollider, distanceBehindTarget);
        return TaskStatus.Success;
    }
}
