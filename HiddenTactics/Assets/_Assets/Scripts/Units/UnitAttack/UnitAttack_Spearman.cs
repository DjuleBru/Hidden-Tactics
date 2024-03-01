using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttack_Spearman : UnitAttack
{
    protected float largeUnitDamageMultiplier = 2f;
    protected override void PerformAllDamageActions(Unit targetUnit, Vector3 damageHitPosition) {
        float attackDamageModified = attackDamage;

        if(targetUnit.GetUnitSO().unitTagList.Contains(UnitSO.UnitTag.large)) {
            attackDamageModified = attackDamage * largeUnitDamageMultiplier;
        }

        targetUnit.GetComponent<UnitHP>().TakeDamage(attackDamageModified);
        
        if (attackKnockback != 0) {

            Vector2 incomingDamageDirection = new Vector2(targetUnit.transform.position.x - damageHitPosition.x, targetUnit.transform.position.y - damageHitPosition.y);
            Vector2 force = incomingDamageDirection * attackKnockback;

            targetUnit.TakeKnockBack(force);
        }
        if (attackDazedTime != 0) {
            targetUnit.TakeDazed(attackDazedTime);
        }
    }
}
