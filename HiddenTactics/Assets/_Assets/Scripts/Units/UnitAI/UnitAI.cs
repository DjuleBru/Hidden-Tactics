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
        blockedByBuilding,
        moveForwards,
        moveToMeleeTarget,
        attacking,
        dead,
        fallen,
    }

    protected NetworkVariable<State> state = new NetworkVariable<State>();
    public event EventHandler OnStateChanged;

    protected virtual void Awake() {
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
        unitAttack = GetComponent<UnitAttack>();

        mainAttackSO = unit.GetUnitSO().mainAttackSO;
        sideAttackSO = unit.GetUnitSO().sideAttackSO;
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;
        state.Value = State.idle;

        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitFell += Unit_OnUnitFell;
        unit.OnUnitDazed += Unit_OnUnitDazed;
        unitAttack.OnUnitAttack += UnitAttack_OnUnitAttack;
        unitAttack.OnUnitAttackEnded += UnitAttack_OnUnitAttackEnded;
        unitAttack.OnUnitAttackStarted += UnitAttack_OnUnitAttackStarted;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }


    protected virtual void Update() {
        if (!IsServer) return;
        // AI runs only on server

        if (!unitActive | state.Value == State.dead) return;

        if(state.Value != State.idle) {
            CheckConditionsBeforeSwitch();
        }

        switch (state.Value) {
            case State.idle:
                IdleStateUpdate();
                break;
            case State.blockedByBuilding:
                BlockedByBuildingStateUpdate();
                break;
            case State.moveForwards:
                MoveForwardsStateUpdate();
                break;
            case State.moveToMeleeTarget:
                MoveToMeleeTargetStateUpdate();
                break;
            case State.attacking:
                AttackingStateUpdate();
                break;
            case State.dead:
                DeadStateUpdate();
                break;
        }
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
            List<Building> buildingList = BattleGrid.Instance.GetBuildingListAtGridPosition(nextGridPosition);

            if(buildingList.Count > 0) {
                Building buildingOnBattlefield = buildingList[0];

                if (state.Value != State.blockedByBuilding) {
                    // There is a building that blocks the unit

                    if (buildingOnBattlefield != null) {

                        if (buildingOnBattlefield.GetBuildingSO().buildingBlocksUnitMovement && (buildingOnBattlefield.IsOwnedByPlayer()) == unit.IsOwnedByPlayer()) {
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
            ChangeState(State.attacking);
        }
    }

    protected void State_OnValueChanged(State previousValue, State newValue) {
        if (!IsSpawned) return;
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

        if(state.Value == State.blockedByBuilding) {
            unitMovement.StopMoving();
        }

        if (state.Value == State.attacking) {
            unitMovement.StopMoving();
        }

        if (state.Value == State.moveToMeleeTarget) {

        }

        if (state.Value == State.moveForwards) {
            unitAttack.ResetAttackTarget();
        }

        if (state.Value == State.dead) {
            unitMovement.StopMoving();
        }

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    #region EVENT RESPONSES
    protected virtual void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
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

    protected void Unit_OnUnitFell(object sender, EventArgs e) {
        if (!IsServer) return;
        ChangeState(State.fallen);
    }

    protected virtual void UnitAttack_OnUnitAttack(object sender, EventArgs e) {

    }

    protected virtual void UnitAttack_OnUnitAttackStarted(object sender, EventArgs e) {
    }

    protected virtual void UnitAttack_OnUnitAttackEnded(object sender, EventArgs e) {
    }
    #endregion
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

    protected void ChangeState(State newState) {
        state.Value = newState;
    }

    public void SetIdleState() {
        state.Value = State.idle;
    }

    protected void InvokeOnStateChanged() {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetAttackStarted(bool attackStarted) {
        this.attackStarted = attackStarted;
    }

    #region GET PARAMETERS

    public State GetState() {
        return state.Value;
    }
    public virtual bool IsWalking() {
        return state.Value == State.moveForwards;
    }

    public bool IsIdle() {
        return state.Value == State.idle;
    }

    public bool IsBlockedByBuilding() {
        return state.Value == State.blockedByBuilding;
    }

    public bool IsDead() {
        return state.Value == State.dead;
    }

    public bool IsAttacking() {
        return state.Value == State.attacking;
    }
    public bool IsMovingForwards() {
        return state.Value == State.moveForwards;
    }
    public bool IsMovingToTarget() {
        return state.Value == State.moveToMeleeTarget;
    }

    public bool IsFallen() {
        return state.Value == State.fallen;
    }
    #endregion

    #region CHANGING ATTACK SO

    protected void ActivateMainAttack() {
        InvokeOnMainAttackActivatedServerRpc();
        unitAttack.SetAttackTarget(unitTargetingSystem.GetMainAttackTarget());
        unitAttack.SetActiveAttackSO(mainAttackSO);
    }

    protected void ActivateSideAttack() {
        InvokeOnSideAttackActivatedServerRpc();
        unitAttack.SetAttackTarget(unitTargetingSystem.GetSideAttackTarget());
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

    public override void OnDestroy() {
        base.OnDestroy();

        if (unit.GetUnitIsOnlyVisual()) return;

        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }
}
