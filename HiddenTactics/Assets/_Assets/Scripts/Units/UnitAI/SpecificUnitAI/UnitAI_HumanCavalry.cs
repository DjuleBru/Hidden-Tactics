using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_HumanCavalry : UnitAI
{
    private float gallopTimer;

    private NetworkVariable<bool> galloping = new NetworkVariable<bool>(false);

    [SerializeField] private float gallopMoveSpeedMultiplier;
    [SerializeField] private float gallopDamageBuffMultiplier;
    [SerializeField] private float gallopKnockbackBuffAbsolute;
    [SerializeField] private float gallopTriggerTime;

    private float unitInitialDamage;
    private float unitInitialMoveSpeed;

    protected void Start() {
        gallopTimer = gallopTriggerTime;
        unitInitialDamage = unitAttack.GetAttackDamage();
        unitInitialMoveSpeed = unitMovement.GetMoveSpeed();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        galloping.OnValueChanged += Galloping_OnValueChanged;
    }

    private void Update() {
        CheckConditionsBeforeSwitch();
    }

    protected override void CheckConditionsBeforeSwitch() {
        if (galloping.Value) return;

        if(state.Value == State.moveToMeleeTarget || state.Value == State.moveForwards) {

            gallopTimer -= Time.deltaTime;

            if (gallopTimer < 0 && !galloping.Value) {
                gallopTimer = gallopTriggerTime;
                galloping.Value = true;
                unitMovement.BuffMoveSpeed(unitInitialMoveSpeed * gallopMoveSpeedMultiplier);
                unitAttack.BuffAttackDamage(unitInitialDamage * gallopDamageBuffMultiplier);
                unitAttack.BuffAttackKnockbackAbsolute(gallopKnockbackBuffAbsolute);
                unitAttack.SetAttackTimer(0f);
            }
        }
    }

    protected override void UnitAttack_OnUnitAttack(object sender, System.EventArgs e) {
        if (galloping.Value == true) {
            StartCoroutine(RemoveGallopBuffsCoroutine());
            galloping.Value = false;
        }
    }

    private IEnumerator RemoveGallopBuffsCoroutine() {
        yield return new WaitForSeconds(.2f);
        RemoveGallopBuffs();
    }

    private void RemoveGallopBuffs() {
        gallopTimer = gallopTriggerTime;
        unitAttack.DebuffAttackDamage(unitInitialDamage * gallopDamageBuffMultiplier);
        unitAttack.DebuffAttackKnockbackAbsolute(gallopKnockbackBuffAbsolute);
        unitMovement.DebuffMoveSpeed(unitInitialMoveSpeed * gallopMoveSpeedMultiplier);
        unitAttack.InvokeOnUnitAttackEnded();
    }

    private void Galloping_OnValueChanged(bool previousValue, bool newValue) {
        if(newValue == true) {
            unitAttack.InvokeOnUnitAttackStarted();
        } else {
            unitAttack.InvokeOnUnitAttackEnded();
        }
    }

    [ClientRpc]
    protected override void ChangeStateClientRpc() {
        base.ChangeStateClientRpc();
        if (state.Value == State.idle) {
            if(galloping.Value == true) {
                galloping.Value = false;
                RemoveGallopBuffs();
            }
        }
    }

}
