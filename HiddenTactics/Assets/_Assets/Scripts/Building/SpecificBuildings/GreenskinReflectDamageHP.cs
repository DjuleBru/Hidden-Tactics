using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenskinReflectDamageHP : BuildingHP, IDamageSource
{
    [SerializeField] private float damageReflection;

    public override void TakeDamage(float damage, IDamageSource damageSource) {
        base.TakeDamage(damage, damageSource);

        if((damageSource as MonoBehaviour).GetComponent<UnitAttack>().GetActiveAttackSO().attackType == AttackSO.AttackType.melee) {
            (damageSource as MonoBehaviour).GetComponent<IDamageable>().TakeDamage(damageReflection, this);
        }

    }
}
