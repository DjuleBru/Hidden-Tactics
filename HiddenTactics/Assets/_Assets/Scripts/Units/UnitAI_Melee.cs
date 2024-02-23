using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI_Melee : UnitAI
{
    protected void Update() {
        if (!IsServer) return;
        // AI runs only on server

        if (!unitActive | !unit.GetUnitIsBought()) return;

        switch (state.Value) {
            case State.idle:
                break;
            case State.moveForwards:

                if (unitTargetingSystem.GetTargetUnit() != null) {
                    //Unit has a valid target
                    ChangeState(State.moveToTarget);
                }

                break;
            case State.moveToTarget:

                if (unitTargetingSystem.GetTargetUnit() == null) {
                    ChangeState(State.moveForwards);
                }

                break;
            case State.attacking:

                if (unitAttack.GetAttackTarget() == null) {
                    // Unit attack has no target !
                    unitAttack.SetAttackTarget(unitTargetingSystem.GetTargetUnit());
                }

                if (unitTargetingSystem.GetTargetUnit() == null | !unitTargetingSystem.GetTargetUnitIsInRange()) {
                    // Unit has no attack targets or target attack unit is out of range
                    ChangeState(State.moveForwards);
                }

                if (unitAttack.GetAttackTarget().GetUnitIsDead()) {
                    // Unit attack target is dead !
                    unitAttack.SetAttackTarget(unitTargetingSystem.GetTargetUnit());
                }

                break;
            case State.dead:

                break;

        }

    }

    protected void FixedUpdate() {
        if (!IsServer) return;
        // AI runs only on server

        if (!unitActive | !unit.GetUnitIsBought()) return;

        if (state.Value == State.moveForwards) {
            unitMovement.MoveForwards();
        }

        if (state.Value == State.moveToTarget) {
            if (unitTargetingSystem.GetTargetUnit() != null) {
                unitMovement.MoveToTarget(unitTargetingSystem.GetTargetUnit().transform.position);

                if (unitTargetingSystem.GetClosestTargetDistance() < unit.GetUnitSO().mainAttackSO.meleeAttackRange) {
                    unitMovement.StopMoving();
                    unitAttack.SetAttackTarget(unitTargetingSystem.GetTargetUnit());
                    ChangeState(State.attacking);
                }
            }
        }
    }
}
