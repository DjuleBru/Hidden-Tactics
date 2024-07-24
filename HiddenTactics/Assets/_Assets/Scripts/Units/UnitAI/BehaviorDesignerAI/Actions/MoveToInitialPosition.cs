using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToInitialPosition : Action
{
    private UnitMovement unitMovement;

    public override void OnAwake() {
        unitMovement = GetComponent<UnitMovement>();
    }
    public override TaskStatus OnUpdate() {
        unitMovement.MoveToInitialPosition();

        return TaskStatus.Success;
    }
}
