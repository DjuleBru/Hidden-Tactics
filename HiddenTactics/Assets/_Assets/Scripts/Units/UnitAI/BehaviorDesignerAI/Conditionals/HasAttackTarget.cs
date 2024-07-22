using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasAttackTarget : Conditional {

    private UnitAttack unitAttack;
    private ITargetable target;
    private UnitTargetingSystem unitTargetingSystem;
    public UnitTargetingSystem.AttackMode attackMode;

    public override void OnAwake() {
        unitAttack = GetComponent<UnitAttack>();
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
    }
    public override TaskStatus OnUpdate() {
        // Has attack target if : target is set, not dead, and in range
        bool hasAttackTarget = (unitAttack.GetAttackTarget() != null && !unitAttack.GetAttackTarget().GetIsDead() && unitTargetingSystem.GetTargetUnitIsInRange(unitAttack.GetActiveAttackSO()));


        if (hasAttackTarget) {

            // Check if attack target has changeds
            if(target != null) {

                if(attackMode == UnitTargetingSystem.AttackMode.mainAttack) {
                    if (target != unitTargetingSystem.GetMainAttackTarget()) {
                        // Attack target changed
                        return TaskStatus.Failure;
                    }
                }

                 if(attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
                    if (target != unitTargetingSystem.GetSideAttackTarget()) {
                        // Attack target changed
                        return TaskStatus.Failure;
                    }
                }

            }

            target = unitAttack.GetAttackTarget();

            return TaskStatus.Success;
        }
        else {
            return TaskStatus.Failure;
        }

    }
}
