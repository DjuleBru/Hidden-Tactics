using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI : MonoBehaviour
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

    private State state;
    public event EventHandler OnStateChanged;

    private void Awake() {
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
        unitAttack = GetComponent<UnitAttack>();

        state = State.idle;
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitDazed += Unit_OnUnitDazed;
    }

    private void Update() {
        if (!unitActive) return;

        switch(state) {
            case State.idle:
                break; 
            case State.moveForwards:

                if(unitTargetingSystem.GetMeleeTargetUnit() != null) {
                    //Unit has a valid melee target
                    ChangeState(State.moveToTarget);
                }

            break;
            case State.moveToTarget:

                if (unitTargetingSystem.GetMeleeTargetUnit() == null) {
                    ChangeState(State.moveForwards);
                }

                break;
            case State.attacking:

                if(unitTargetingSystem.GetMeleeTargetUnit() == null | !unitTargetingSystem.GetMeleeTargetUnitIsInRange()) {
                    // Unit has no attack targets or target attack unit is out of range
                    ChangeState(State.moveForwards);
                }

                break;
            case State.dead:

            break;

        }

    }

    private void FixedUpdate() {
        if (!unitActive) return;

        if (state == State.moveForwards) {
            unitMovement.MoveForwards();
        }
        if(state == State.moveToTarget) {
            unitMovement.MoveToTarget(unitTargetingSystem.GetMeleeTargetUnit().transform.position);

            if(unitTargetingSystem.GetClosestTargetDistance() < unit.GetUnitSO().mainAttackRange) {
                unitMovement.StopMoving();
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMeleeTargetUnit());
                ChangeState(State.attacking);
            }
        }
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        unitActive = BattleManager.Instance.IsBattlePhase();
        if(unitActive) {
            ChangeState(State.moveForwards);
        } else {
            ChangeState(State.idle);
            unitMovement.StopMoving();
        }
    }


    private void Unit_OnUnitDazed(object sender, Unit.OnUnitDazedEventArgs e) {
        StartCoroutine(TakeDazed(e.dazedTime));
    }

    private void Unit_OnUnitDied(object sender, EventArgs e) {
        ChangeState(State.dead);
    }

    private IEnumerator TakeDazed(float dazedTime) {
        unitActive = false;

        yield return new WaitForSeconds(dazedTime);
        if(!unit.GetUnitIsDead()) {
            // Unit is still alive
            unitActive = true;
            ChangeState(State.moveForwards);
        }
    }

    private void ChangeState(State newState) {
        state = newState;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetUnitActive(bool active) {
        unitActive = active;
    }

    public bool IsWalking() {
        return state == State.moveForwards;
    }

    public bool IsIdle() {
        return state == State.idle;
    }

    public bool IsDead() {
        return state == State.dead;
    }

    public bool IsAttacking() {
        return state == State.attacking;
    }

    public bool IsMovingToTarget() {
        return state == State.moveToTarget;
    }

}
