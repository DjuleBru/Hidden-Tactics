using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopMoving : Action
{
    private UnitMovement unitMovement;

    public override void OnAwake() {
        unitMovement = GetComponent<UnitMovement>();
    }
    public override TaskStatus OnUpdate() {
        unitMovement.StopMoving();
        return TaskStatus.Success;
    }
}
