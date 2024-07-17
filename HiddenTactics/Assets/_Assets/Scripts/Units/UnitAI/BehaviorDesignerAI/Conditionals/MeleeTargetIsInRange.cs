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
        if (unitTargetingSystem.GetClosestTargetDistance() < meleeAttackSO.meleeAttackRange / 2) {
            // There is a unit in melee range (use half of the range to give the unit time to attack)
            return TaskStatus.Success;
        } else {
            return TaskStatus.Failure;
        }

    }
}
