using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_MeleeMain_SideRanged : UnitAI
{

    [SerializeField] private int ammoMax;

    private int ammoCount;
    private bool foundRangedTarget;

    protected override void Awake() {
        base.Awake();
        ammoCount = ammoMax;
    }

    protected override void CheckConditionsBeforeSwitch() {
        CheckIfEnemyUnitsAreInRangedRange();
    }

    protected override void MoveForwardsStateUpdate() {
        unitMovement.MoveForwards();

        if (unitTargetingSystem.GetMainAttackTargetUnit() != null) {
            //Unit has a valid main attack target
            ChangeState(State.moveToMeleeTarget);
        }
    }

    protected override void MoveToMeleeTargetStateUpdate() {
        if (unitTargetingSystem.GetMainAttackTargetUnit() == null) {
            // There is no more unit in melee range
            ChangeState(State.moveForwards);
        }
        else {
            unitMovement.MoveToTarget(unitTargetingSystem.GetMainAttackTargetUnit().transform.position);
        }

        CheckIfTargetUnitIsInMeleeAttackRange(mainAttackSO, true);
    }

    protected override void AttackingStateUpdate() {
        if (!foundRangedTarget) {
            // Unit is attacking in melee 

            if (unitTargetingSystem.GetMainAttackTargetUnit() == null | !unitTargetingSystem.GetTargetUnitIsInRange(mainAttackSO)) {
                // Unit has no attack targets or target attack unit is out of range
                ChangeState(State.moveForwards);
            }

            if (unitAttack.GetAttackTarget().GetUnitIsDead() && unitTargetingSystem.GetMainAttackTargetUnit() != null) {
                // Unit attack target is dead and there are other target units!
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
            }

        }
        else {
            // Unit is attacking in ranged
            if (unitTargetingSystem.GetSideAttackTargetUnit() == null | ammoCount == 0) {
                // Unit has no attack targets or has no more ammo
                ChangeState(State.moveForwards);
            }

            if (unitAttack.GetAttackTarget().GetUnitIsDead() && unitTargetingSystem.GetSideAttackTargetUnit() != null) {
                // Unit attack target is dead and there are other target units!
                unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTargetUnit());
            }
        }
    }

    protected void CheckIfEnemyUnitsAreInRangedRange() {
        if (ammoCount == 0) {
            foundRangedTarget = false;
            return;
        }

        if (unitTargetingSystem.GetSideAttackTargetUnit() != null && unitTargetingSystem.GetMainAttackTargetUnit() == null) {
            foundRangedTarget = true;

            // THERE IS A UNIT IN RANGED RANGE AND NO UNIT IN MELEE 
            ActivateSideAttack();
            ChangeState(State.attacking);
        } else {
            foundRangedTarget = false;
        }
    }

    protected override void UnitAttack_OnUnitAttackEnded(object sender, EventArgs e) {
        ammoCount--;
    }

    [ClientRpc]
    protected override void ChangeStateClientRpc() {
        base.ChangeStateClientRpc();
        if (state.Value == State.idle) {
            foundRangedTarget = false;
            ammoCount = ammoMax;
        }
        if (state.Value == State.attacking) {
            unitMovement.StopMoving();
        }
        if (state.Value == State.moveToMeleeTarget) {
            ammoCount = ammoMax;
        }
        if (state.Value == State.moveForwards) {
            ActivateMainAttack();
            foundRangedTarget = false;
        }
        if (state.Value == State.dead) {
            foundRangedTarget = false;
        }
    }
}
