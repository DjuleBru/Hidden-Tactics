using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForwards : Action
{
    private UnitMovement unitMovement;

    public override void OnAwake() {
        unitMovement = GetComponent<UnitMovement>();
    }
    public override TaskStatus OnUpdate() {
        unitMovement.MoveForwards();

        return TaskStatus.Success;
    }
}
