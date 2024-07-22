using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialActive : Conditional
{
    private UnitAI unitAI;
    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
    }

    public override TaskStatus OnUpdate() {
        if(unitAI.GetSpecialActive()) {
            return TaskStatus.Success;
        } else {
            return TaskStatus.Failure;
        }
    }
}
