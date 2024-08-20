using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenskinReflectDamageHP : BuildingHP, IDamageSource
{
    public override void TakeDamage(float damage, IDamageSource damageSource) {
        base.TakeDamage(damage, damageSource);

        if((damageSource as MonoBehaviour).GetComponent<UnitAttack>().GetActiveAttackSO().attackType == AttackSO.AttackType.melee) {
            (damageSource as MonoBehaviour).GetComponent<IDamageable>().TakeDamage(GetComponent<Building>().GetBuildingSO().reflectMeleeDamageAmount, this);
        }

    }
}
