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
        bool hasAttackTarget = (unitAttack.GetAttackTarget() != null && !unitAttack.GetAttackTarget().GetIsDead());

        if (hasAttackTarget) {

            // Check if attack target has changeds
            if(target != null) {

                //Debug.Log("target " + (target as MonoBehaviour).gameObject.GetInstanceID());
                //Debug.Log("GetMainAttackTarget " + (unitTargetingSystem.GetMainAttackTarget() as MonoBehaviour).gameObject.GetInstanceID());

                if (attackMode == UnitTargetingSystem.AttackMode.mainAttack) {

                    if (target != unitTargetingSystem.GetMainAttackTarget()) {
                        // Attack target changed
                        return TaskStatus.Failure;
                    } else {
                        return TaskStatus.Success;
                    }
                }

                 if(attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
                    if (target != unitTargetingSystem.GetSideAttackTarget()) {
                        // Attack target changed
                        return TaskStatus.Failure;
                    }
                    else {
                        return TaskStatus.Success;
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
