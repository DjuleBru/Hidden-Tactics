using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasTarget : Conditional {

    private UnitTargetingSystem unitTargetingSystem;
    public UnitTargetingSystem.AttackMode attackMode;

    public override void OnAwake() {
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
    }
    public override TaskStatus OnUpdate() {
        bool hasMeleeTarget = false;

       if (attackMode == UnitTargetingSystem.AttackMode.mainAttack) {
            hasMeleeTarget = unitTargetingSystem.GetMainAttackTarget() != null;
       }
       if(attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
            hasMeleeTarget = unitTargetingSystem.GetSideAttackTarget() != null;
       }

       if (hasMeleeTarget) {
            return TaskStatus.Success;
        } else {
            return TaskStatus.Failure;
        }

    }
}
