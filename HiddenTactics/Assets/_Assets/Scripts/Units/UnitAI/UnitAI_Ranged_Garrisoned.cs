using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI_Ranged_Garrisoned : UnitAI_Ranged
{
    protected override void CheckConditionsBeforeSwitch() {

    }

    protected override void MoveForwardsStateUpdate() {
        unitMovement.StopMoving();

        if (unitTargetingSystem.GetMainAttackTarget() != null) {
            //Unit has a valid target for shooting
            ChangeState(State.attacking);
            return;
        }
    }

    public override bool IsWalking() {
        return false;
    }
}
