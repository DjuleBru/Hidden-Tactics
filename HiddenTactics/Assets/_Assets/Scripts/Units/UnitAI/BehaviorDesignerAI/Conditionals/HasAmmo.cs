using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasAmmo : Conditional
{
    private UnitAI_RangedWithAmmo unitAI;

    public override void OnAwake() {
        unitAI = GetComponent<UnitAI_RangedWithAmmo>();
    }
    public override TaskStatus OnUpdate() {
       
        if(unitAI.GetAmmoCount() > 0) {
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}
