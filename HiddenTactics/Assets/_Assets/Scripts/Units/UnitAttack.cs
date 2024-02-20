using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    public event EventHandler OnUnitAttacked;

    private Unit unit;
    private UnitAI unitAI;

    private Unit attackTarget;
    private int attackDamage;
    private float attackTimer;
    private float attackRate;
    private float attackAOE;
    private float attackKnockback;
    private float attackDazedTime;
    private float attackAnimationHitDelay;

    private bool attacking;

    private void Awake() {
        unitAI = GetComponent<UnitAI>();
        unit = GetComponent<Unit>();
    }

    private void Start() {
        unitAI.OnStateChanged += UnitAI_OnStateChanged;
        unit.OnUnitDazed += Unit_OnUnitDazed;

        attackDamage = unit.GetUnitSO().mainAttackDamage;
        attackRate = unit.GetUnitSO().mainAttackRate;
        attackKnockback = unit.GetUnitSO().mainAttackKnockback;
        attackDazedTime = unit.GetUnitSO().mainAttackDazedTime;
        attackAnimationHitDelay = unit.GetUnitSO().mainAttackAnimationHitDelay;
        attackAOE = unit.GetUnitSO().mainAttackAOE;
    }

    private void Update() {
        if (attacking) {
            attackTimer -= Time.deltaTime;
            if (attackTimer < 0) {
                attackTimer = attackRate;
                StartCoroutine(Attack(attackTarget));
            }
        }
    }

    private IEnumerator Attack(Unit targetUnit) {
        OnUnitAttacked?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(attackAnimationHitDelay);

        if(!unit.GetUnitIsDead() && !targetUnit.GetUnitIsDead()) {
            // Unit is still alive on attack animation hit and target unit is still alive

            PerformAllAttackActions(targetUnit);

            if(attackAOE != 0) {
                foreach(Unit unitAOETarget in FindAOEAttackTargets(transform.position)) {
                    PerformAllAttackActions(unitAOETarget);
                }
            }
        }
    }

    private void PerformAllAttackActions(Unit targetUnit) {
        targetUnit.TakeDamage(attackDamage);
        if (attackKnockback != 0) {
            targetUnit.TakeKnockBack(attackKnockback, transform.position);
        }
        if (attackDazedTime != 0) {
            targetUnit.TakeDazed(attackDazedTime);
        }
    }

    private List<Unit> FindAOEAttackTargets(Vector3 targetPosition) {

        List<Unit> AOETargetUnitList = new List<Unit>();
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(targetPosition, attackAOE);

        foreach (Collider2D collider in colliderArray) {
            if (collider.TryGetComponent<Unit>(out Unit unit)) {
                // Collider is a unit

                if (unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.GetUnitIsDead()) {
                    // target unit is not from the same team AND Unit is not dead
                    AOETargetUnitList.Add(unit);
                }
            };
        }

        return AOETargetUnitList;
    }

    private void Unit_OnUnitDazed(object sender, Unit.OnUnitDazedEventArgs e) {
        attacking = false;
    }

    private void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        if(unitAI.IsAttacking()) {
            attacking = true;
        } else {
            attacking = false;
        }
    }

    public void SetAttackTarget(Unit unit) {
        attackTarget = unit;
    }

}
