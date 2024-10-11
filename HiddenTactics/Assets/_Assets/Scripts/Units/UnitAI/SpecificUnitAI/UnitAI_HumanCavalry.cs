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
    private UnitBuffManager unitBuffManager;

    protected void Start() {
        gallopTimer = gallopTriggerTime;
        unitInitialDamage = unitAttack.GetAttackDamage();
        unitInitialMoveSpeed = unitMovement.GetMoveSpeed();
        unitBuffManager = GetComponent<UnitBuffManager>();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        galloping.OnValueChanged += Galloping_OnValueChanged;
    }

    private void Update() {
        if (!IsServer) return;
        CheckConditionsBeforeSwitch();
    }

    protected override void CheckConditionsBeforeSwitch() {
        if (galloping.Value) return;

        if(localState == State.moveToMeleeTarget || localState == State.moveForwards) {

            gallopTimer -= Time.deltaTime;

            if (gallopTimer < 0 && !galloping.Value) {
                gallopTimer = gallopTriggerTime;
                galloping.Value = true;
                unitBuffManager.BuffMoveSpeed(gallopMoveSpeedMultiplier);
                unitBuffManager.BuffAttackDamage(unitInitialDamage * gallopDamageBuffMultiplier);
                unitBuffManager.BuffAttackKnockbackAbsolute(gallopKnockbackBuffAbsolute);
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
        unitBuffManager.ResetAttackDamage();
        unitBuffManager.DebuffAttackKnockbackAbsolute(gallopKnockbackBuffAbsolute);
        unitBuffManager.ResetMoveSpeed();
        unitAttack.InvokeOnUnitAttackEnded();
    }

    private void Galloping_OnValueChanged(bool previousValue, bool newValue) {
        if(newValue == true) {
            unitAttack.InvokeOnUnitAttackStarted();
        } else {
            unitAttack.InvokeOnUnitAttackEnded();
        }
    }

    //[ClientRpc]
    protected override void ChangeStateResponse() {
        base.ChangeStateResponse();
        if (localState == State.idle) {
            if(galloping.Value == true) {
                galloping.Value = false;
                RemoveGallopBuffs();
            }
        }
    }

}
