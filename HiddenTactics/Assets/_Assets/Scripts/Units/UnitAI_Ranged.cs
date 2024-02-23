using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI_Ranged : UnitAI
{

    private bool foundMeleeTarget;
    private bool attackingInMelee;

    protected void Update() {

        if (!IsServer) return;
        // AI runs only on server

        if (!unitActive | !unit.GetUnitIsBought()) return;

        CheckIfEnemyUnitsAreInMeleeRange();

        switch (state.Value) {
            case State.idle:
                unitAttack.SetActiveAttackSO(mainAttackSO);
                break;
            case State.moveForwards:
                attackingInMelee = false;
                foundMeleeTarget = false;

                if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                    //Unit has a valid target for shooting
                    unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
                    unitAttack.SetActiveAttackSO(mainAttackSO);
                    ChangeState(State.attacking);
                    return;
                }

                break;
            case State.moveToTarget:
                attackingInMelee = true;

                if (unitTargetingSystem.GetSideAttackTargetUnit() == null) {
                    // There is no more unit in melee range

                    attackingInMelee = false;
                    foundMeleeTarget = false;
                    InvokeOnMainAttackActivated();
                    ChangeState(State.moveForwards);
                }

                break;
            case State.attacking:

                if (unitAttack.GetAttackTarget() == null) {
                    // Unit attack has no target !
                    unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
                }

                if(!foundMeleeTarget) {
                    attackingInMelee = false;

                    // Unit is attacking in ranged 

                    if (unitTargetingSystem.GetMainAttackTargetUnit() == null) {
                        // Unit has no attack targets
                        ChangeState(State.moveForwards);
                    }

                    if (unitAttack.GetAttackTarget().GetUnitIsDead() && unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                        // Unit attack target is dead and there are other target units!
                        unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
                    }

                } else {

                    attackingInMelee = true;
                    // Unit is attacking in melee
                    if (unitTargetingSystem.GetSideAttackTargetUnit() == null | !unitTargetingSystem.GetTargetUnitIsInRange(sideAttackSO)) {
                        // Unit has no attack targets or target attack unit is out of range
                        InvokeOnMainAttackActivated();
                        ChangeState(State.moveForwards);
                    }

                    if (unitAttack.GetAttackTarget().GetUnitIsDead()) {
                        // Unit attack target is dead !
                        if (unitTargetingSystem.GetSideAttackTargetUnit() != null) {
                            unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTargetUnit());
                        }
                        else {
                            attackingInMelee = false;
                            ChangeState(State.moveForwards);
                        }
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

            unitMovement.MoveToTarget(unitTargetingSystem.GetSideAttackTargetUnit().transform.position);

            if (unitTargetingSystem.GetClosestTargetDistance() < unit.GetUnitSO().sideAttackSO.meleeAttackRange) {
                unitMovement.StopMoving();
                unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTargetUnit());
                unitAttack.SetActiveAttackSO(sideAttackSO);
                ChangeState(State.attacking);
            }

        }
    }

    protected void CheckIfEnemyUnitsAreInMeleeRange() {
        if (attackingInMelee) return;

        if (unitTargetingSystem.GetSideAttackTargetUnit() != null) {
            foundMeleeTarget = true;
            InvokeOnSideAttackActivated();

            // THERE IS A UNIT IN MELEE RANGE
            ChangeState(State.moveToTarget);
            unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTargetUnit());
            unitAttack.SetActiveAttackSO(sideAttackSO);
        } else { 
            foundMeleeTarget = false;
        }

    }
}
