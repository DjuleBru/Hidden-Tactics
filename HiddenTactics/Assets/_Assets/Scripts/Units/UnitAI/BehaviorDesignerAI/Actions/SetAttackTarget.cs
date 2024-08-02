using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAttackTarget : Action {

    private UnitAttack unitAttack;
    private UnitTargetingSystem unitTargetingSystem;
    public UnitTargetingSystem.AttackMode attackMode;
    public bool randomTarget;

    public override void OnAwake() {
        unitAttack = GetComponent<UnitAttack>();
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
    }
    public override TaskStatus OnUpdate() {
        if(randomTarget) {

            if (attackMode == UnitTargetingSystem.AttackMode.mainAttack) {
                unitAttack.SetAttackTarget(unitTargetingSystem.GetRandomMainAttackTarget());
            }

            if (attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
                unitAttack.SetAttackTarget(unitTargetingSystem.GetRandomSideAttackTarget());
            }

        } else {

            if (attackMode == UnitTargetingSystem.AttackMode.mainAttack) {
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTarget());
            }

            if (attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
                unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTarget());
            }

        }

        return TaskStatus.Success;
    }
}
