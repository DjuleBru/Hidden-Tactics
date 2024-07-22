using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsStatusEffect : Conditional
{
    public AttackSO.UnitAttackSpecial statusEffect;
    private Unit unit;

    public override void OnAwake() {
        unit = GetComponent<Unit>();
    }


    public override TaskStatus OnUpdate() {
        if(statusEffect == AttackSO.UnitAttackSpecial.fear) {
            bool scared = unit.GetScared();
            if(scared ) {
                return TaskStatus.Success;
            } else {
                return TaskStatus.Failure;
            }
        }
        return TaskStatus.Failure;
    }
}
