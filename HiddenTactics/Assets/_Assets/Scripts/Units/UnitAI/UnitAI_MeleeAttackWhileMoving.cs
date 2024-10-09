using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_MeleeAttackWhileMoving : UnitAI
{
    //[ClientRpc]
    protected override void ChangeStateResponse() {
        if (localState == State.idle) {
            unitAttack.ResetAttackTarget();
            ActivateMainAttack();
            unitMovement.StopMoving();
        }

        if (localState == State.blockedByBuilding) {
            unitMovement.StopMoving();
        }

        if (localState == State.attackingMelee) {
        }

        if (localState == State.moveToMeleeTarget) {

        }

        if (localState == State.moveForwards) {
            unitAttack.ResetAttackTarget();
        }

        if (localState == State.dead) {
            unitMovement.StopMoving();
        }

        InvokeOnStateChanged();
    }
}
