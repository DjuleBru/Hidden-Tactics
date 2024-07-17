using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsStateConditional : Conditional
{
    private UnitAI unitAI;
    public UnitAI.State state;

    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
    }
    public override TaskStatus OnUpdate() {
        bool isState = unitAI.IsState(state);

        if (isState) {
            return TaskStatus.Success;
        }
        else {
            return TaskStatus.Failure;
        }
    }
}

