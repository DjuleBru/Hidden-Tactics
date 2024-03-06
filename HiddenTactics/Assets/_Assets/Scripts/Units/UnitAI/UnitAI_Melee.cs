using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_Melee : UnitAI
{

    protected override void MoveForwardsStateUpdate() {
        unitMovement.MoveForwards();
        if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
            //Unit has a valid target
            ChangeState(State.moveToMeleeTarget);
        }
    }

    protected override void MoveToMeleeTargetStateUpdate() {
        if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {

            Collider2D targetCollider = (unitTargetingSystem.GetMainAttackTargetUnit() as MonoBehaviour).GetComponent<Collider2D>();
            Vector3 closestPointOnTargetCollider = targetCollider.ClosestPoint(transform.position);
            unitMovement.MoveToTarget(closestPointOnTargetCollider);
        }
        else {
            ChangeState(State.moveForwards);
        }

        CheckIfTargetIsInMeleeAttackRange(mainAttackSO, true);
    }

    protected override void AttackingStateUpdate() {

        if (attackStarted) return;

        if (unitAttack.GetAttackTarget() == null) {
            // Unit attack has no target !
            unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
        }

        if (unitTargetingSystem.GetMainAttackTargetUnit() == null | !unitTargetingSystem.GetTargetUnitIsInRange(mainAttackSO)) {
            // Unit has no attack targets or target attack unit is out of range
            ChangeState(State.moveForwards);
        }

        if(unitAttack.GetAttackTarget() != null) {

            if (unitAttack.GetAttackTarget().GetIsDead()) {
                // Attack target is dead !
                if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                    unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
                }
                else {
                    ChangeState(State.moveForwards);
                }
            }
        }
    }
}
