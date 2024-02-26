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
            unitMovement.MoveToTarget(unitTargetingSystem.GetMainAttackTargetUnit().transform.position);
        }
        else {
            ChangeState(State.moveForwards);
        }

        CheckIfTargetUnitIsInMeleeAttackRange(mainAttackSO, true);
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

        if (unitAttack.GetAttackTarget().GetUnitIsDead()) {
            // Unit attack target is dead !
            if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
            }
            else {
                ChangeState(State.moveForwards);
            }
        }
    }

}
