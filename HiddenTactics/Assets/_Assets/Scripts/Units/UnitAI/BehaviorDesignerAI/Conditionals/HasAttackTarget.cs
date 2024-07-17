using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasAttackTarget : Conditional {

    private UnitAttack unitAttack;
    private UnitTargetingSystem unitTargetingSystem;

    public override void OnAwake() {
        unitAttack = GetComponent<UnitAttack>();
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
    }
    public override TaskStatus OnUpdate() {
        // Has attack target if : target is set, not dead, and in range
        bool hasAttackTarget = (unitAttack.GetAttackTarget() != null && !unitAttack.GetAttackTarget().GetIsDead() && unitTargetingSystem.GetTargetUnitIsInRange(unitAttack.GetActiveAttackSO()));

        if (hasAttackTarget) {
            return TaskStatus.Success;
        }
        else {
            return TaskStatus.Failure;
        }

    }
}
