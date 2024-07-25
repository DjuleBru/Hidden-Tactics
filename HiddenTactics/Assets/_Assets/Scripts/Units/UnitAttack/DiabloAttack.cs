using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiabloAttack : UnitAttack
{
    [SerializeField] private float dieAOEDistance;
    [SerializeField] private float dieFlameFXTimer;
    protected override void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        base.UnitAI_OnStateChanged(sender, e);
        if(unitAI.IsDead()) {

            foreach (Unit unitAOETarget in FindAOEAttackTargets(transform.position, dieAOEDistance)) {
                // Die effect
                unitAOETarget.TakeSpecial(AttackSO.UnitAttackSpecial.fire, dieFlameFXTimer);
            }

        }
    }
}
