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

                if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                    //Unit has a valid target
                    ChangeState(State.moveToTarget);
                }

                break;
            case State.moveToTarget:

                if (unitTargetingSystem.GetMainAttackTargetUnit() == null) {
                    ChangeState(State.moveForwards);
                }

                break;
            case State.attacking:

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
                    if(unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                        unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
                    } else {
                        ChangeState(State.moveForwards);
                    }
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
            if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                unitMovement.MoveToTarget(unitTargetingSystem.GetMainAttackTargetUnit().transform.position);

                if (unitTargetingSystem.GetClosestTargetDistance() < unit.GetUnitSO().mainAttackSO.meleeAttackRange) {
                    unitMovement.StopMoving();
                    unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
                    ChangeState(State.attacking);
                }
            }
        }
    }
}
