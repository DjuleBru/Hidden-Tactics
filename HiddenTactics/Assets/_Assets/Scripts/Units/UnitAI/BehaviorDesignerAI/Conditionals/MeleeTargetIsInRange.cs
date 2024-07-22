using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeTargetIsInRange : Conditional 
{
    private Unit unit;
    private UnitTargetingSystem unitTargetingSystem;

    private AttackSO meleeAttackSO;
    public UnitTargetingSystem.AttackMode attackMode;

    public float rangeDivider;

    public override void OnAwake() {
        unitTargetingSystem = GetComponent<UnitTargetingSystem>();
        unit = GetComponent<Unit>();

        if (attackMode == UnitTargetingSystem.AttackMode.mainAttack) {
            meleeAttackSO = unit.GetUnitSO().mainAttackSO;
        }

        if (attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
            meleeAttackSO = unit.GetUnitSO().sideAttackSO;
        }
    }
    

    public override TaskStatus OnUpdate() {

        if (attackMode == UnitTargetingSystem.AttackMode.mainAttack) {

            ColliderDistance2D distanceBetweenUnitColliders = Physics2D.Distance(gameObject.GetComponent<Collider2D>(), (unitTargetingSystem.GetMainAttackTarget() as MonoBehaviour).gameObject.GetComponent<Collider2D>());
            float distanceToTarget = distanceBetweenUnitColliders.distance;

            if (distanceToTarget < meleeAttackSO.meleeAttackRange / rangeDivider) {
                // There is a unit in melee range (use half of the range to give the unit time to attack)
                return TaskStatus.Success;
            }
            else {
                return TaskStatus.Failure;
            }
        }

        if (attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
            ColliderDistance2D distanceBetweenUnitColliders = Physics2D.Distance(gameObject.GetComponent<Collider2D>(), (unitTargetingSystem.GetSideAttackTarget() as MonoBehaviour).gameObject.GetComponent<Collider2D>());
            float distanceToTarget = distanceBetweenUnitColliders.distance;
            if (distanceToTarget < meleeAttackSO.meleeAttackRange / rangeDivider) {
                // There is a unit in melee range (use half of the range to give the unit time to attack)
                return TaskStatus.Success;
            }
            else {
                return TaskStatus.Failure;
            }
        }

        return TaskStatus.Failure;
    }
}
