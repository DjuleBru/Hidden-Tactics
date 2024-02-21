using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AttackSO : ScriptableObject
{

    public List<UnitSO.unitType> attackTargets;
    public List<Vector2> attackTargetTiles;
    public float attackTargetingRange;
    public int attackDamage;
    public float attackRate;
    public float attackRange;
    public float attackKnockback;
    public float attackDazedTime;
    public float attackAOE;
    public float attackAnimationHitDelay;
    public List<UnitSO.UnitAttackSpecial> attackSpecialList;
}
