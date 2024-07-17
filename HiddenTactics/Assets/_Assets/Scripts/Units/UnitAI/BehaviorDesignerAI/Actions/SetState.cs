using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetState : Action
{
    private UnitAI unitAI;
    public UnitAI.State state;

    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
    }
    public override TaskStatus OnUpdate() {
        bool isState = unitAI.IsState(state);

        if (!isState) {
            unitAI.ChangeState(state);
        }

        return TaskStatus.Failure;
    }
}
