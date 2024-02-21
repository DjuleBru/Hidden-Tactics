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
    public float mass;
    public unitType moveType;
    public float unitMoveSpeed;

    public AttackSO mainAttackSO;
    public AttackSO sideAttackSO;
    public AttackSO specialAttackSO;
}
