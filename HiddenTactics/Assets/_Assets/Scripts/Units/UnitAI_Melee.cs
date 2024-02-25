using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_Melee : UnitAI
{
    protected void Update() {
        if (!IsServer) return;
        // AI runs only on server

        if (!unitActive | !unit.GetUnitIsBought() | state.Value == State.dead) return;

        switch (state.Value) {
            case State.idle:
                break;
            case State.moveForwards:

                unitMovement.MoveForwards();
                if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                    //Unit has a valid target
                    ChangeState(State.moveToMeleeTarget);
                }

                break;
            case State.moveToMeleeTarget:

                if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                    unitMovement.MoveToTarget(unitTargetingSystem.GetMainAttackTargetUnit().transform.position);
                } else { 
                    ChangeState(State.moveForwards);
                }

                CheckIfTargetUnitIsInMeleeAttackRange(mainAttackSO, true);

                break;
            case State.attacking:

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
}
