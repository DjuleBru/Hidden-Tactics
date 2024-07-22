using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiabloAttack : UnitAttack
{
    protected override void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        base.UnitAI_OnStateChanged(sender, e);
        if(unitAI.IsDead()) {

            foreach (Unit unitAOETarget in FindAOEAttackTargets(transform.position, 1.5f)) {
                // Die effect
                unitAOETarget.TakeSpecial(AttackSO.UnitAttackSpecial.fire, activeAttackSO.specialEffectDuration *2);
            }

        }
    }
}
