using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
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

    protected float specialTimer;

    protected bool unitActive;
    protected bool attackStarted;
    protected bool attackEnded;
    protected bool specialActive;

    public enum State {
        idle,
        blockedByBuilding,
        waiting,
        jumping,
        moveForwards,
        moveToMeleeTarget,
        attackingMelee,
        attackingRanged,
        dead,
        fallen,
    }

    protected NetworkVariable<State> state = new NetworkVariable<State>();
    protected State localState;
    public event EventHandler OnStateChanged;

    protected virtual void Awake() {
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
        unitAttack = GetComponent<UnitAttack>();

        mainAttackSO = unit.GetUnitSO().mainAttackSO;
        sideAttackSO = unit.GetUnitSO().sideAttackSO;

        localState = State.idle;
    }

    public override void OnNetworkSpawn() {
        if (unit.GetUnitIsOnlyVisual()) return;

        state.OnValueChanged += State_OnValueChanged;

        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitFell += Unit_OnUnitFell;
        unit.OnUnitDazed += Unit_OnUnitDazed;
        unitAttack.OnUnitAttack += UnitAttack_OnUnitAttack;
        unitAttack.OnUnitAttackEnded += UnitAttack_OnUnitAttackEnded;
        unitAttack.OnUnitAttackStarted += UnitAttack_OnUnitAttackStarted;
    }


    protected virtual void CheckConditionsBeforeSwitch() {
        CheckIfBuildingBlocksMovement();
    }

    protected virtual void CheckIfBuildingBlocksMovement() {
        // Determine nextGridPosition in function of unit belonging
        GridPosition nextGridPosition = new GridPosition(0, 0);

        if (unit.IsOwnedByPlayer()) {
            nextGridPosition = new GridPosition(unit.GetCurrentGridPosition().x + 1, unit.GetCurrentGridPosition().y);
        }
        else {
            nextGridPosition = new GridPosition(unit.GetCurrentGridPosition().x - 1, unit.GetCurrentGridPosition().y);
        }

        if (BattleGrid.Instance.IsValidGridPosition(nextGridPosition)) {
            // This GridPosition is a valid grid position
            Building building = BattleGrid.Instance.GetBuildingAtGridPosition(nextGridPosition);

            if(building != null) {
                if (state.Value != State.blockedByBuilding) {
                    // There is a building that blocks the unit

                    if (building != null) {

                        if (building.GetBuildingSO().buildingBlocksUnitMovement && (building.IsOwnedByPlayer()) == unit.IsOwnedByPlayer()) {
                            // Building blocks unit movement AND is owned by the same player
                            ChangeState(State.blockedByBuilding);
                        }
                    };
                }
            } else {
                if(state.Value == State.blockedByBuilding) {
                    ChangeState(State.moveForwards);
                }
            }
        }
    }

    protected virtual void IdleStateUpdate() {

    }

    protected virtual void BlockedByBuildingStateUpdate() {
    }
    
    protected virtual void MoveForwardsStateUpdate() {

    }

    protected virtual void MoveToMeleeTargetStateUpdate() {

    }

    protected virtual void AttackingStateUpdate() {

    }

    protected virtual void DeadStateUpdate() {

    }

    protected void CheckIfTargetIsInMeleeAttackRange(AttackSO meleeAttackSO, bool meleeAttackSOIsMainAttackSO) {

        if (unitTargetingSystem.GetClosestTargetDistance() < meleeAttackSO.meleeAttackRange/2 ) {
            // There is a unit in melee range (use half of the range to give the unit time to attack)
            unitMovement.StopMoving();
            unitAttack.SetActiveAttackSO(meleeAttackSO);

            if(meleeAttackSOIsMainAttackSO) {
                unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTarget());
            } else {
                unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTarget());
            }
            ChangeState(State.attackingMelee);
        }
    }

    protected void State_OnValueChanged(State previousValue, State newValue) {
        //Debug.Log(unit + " State_OnValueChanged " + newValue);

        if (!unit.GetUnitIsBought()) return;

        state.Value = newValue;
        localState = newValue;

        ChangeStateResponse();
    }

    protected virtual void ChangeStateResponse() {
        if (localState == State.idle) {
            unitAttack.ResetAttackTarget();
            ActivateMainAttack();
            unitMovement.StopMoving();
        }

        if(localState == State.blockedByBuilding) {
            unitMovement.StopMoving();
        }

        if (localState == State.attackingMelee) {
            unitMovement.StopMoving();
        }

        if (localState == State.moveToMeleeTarget) {

        }

        if (localState == State.moveForwards) {
            unitAttack.ResetAttackTarget();
        } 

        if (localState == State.dead) {
            unitMovement.StopMoving();
        }

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    #region EVENT RESPONSES
    protected virtual void BattleManager_OnStateChanged(object sender, System.EventArgs e) {

        if (BattleManager.Instance.IsBattlePhase()) {
            specialTimer = 0f;
            localState = State.moveForwards;
            ChangeStateResponse();
        }
        else {
            localState = State.idle;
            unitMovement.StopMoving();
            ChangeStateResponse();
        }

        if (!IsServer) return;
        unitActive = BattleManager.Instance.IsBattlePhase();
    }

    protected void Unit_OnUnitDazed(object sender, Unit.OnUnitSpecialEventArgs e) {
        if (!IsServer) return;
        StartCoroutine(TakeDazed(e.effectDuration));
    }

    private void Unit_OnUnitPlaced(object sender, EventArgs e) {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Unit_OnUnitActivated(object sender, EventArgs e) {
    }

    protected void Unit_OnUnitDied(object sender, EventArgs e) {
        ChangeState(State.dead);
    }

    protected void Unit_OnUnitFell(object sender, EventArgs e) {
        ChangeState(State.fallen);
    }

    protected virtual void UnitAttack_OnUnitAttack(object sender, EventArgs e) {

    }

    protected virtual void UnitAttack_OnUnitAttackStarted(object sender, EventArgs e) {
        attackStarted = true;
        attackEnded = false;
    }

    protected virtual void UnitAttack_OnUnitAttackEnded(object sender, EventArgs e) {
        attackStarted = false;
        attackEnded = true;
    }

    #endregion

    #region SET PARAMETERS

    public void ChangeState(State newState) {
        // Check if both states are the same : useless call
        if (state.Value == newState && localState == newState) return;

        localState = newState;

        // If only state.Value is the same : call ChangeState
        if (state.Value == newState) {
            ChangeStateResponse();
        } else {
            if (!IsServer) return;
            state.Value = newState;
        }

    }

    protected IEnumerator TakeDazed(float dazedTime) {
        unitActive = false;
        unitMovement.SetDazed(true);
        unitAttack.SetDazed(true);

        yield return new WaitForSeconds(dazedTime);

        if (!unit.GetIsDead() & BattleManager.Instance.IsBattlePhase()) {
            // Unit is still alive and it is still battle phase
            unitActive = true;
        }

        unitMovement.SetDazed(false);
        unitAttack.SetDazed(false);
    }

    protected void InvokeOnStateChanged() {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetAttackStarted(bool attackStarted) {
        this.attackStarted = attackStarted;
    }

    public void SetSpecialTimer(float timer) {
        specialTimer = timer;
    }
    #endregion

    #region GET PARAMETERS

    public State GetState() {
        return localState;
    }
    public bool GetAttackStarted() {
        return attackStarted;
    }
    public bool GetAttackEnded() {
        return attackEnded;
    }

    public bool IsIdle() {
        return localState == State.idle;
    }

    public bool IsBlockedByBuilding() {
        return localState == State.blockedByBuilding;
    }

    public bool IsDead() {
        return localState == State.dead;
    }

    public bool IsAttacking() {
        return localState == State.attackingMelee || localState == State.attackingRanged;
    }
    public virtual bool IsMovingForwards() {
        return localState == State.moveForwards;
    }
    public bool IsMovingToTarget() {
        return localState == State.moveToMeleeTarget;
    }
    public bool IsWaiting() {
        return localState == State.waiting;
    }
    public bool IsFallen() {
        return localState == State.fallen;
    }

    public bool UnitIsActive() {
        return unitActive;
    }

    public bool IsState(State askedState) {
        return localState == askedState;
    }
    public bool IsNotState(State askedState) {
        return localState != askedState;
    }

    public bool GetMainAttackActive() {
        if (unitAttack.GetActiveAttackSO() == mainAttackSO) return true;
        return false;
    }

    public bool GetSideAttackActive() {
        if (unitAttack.GetActiveAttackSO() == sideAttackSO) return true;
        return false;
    }

    public float GetSpecialTimer() {
        return specialTimer;
    }

    public bool GetSpecialActive() {
        return specialActive;
    }

    #endregion

    #region CHANGING ATTACK SO

    public void ActivateMainAttack() {
        if (unitAttack.GetActiveAttackSO() == mainAttackSO) return;
        unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTarget());
        unitAttack.SetActiveAttackSO(mainAttackSO);
    }

    public void ActivateSideAttack() {
        if (unitAttack.GetActiveAttackSO() == sideAttackSO) return;
        unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTarget());
        unitAttack.SetActiveAttackSO(sideAttackSO);
    }

    #endregion

    public override void OnDestroy() {
        base.OnDestroy();

        if (unit.GetUnitIsOnlyVisual()) return;

        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }
}
