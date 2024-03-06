using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttack_Spearman : UnitAttack
{
    protected float largeUnitDamageMultiplier = 2f;
    protected override void PerformAllDamageActions(ITargetable target, Vector3 damageHitPosition) {
        float attackDamageModified = attackDamage;

        if (target is Unit) {
            Unit targetUnit = (Unit)target;
            if (targetUnit.GetUnitSO().unitTagList.Contains(UnitSO.UnitTag.large)) {
                attackDamageModified = attackDamage * largeUnitDamageMultiplier;
            }

            targetUnit.GetComponent<UnitHP>().TakeDamage(attackDamageModified);

            if (attackKnockback != 0) {
                Vector2 incomingDamageDirection = new Vector2((target as Unit).transform.position.x - damageHitPosition.x, (target as Unit).transform.position.y - damageHitPosition.y);
                Vector2 force = incomingDamageDirection * attackKnockback;

                (target as Unit).TakeKnockBack(force);
            }

            if (attackDazedTime != 0) {
                (target as Unit).TakeDazed(attackDazedTime);
            }
        }

    }
}
