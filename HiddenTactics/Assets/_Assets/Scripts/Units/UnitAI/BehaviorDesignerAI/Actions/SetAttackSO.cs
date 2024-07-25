using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAttackSO : Action {
    private Unit unit;
    private AttackSO attackSO;
    private UnitAttack unitAttack;
    public UnitTargetingSystem.AttackMode attackMode;

    public override void OnAwake() {
        unitAttack = GetComponent<UnitAttack>();
        unit = GetComponent<Unit>();


        if (attackMode == UnitTargetingSystem.AttackMode.mainAttack) {
            attackSO = unit.GetUnitSO().mainAttackSO;
        }

        if (attackMode == UnitTargetingSystem.AttackMode.sideAttack) {
            attackSO = unit.GetUnitSO().sideAttackSO;
        }
    }

    public override TaskStatus OnUpdate() {

        unitAttack.SetActiveAttackSO(attackSO);

        return TaskStatus.Success;
    }
}
