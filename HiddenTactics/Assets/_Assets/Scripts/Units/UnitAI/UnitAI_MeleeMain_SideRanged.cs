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
        base.CheckConditionsBeforeSwitch();

        if(state.Value != State.blockedByBuilding) {
            CheckIfEnemyUnitsAreInRangedRange();
        }
    }

    protected override void MoveForwardsStateUpdate() {
        unitMovement.MoveForwards();

        if (unitTargetingSystem.GetMainAttackTarget() != null) {
            //Unit has a valid main attack target
            ChangeState(State.moveToMeleeTarget);
        }
    }

    protected override void MoveToMeleeTargetStateUpdate() {
        if (unitTargetingSystem.GetMainAttackTarget() == null) {
            // There is no more unit in melee range
            ChangeState(State.moveForwards);
        }
        else {
            unitMovement.MoveToTarget((unitTargetingSystem.GetMainAttackTarget() as MonoBehaviour).transform.position);
        }

        CheckIfTargetIsInMeleeAttackRange(mainAttackSO, true);
    }

    protected override void AttackingStateUpdate() {
        if (!foundRangedTarget) {
            // Unit is attacking in melee 

            if (unitTargetingSystem.GetMainAttackTarget() == null | !unitTargetingSystem.GetTargetUnitIsInRange(mainAttackSO)) {
                // Unit has no attack targets or target attack unit is out of range
                ChangeState(State.moveForwards);
                return;
            }

            if (unitAttack.GetAttackTarget().GetIsDead() && unitTargetingSystem.GetMainAttackTarget() != null) {
                // Unit attack target is dead and there are other target units!
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTarget());
            }

        }
        else {
            // Unit is attacking in ranged
            if (unitTargetingSystem.GetSideAttackTarget() == null | ammoCount == 0) {
                // Unit has no attack targets or has no more ammo
                ChangeState(State.moveForwards);
                return;
            }

            if (unitAttack.GetAttackTarget().GetIsDead() && unitTargetingSystem.GetSideAttackTarget() != null) {
                // Unit attack target is dead and there are other target units!
                unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTarget());
            }
        }
    }

    protected void CheckIfEnemyUnitsAreInRangedRange() {
        if (ammoCount == 0) {
            foundRangedTarget = false;
            return;
        }

        if (unitTargetingSystem.GetSideAttackTarget() != null && unitTargetingSystem.GetMainAttackTarget() == null) {
            foundRangedTarget = true;

            // THERE IS A UNIT IN RANGED RANGE AND NO UNIT IN MELEE 
            ActivateSideAttack();
            ChangeState(State.attackingRanged);
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
        if (state.Value == State.attackingRanged) {
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
