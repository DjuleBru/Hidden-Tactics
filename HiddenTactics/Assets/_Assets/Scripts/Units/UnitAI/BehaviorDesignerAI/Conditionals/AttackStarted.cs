using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackStarted : Conditional
{
    private UnitAI unitAI;

    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
    }
    public override TaskStatus OnUpdate() {
        bool attackStarted = unitAI.GetAttackStarted();
        if(attackStarted) {
            return TaskStatus.Success;
        } else {
            return TaskStatus.Failure;
        }
    }
}
