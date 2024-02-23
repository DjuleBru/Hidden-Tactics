using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI : NetworkBehaviour
{
    protected Unit unit;
    protected UnitMovement unitMovement;
    protected UnitTargetingSystem unitTargetingSystem;
    protected UnitAttack unitAttack;

    protected bool unitActive;

    public enum State {
        idle,
        moveForwards,
        moveToTarget,
        attacking,
        dead,
    }

    protected NetworkVariable<State> state = new NetworkVariable<State>(State.idle);
    public event EventHandler OnStateChanged;

    private void Awake() {
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
        unitAttack = GetComponent<UnitAttack>();
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;

        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitDazed += Unit_OnUnitDazed;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    protected void State_OnValueChanged(State previousValue, State newValue) {
        ChangeStateServerRpc();
    }

    [ServerRpc(RequireOwnership =false)]
    protected void ChangeStateServerRpc() {
        ChangeStateClientRpc();
    }

    [ClientRpc]
    protected void ChangeStateClientRpc() {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    protected void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (!IsServer) return; 
        unitActive = BattleManager.Instance.IsBattlePhase();
        if(unitActive) {
            ChangeState(State.moveForwards);
        } else {
            ChangeState(State.idle);
            unitMovement.StopMoving();
        }
    }


    protected void Unit_OnUnitDazed(object sender, Unit.OnUnitDazedEventArgs e) {
        if (!IsServer) return;
        StartCoroutine(TakeDazed(e.dazedTime));
    }

    protected void Unit_OnUnitDied(object sender, EventArgs e) {
        if (!IsServer) return;
        ChangeState(State.dead);
    }

    protected IEnumerator TakeDazed(float dazedTime) {
        unitActive = false;

        yield return new WaitForSeconds(dazedTime);

        if(!unit.GetUnitIsDead() & BattleManager.Instance.IsBattlePhase()) {
            // Unit is still alive and it is still battle phase
            unitActive = true;
            ChangeState(State.moveForwards);
        }
    }

    protected void ChangeState(State newState) {
        state.Value = newState;
    }

    public void SetUnitActive(bool active) {
        unitActive = active;
    }

    public bool IsWalking() {
        return state.Value == State.moveForwards;
    }

    public bool IsIdle() {
        return state.Value == State.idle;
    }

    public bool IsDead() {
        return state.Value == State.dead;
    }

    public bool IsAttacking() {
        return state.Value == State.attacking;
    }

    public bool IsMovingToTarget() {
        return state.Value == State.moveToTarget;
    }
}
