using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachedDestinationPoint : Conditional
{
    private UnitMovement unitMovement;

    public override void OnAwake() {
        unitMovement = GetComponent<UnitMovement>();
    }
    public override TaskStatus OnUpdate() {
       if(Vector3.Distance(transform.position, unitMovement.GetDestinationPoint()) < 1f) {
            return TaskStatus.Success;
        } else {
            return TaskStatus.Failure;
        }
    }
}
