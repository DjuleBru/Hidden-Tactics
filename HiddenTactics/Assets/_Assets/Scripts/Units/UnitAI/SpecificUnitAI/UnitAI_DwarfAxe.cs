using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_DwarfAxe : UnitAI
{
    private bool wearingShield;
    private int projectilesShielded;
    private int maxProjectilesShielded = 2;

    protected void Update() {
        if(specialActive) {
            unitMovement.StopMoving();
        }
    }

    //[ClientRpc]
    protected override void ChangeStateResponse() {

        base.ChangeStateResponse();

        if (localState == State.idle) {

        }
        if (localState == State.attackingMelee) {
            wearingShield = false;
        }

        if (localState == State.moveToMeleeTarget) {
            wearingShield = false;
            ActivateMainAttack();
        }

        if (localState == State.moveForwards) {
            // Move Forwards is called when unit is dazed to reset it. So we need to check if it is dazed
            ActivateShield();
        }

        if (localState == State.dead) {

        }

    }

    private void ActivateShield() {
        wearingShield = true;
        projectilesShielded = 0;
        ActivateSideAttack();
    }

    public void TakeShieldDamage() {
        if(!specialActive) {
            StartCoroutine(TakeShieldDamageCoroutine());
        }
    }

    public IEnumerator TakeShieldDamageCoroutine() {
        specialActive = true;

        unitAttack.InvokeOnUnitAttack();
        yield return new WaitForSeconds(1f);

        projectilesShielded++;

        if (projectilesShielded == maxProjectilesShielded) {
            wearingShield = false;
            InvokeOnStateChanged();
            ActivateMainAttack();
        }

        specialActive = false;
    }

    public bool GetWearingShield() {
        return wearingShield;
    }

}
