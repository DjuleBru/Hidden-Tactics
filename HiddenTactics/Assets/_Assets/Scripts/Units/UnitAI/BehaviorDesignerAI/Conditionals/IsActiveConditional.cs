using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsActiveConditional : Conditional {

    private UnitAI unitAI;

    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
    }
    public override TaskStatus OnUpdate() {
        bool isActive = unitAI.UnitIsActive();

        if (isActive) {
            return TaskStatus.Success;
        }
        else {
            return TaskStatus.Failure;
        }
    }
}
