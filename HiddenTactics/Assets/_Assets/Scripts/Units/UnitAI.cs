using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI : NetworkBehaviour
{
    private Unit unit;
    private UnitMovement unitMovement;
    private UnitTargetingSystem unitTargetingSystem;
    private UnitAttack unitAttack;

    private bool unitActive;

    public enum State {
        idle,
        moveForwards,
        moveToTarget,
        attacking,
        dead,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.idle);
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

    private void Update() {
        if (!IsServer) return;
        // AI runs only on server

        if (!unitActive | !unit.UnitIsBought()) return;

        switch(state.Value) {
            case State.idle:
                break; 
            case State.moveForwards:

                if (unitTargetingSystem.GetTargetUnit() != null) {
                    //Unit has a valid target
                    if(unitTargetingSystem.GetMainAttackType() == AttackSO.AttackType.melee) {
                        ChangeState(State.moveToTarget);
                    } else {
                        unitAttack.SetAttackTarget(unitTargetingSystem.GetTargetUnit());
                        ChangeState(State.attacking);
                    }

                }

            break;
            case State.moveToTarget:

                if (unitTargetingSystem.GetTargetUnit() == null) {
                    ChangeState(State.moveForwards);
                }

                break;
            case State.attacking:

                if(unitTargetingSystem.GetTargetUnit() == null | !unitTargetingSystem.GetTargetUnitIsInRange()) {
                    // Unit has no attack targets or target attack unit is out of range
                    ChangeState(State.moveForwards);
                }

                break;
            case State.dead:

            break;

        }

    }

    private void FixedUpdate() {
        if (!IsServer) return;
        // AI runs only on server

        if (!unitActive | !unit.UnitIsBought()) return;

        if (state.Value == State.moveForwards) {
            unitMovement.MoveForwards();
        }
        if(state.Value == State.moveToTarget) {

            unitMovement.MoveToTarget(unitTargetingSystem.GetTargetUnit().transform.position);

            if(unitTargetingSystem.GetClosestTargetDistance() < unit.GetUnitSO().mainAttackSO.meleeAttackRange) {
                unitMovement.StopMoving();
                unitAttack.SetAttackTarget(unitTargetingSystem.GetTargetUnit());
                ChangeState(State.attacking);
            }

        }
    }
    private void State_OnValueChanged(State previousValue, State newValue) {
        ChangeStateServerRpc();
    }

    [ServerRpc(RequireOwnership =false)]
    private void ChangeStateServerRpc() {
        ChangeStateClientRpc();
    }

    [ClientRpc] 
    private void ChangeStateClientRpc() {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (!IsServer) return; 
        unitActive = BattleManager.Instance.IsBattlePhase();
        if(unitActive) {
            ChangeState(State.moveForwards);
        } else {
            ChangeState(State.idle);
            unitMovement.StopMoving();
        }
    }


    private void Unit_OnUnitDazed(object sender, Unit.OnUnitDazedEventArgs e) {
        if (!IsServer) return;
        StartCoroutine(TakeDazed(e.dazedTime));
    }

    private void Unit_OnUnitDied(object sender, EventArgs e) {
        if (!IsServer) return;
        ChangeState(State.dead);
    }

    private IEnumerator TakeDazed(float dazedTime) {
        unitActive = false;

        yield return new WaitForSeconds(dazedTime);

        if(!unit.UnitIsDead() & BattleManager.Instance.IsBattlePhase()) {
            // Unit is still alive and it is still battle phase
            unitActive = true;
            ChangeState(State.moveForwards);
        }
    }

    private void ChangeState(State newState) {
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
