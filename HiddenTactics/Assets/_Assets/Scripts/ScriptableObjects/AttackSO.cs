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
    public bool attackDecomposition;
    public bool attackHasAOE;
    public bool attackHasAOECollider;
    public GameObject projectilePrefab;
    public List<UnitSO.MoveType> moveTypeAttackTargets;
    public List<Vector2> attackTargetTiles;
    public float meleeAttackTargetingRange;
    public float meleeAttackRange;
    public int attackDamage;
    public float attackRate;
    public float attackKnockback;
    public float attackDazedTime;
    public float attackAOE;
    public float meleeAttackAnimationHitDelay;
    public float meleeAttackAnimationDuration;
    public List<UnitAttackSpecial> attackSpecialList;
}
