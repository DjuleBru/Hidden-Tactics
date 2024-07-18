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
            Debug.Log(this + " chaged state to " + state.ToString());
            unitAI.ChangeState(state);
        }

        return TaskStatus.Success;
    }
}
