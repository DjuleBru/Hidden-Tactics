using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownTimer : Action
{
    private float specialTimer;
    public float timerResetValue;
    private UnitAI unitAI;
    public override void OnAwake() {
        unitAI = GetComponent<UnitAI>();
    }

    public override TaskStatus OnUpdate() {
        specialTimer = unitAI.GetSpecialTimer() - Time.deltaTime;
        unitAI.SetSpecialTimer(specialTimer);

        if (unitAI.GetSpecialTimer() < 0) {
            unitAI.SetSpecialTimer(timerResetValue);
            return TaskStatus.Success;
        }
        else {
            return TaskStatus.Failure;
        }

    }
}
