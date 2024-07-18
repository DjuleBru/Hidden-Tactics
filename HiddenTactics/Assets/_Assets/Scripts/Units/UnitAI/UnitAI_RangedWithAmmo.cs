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

    [ClientRpc]
    protected override void ChangeStateClientRpc() {
        base.ChangeStateClientRpc();
        if (state.Value == State.idle) {
            ammoCount = ammoMax;
        }
        if (state.Value == State.attackingMelee) {
            unitMovement.StopMoving();
        }
        if (state.Value == State.moveToMeleeTarget) {
            ammoCount = ammoMax;
        }
        if (state.Value == State.moveForwards) {
            ActivateMainAttack();
        }
        if (state.Value == State.dead) {
        }
    }
}
