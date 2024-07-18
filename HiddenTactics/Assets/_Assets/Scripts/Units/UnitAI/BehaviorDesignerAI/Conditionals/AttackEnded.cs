using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnded : Conditional
{
    private UnitAI unitAI;

    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
    }
    public override TaskStatus OnUpdate() {
        bool attackEnded = unitAI.GetAttackEnded();

        if (attackEnded) {
            return TaskStatus.Success;
        }
        else {
            return TaskStatus.Failure;
        }
    }
}
