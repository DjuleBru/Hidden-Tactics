using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAttackMode : Action
{
    private UnitAI unitAI;
    private UnitTargetingSystem unitTargetingSystem;
    public UnitTargetingSystem.AttackMode attackMode;

    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
    }
    public override TaskStatus OnUpdate() {
        if (attackMode == UnitTargetingSystem.AttackMode.mainAttack) {
            unitAI.ActivateMainAttack();
        }

        if (attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
            unitAI.ActivateSideAttack();
        }

        return TaskStatus.Success;
    }
}
