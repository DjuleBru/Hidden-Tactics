using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTimer : Action
{

    private UnitAI unitAI;
    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
    }

    public override TaskStatus OnUpdate() {
        if (unitAI.GetSpecialTimer() != 0) {
            unitAI.SetSpecialTimer(0f);
            return TaskStatus.Success;
        }
        else {
            return TaskStatus.Failure;
        }

    }
}
