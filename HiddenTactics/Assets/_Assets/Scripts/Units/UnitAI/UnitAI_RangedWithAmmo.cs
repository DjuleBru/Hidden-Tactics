using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_RangedWithAmmo : UnitAI
{

    [SerializeField] private int ammoMax;

    private int ammoCount;

    protected override void Awake() {
        base.Awake();
        ammoCount = ammoMax;
    }

    protected override void UnitAttack_OnUnitAttackEnded(object sender, EventArgs e) {
        ammoCount--;
    }

    public int GetAmmoCount() {
        return ammoCount;
    }

    public int GetAmmoMax() {
        return ammoMax;
    }

    protected override void ChangeStateResponse() {
        base.ChangeStateResponse();
        if (localState == State.idle) {
            ammoCount = ammoMax;
        }
        if (localState == State.attackingMelee) {
            unitMovement.StopMoving();
        }
        if (localState == State.moveToMeleeTarget) {
            ammoCount = ammoMax;
        }
        if (localState == State.moveForwards) {
            ActivateMainAttack();
        }
        if (localState == State.dead) {
        }
    }
}
