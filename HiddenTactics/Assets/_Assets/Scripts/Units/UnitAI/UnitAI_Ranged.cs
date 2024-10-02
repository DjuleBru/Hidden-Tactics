using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class UnitAI_Ranged : UnitAI
{

    private bool foundMeleeTarget;

    protected override void CheckConditionsBeforeSwitch() {
        base.CheckConditionsBeforeSwitch();

        CheckIfEnemyUnitsAreInMeleeRange();
    }

    protected override void CheckIfBuildingBlocksMovement() {
        // Determine nextGridPosition in function of unit belonging
        GridPosition nextGridPosition = new GridPosition(0, 0);

        if (unit.IsOwnedByPlayer()) {
            nextGridPosition = new GridPosition(unit.GetCurrentGridPosition().x + 1, unit.GetCurrentGridPosition().y);
        }
        else {
            nextGridPosition = new GridPosition(unit.GetCurrentGridPosition().x - 1, unit.GetCurrentGridPosition().y);
        }

        if (BattleGrid.Instance.IsValidGridPosition(nextGridPosition)) {
            // This GridPosition is a valid grid position
            Building building = BattleGrid.Instance.GetBuildingAtGridPosition(nextGridPosition);

            if(state.Value != State.attackingMelee || state.Value != State.attackingRanged) {

                // This ranged unit is not attacking
                if (state.Value != State.blockedByBuilding) {
                    // This ranged unit is not blocked by building yet

                    if (building != null) {
                        // There is a building that blocks the unit

                        if (building.GetBuildingSO().buildingBlocksUnitMovement) {
                            ChangeState(State.blockedByBuilding);
                        }
                    };
                }
                else {
                    // This ranged unit is blocked by building
                    if (building != null) {
                        ChangeState(State.moveForwards);
                    }
                }

            }
        }
    }

    protected override void BlockedByBuildingStateUpdate() {
        if (unitTargetingSystem.GetMainAttackTarget() != null) {
            // Unit has a target to shoot
            ChangeState(State.attackingRanged);
            return;
        }
    }

    protected override void MoveForwardsStateUpdate() {
        unitMovement.MoveForwards();

        if (unitTargetingSystem.GetMainAttackTarget() != null) {
            //Unit has a valid target for shooting
            ChangeState(State.attackingRanged);
            return;
        }
    }

    protected override void MoveToMeleeTargetStateUpdate() {
        if (unitTargetingSystem.GetSideAttackTarget() == null) {
            // There is no more unit in melee range
            ActivateMainAttack();
            ChangeState(State.moveForwards);
        }
        else {
            unitMovement.MoveToTarget((unitTargetingSystem.GetSideAttackTarget() as MonoBehaviour).transform.position);
        }

        CheckIfTargetIsInMeleeAttackRange(sideAttackSO, false);
    }

    protected override void AttackingStateUpdate() {
        if (unitAttack.GetAttackTarget() == null && unitTargetingSystem.GetMainAttackTarget() != null) {
            // Unit attack has not been set yet
            unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTarget());
        }

        if (!foundMeleeTarget) {

            if (unitTargetingSystem.GetMainAttackTarget() == null) {
                // Unit has no more attack targets
                ChangeState(State.moveForwards);
                return;
            }

            if (unitAttack.GetAttackTarget().GetIsDead() && unitTargetingSystem.GetMainAttackTarget() != null) {
                // Unit attack target is dead and there are other target units!
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTarget());
            }

            if (unitAttack.GetAttackTarget() != unitTargetingSystem.GetMainAttackTarget()) {
                // Unit attack target is not targeted anymore.
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTarget());
            }

        }
        else {

            if (unitTargetingSystem.GetSideAttackTarget() == null | !unitTargetingSystem.GetTargetUnitIsInRange(sideAttackSO)) {
                // Unit has no attack targets or target attack unit is out of range
                ChangeState(State.moveForwards);
                return;
            }

            if (unitAttack.GetAttackTarget().GetIsDead() && unitTargetingSystem.GetSideAttackTarget() != null) {
                // Unit attack target is dead and there are other target units!
                unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTarget());
                return;
            }
            else {
                ChangeState(State.moveForwards);
            }
        }
    }

    protected void CheckIfEnemyUnitsAreInMeleeRange() {
        if (unitTargetingSystem.GetSideAttackTarget() != null) {
            foundMeleeTarget = true;

            // THERE IS A UNIT IN MELEE RANGE
            ActivateSideAttack();
            ChangeState(State.moveToMeleeTarget);
            return;
        }
    }

    //[ClientRpc]
    protected override void ChangeState() {
        base.ChangeState();

        if (state.Value == State.idle) {
            foundMeleeTarget = false;
        }
        if (state.Value == State.attackingRanged) {
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
