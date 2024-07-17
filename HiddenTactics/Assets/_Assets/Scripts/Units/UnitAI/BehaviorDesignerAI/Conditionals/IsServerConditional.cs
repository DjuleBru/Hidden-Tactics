using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Netcode;

public class IsServerConditional : Conditional
{
    private Unit unit;

    public override void OnAwake() {
        unit = GetComponent<Unit>();
    }
    public override TaskStatus OnUpdate() {
        bool isServer = unit.GetIsServer();

        if(isServer) {
            return TaskStatus.Success;
        } else {
            return TaskStatus.Failure;
        }
    }
}
