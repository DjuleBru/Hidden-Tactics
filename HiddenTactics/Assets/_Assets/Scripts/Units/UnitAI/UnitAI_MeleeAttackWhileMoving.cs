using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_MeleeAttackWhileMoving : UnitAI
{
    [ClientRpc]
    protected override void ChangeStateClientRpc() {
        if (state.Value == State.idle) {
            unitAttack.ResetAttackTarget();
            ActivateMainAttack();
            unitMovement.StopMoving();
        }

        if (state.Value == State.blockedByBuilding) {
            unitMovement.StopMoving();
        }

        if (state.Value == State.attackingMelee) {
        }

        if (state.Value == State.moveToMeleeTarget) {

        }

        if (state.Value == State.moveForwards) {
            unitAttack.ResetAttackTarget();
        }

        if (state.Value == State.dead) {
            unitMovement.StopMoving();
        }

        InvokeOnStateChanged();
    }
}
