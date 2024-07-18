using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsAttackMode : Conditional
{
    private UnitAI unitAI;
    public UnitTargetingSystem.AttackMode attackMode;

    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
    }
    public override TaskStatus OnUpdate() {
        if(attackMode == UnitTargetingSystem.AttackMode.mainAttack) {
            if(unitAI.GetMainAttackActive()) {
                return TaskStatus.Success;
            }
        }

        if(attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
            if (unitAI.GetSideAttackActive()) {
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Failure;
    }
}
