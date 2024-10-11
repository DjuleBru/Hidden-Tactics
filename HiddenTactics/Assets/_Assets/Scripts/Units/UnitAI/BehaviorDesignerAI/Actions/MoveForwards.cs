using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForwards : Action
{
    private UnitMovement unitMovement;
    private UnitAI unitAI;

    public override void OnAwake() {
        unitMovement = GetComponent<UnitMovement>();
        unitAI = GetComponent<UnitAI>();
    }
    public override TaskStatus OnUpdate() {
        if(unitAI.IsMovingForwards()) {
            unitMovement.MoveForwards();
        }

        return TaskStatus.Success;
    }
}
