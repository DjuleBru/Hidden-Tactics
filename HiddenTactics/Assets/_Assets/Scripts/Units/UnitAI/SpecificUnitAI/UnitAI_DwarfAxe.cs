using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_DwarfAxe : UnitAI_Melee
{
    private bool wearingShield;
    private bool shieldingDamage;
    private int projectilesShielded;
    private int maxProjectilesShielded = 2;

    protected override void Update() {
        base.Update();
        if(shieldingDamage) {
            unitMovement.StopMoving();
        }

    }

    [ClientRpc]
    protected override void ChangeStateClientRpc() {

        base.ChangeStateClientRpc();

        if (state.Value == State.idle) {

        }
        if (state.Value == State.attacking) {
            wearingShield = false;
        }

        if (state.Value == State.moveToMeleeTarget) {
            wearingShield = false;
            ActivateMainAttack();
        }

        if (state.Value == State.moveForwards) {
            // Move Forwards is called when unit is dazed to reset it. So we need to check if it is dazed
            ActivateShield();
        }

        if (state.Value == State.dead) {

        }

    }

    private void ActivateShield() {
        wearingShield = true;
        projectilesShielded = 0;
        ActivateSideAttack();
    }

    public void TakeShieldDamage() {
        if(!shieldingDamage) {
            StartCoroutine(TakeShieldDamageCoroutine());
        }

    }

    public IEnumerator TakeShieldDamageCoroutine() {
        shieldingDamage = true;

        unitAttack.InvokeOnUnitAttack();
        yield return new WaitForSeconds(1f);

        projectilesShielded++;

        if (projectilesShielded == maxProjectilesShielded) {
            wearingShield = false;
            InvokeOnStateChanged();
            ActivateMainAttack();
        }

        shieldingDamage = false;
    }

    public bool GetWearingShield() {
        return wearingShield;
    }

}
