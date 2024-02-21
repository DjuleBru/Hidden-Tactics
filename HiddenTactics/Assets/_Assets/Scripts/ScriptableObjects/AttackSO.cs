using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AttackSO : ScriptableObject
{
    public enum AttackType {
        melee,
        ranged,
    }

    public enum UnitAttackSpecial {
        none,
        pierce,
        fire,
        ice,
        poison,
        shock,
        bleed,
        fear,
    }

    public AttackType attackType;
    public List<UnitSO.unitType> attackTargets;
    public List<Vector2> attackTargetTiles;
    public float meleeAttackTargetingRange;
    public float meleeAttackRange;
    public int attackDamage;
    public float attackRate;
    public float attackKnockback;
    public float attackDazedTime;
    public float attackAOE;
    public float attackAnimationHitDelay;
    public List<UnitAttackSpecial> attackSpecialList;
}
