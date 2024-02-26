using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class UnitAI_Ranged : UnitAI
{

    private bool foundMeleeTarget;

    protected override void CheckConditionsBeforeSwitch() {
        CheckIfEnemyUnitsAreInMeleeRange();
    }

    protected override void MoveForwardsStateUpdate() {
        unitMovement.MoveForwards();

        if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
            //Unit has a valid target for shooting
            ChangeState(State.attacking);
            return;
        }
    }

    protected override void MoveToMeleeTargetStateUpdate() {
        if (unitTargetingSystem.GetSideAttackTargetUnit() == null) {
            // There is no more unit in melee range
            ActivateMainAttack();
            ChangeState(State.moveForwards);
        }
        else {
            unitMovement.MoveToTarget(unitTargetingSystem.GetSideAttackTargetUnit().transform.position);
        }

        CheckIfTargetUnitIsInMeleeAttackRange(sideAttackSO, false);
    }

    protected override void AttackingStateUpdate() {
        if (unitAttack.GetAttackTarget() == null && unitTargetingSystem.GetMainAttackTargetUnit() != null) {
            // Unit attack has not been set yet
            unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
        }

        if (!foundMeleeTarget) {
            // Unit is attacking in ranged 

            if (unitTargetingSystem.GetMainAttackTargetUnit() == null) {
                // Unit has no more attack targets
                ChangeState(State.moveForwards);
            }

            if (unitAttack.GetAttackTarget().GetUnitIsDead() && unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                // Unit attack target is dead and there are other target units!
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
            }

        }
        else {
            // Unit is attacking in melee
            if (unitTargetingSystem.GetSideAttackTargetUnit() == null | !unitTargetingSystem.GetTargetUnitIsInRange(sideAttackSO)) {
                // Unit has no attack targets or target attack unit is out of range
                ChangeState(State.moveForwards);
            }

            if (unitAttack.GetAttackTarget().GetUnitIsDead() && unitTargetingSystem.GetSideAttackTargetUnit() != null) {
                // Unit attack target is dead and there are other target units!
                unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTargetUnit());
            }
            else {
                ChangeState(State.moveForwards);
            }
        }
    }

    protected void CheckIfEnemyUnitsAreInMeleeRange() {
        if (unitTargetingSystem.GetSideAttackTargetUnit() != null) {
            foundMeleeTarget = true;

            // THERE IS A UNIT IN MELEE RANGE
            ActivateSideAttack();
            ChangeState(State.moveToMeleeTarget);
        } else { 
            foundMeleeTarget = false;
        }
    }

    [ClientRpc]
    protected override void ChangeStateClientRpc() {
        base.ChangeStateClientRpc();
        if (state.Value == State.idle) {
            foundMeleeTarget = false;
        }
        if (state.Value == State.attacking) {
            unitMovement.StopMoving();
        }
        if (state.Value == State.moveToMeleeTarget) {

        }
        if (state.Value == State.moveForwards) {
            foundMeleeTarget = false;
        }
        if (state.Value == State.dead) {
            foundMeleeTarget = false;
        }
    }
}
