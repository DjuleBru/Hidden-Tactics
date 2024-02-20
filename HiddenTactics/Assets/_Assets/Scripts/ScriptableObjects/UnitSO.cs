using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]

public class UnitSO : ScriptableObject {
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

    public enum unitType {
        ground,
        air,
        building
    }

    public int HP;
    public int armor;
    public float weight;
    public unitType moveType;
    public float unitMoveSpeed;

    public List<unitType> mainAttackTargets;
    public List<Vector2> mainAttackTargetTiles;
    public float mainAttackTargetingRange;
    public int mainAttackDamage;
    public float mainAttackRate;
    public float mainAttackRange;
    public float mainAttackKnockback;
    public float mainAttackDazedTime;
    public float mainAttackAOE;
    public float mainAttackAnimationHitDelay;
    public UnitAttackSpecial mainAttackSpecial;

    public bool hasSideAttack;
    public List<unitType> sideAttackTargets;
    public List<Vector2> sideAttackTargetTiles;
    public float sideAttackTargetingRange;
    public int sideAttackDamage;
    public float sideAttackRate;
    public float sideAttackRange;
    public float sidettackKnockback;
    public float sideAttackDazedTime;
    public float sideAttackAOE;
    public float sideAttackAnimationHitDelay;
    public UnitAttackSpecial sideAttackSpecial;

}
