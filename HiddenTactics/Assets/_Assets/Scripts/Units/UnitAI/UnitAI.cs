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

    protected AttackSO mainAttackSO;
    protected AttackSO sideAttackSO;

    public event EventHandler OnMainAttackActivated;
    public event EventHandler OnSideAttackActivated;

    protected bool unitActive;
    protected bool attackStarted;

    public enum State {
        idle,
        moveForwards,
        moveToMeleeTarget,
        attacking,
        dead,
    }

    protected NetworkVariable<State> state = new NetworkVariable<State>(State.idle);
    public event EventHandler OnStateChanged;

    protected void Awake() {
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
        unitAttack = GetComponent<UnitAttack>();

        mainAttackSO = unit.GetUnitSO().mainAttackSO;
        sideAttackSO = unit.GetUnitSO().sideAttackSO;
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;

        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitDazed += Unit_OnUnitDazed;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    protected void CheckIfTargetUnitIsInMeleeAttackRange(AttackSO meleeAttackSO, bool meleeAttackSOIsMainAttackSO) {
        if (unitTargetingSystem.GetClosestTargetDistance() < meleeAttackSO.meleeAttackRange) {
            unitMovement.StopMoving();
            unitAttack.SetActiveAttackSO(meleeAttackSO);

            if(meleeAttackSOIsMainAttackSO) {
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
            } else {
                unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTargetUnit());
            }
            ChangeState(State.attacking);
        }
    }

    protected void State_OnValueChanged(State previousValue, State newValue) {
        ChangeStateServerRpc();
    }

    [ServerRpc(RequireOwnership =false)]
    protected void ChangeStateServerRpc() {
        ChangeStateClientRpc();
    }

    [ClientRpc]
    protected virtual void ChangeStateClientRpc() {
        if (state.Value == State.idle) {
            unitAttack.ResetAttackTarget();
            ActivateMainAttack();
            unitMovement.StopMoving();
        }
        if (state.Value == State.attacking) {
            unitMovement.StopMoving();
        }
        if (state.Value == State.moveToMeleeTarget) {

        }
        if (state.Value == State.moveForwards) {
        }
        if (state.Value == State.dead) {
            unitMovement.StopMoving();
        }

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    protected void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (!IsServer) return; 
        unitActive = BattleManager.Instance.IsBattlePhase();
        if(BattleManager.Instance.IsBattlePhase()) {
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
        unitMovement.SetDazed(true);
        yield return new WaitForSeconds(dazedTime);

        unitMovement.SetDazed(false);

        if (!unit.GetUnitIsDead() & BattleManager.Instance.IsBattlePhase()) {
            // Unit is still alive and it is still battle phase
            unitActive = true;
            ChangeState(State.moveForwards);
        }
    }

    protected void ChangeState(State newState) {
        state.Value = newState;
    }

    public void SetAttackStarted(bool attackStarted) {
        this.attackStarted = attackStarted;
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
        return state.Value == State.moveToMeleeTarget;
    }

    #region CHANGING ATTACK SO

    protected void ActivateMainAttack() {
        InvokeOnMainAttackActivatedServerRpc();
        unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTargetUnit());
        unitAttack.SetActiveAttackSO(mainAttackSO);
    }

    protected void ActivateSideAttack() {
        InvokeOnSideAttackActivatedServerRpc();
        unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTargetUnit());
        unitAttack.SetActiveAttackSO(sideAttackSO);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InvokeOnMainAttackActivatedServerRpc() {
        InvokeOnMainAttackActivatedClientRpc();
    }

    [ClientRpc]
    private void InvokeOnMainAttackActivatedClientRpc() {
        OnMainAttackActivated?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InvokeOnSideAttackActivatedServerRpc() {
        InvokeOnSideAttackActivatedClientRpc();
    }
    [ClientRpc]
    private void InvokeOnSideAttackActivatedClientRpc() {
        OnSideAttackActivated?.Invoke(this, EventArgs.Empty);
    }
    #endregion
}
