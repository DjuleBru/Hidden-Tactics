using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]

public class UnitSO : ScriptableObject {
    public enum MoveType {
        ground,
        air,
    }

    public enum UnitTag {
        large,
        mounted,
    }

    public List<UnitTag> unitTagList;

    public ITargetable.TargetType unitTargetType;

    public int HP;
    public int armor;
    public float mass;
    public MoveType moveType;
    public float unitMoveSpeed;

    public bool isGarrisonedUnit;

    public WeaponSO mainWeaponSO;
    public AttackSO mainAttackSO;
    public AttackSO sideAttackSO;
    public AttackSO specialAttackSO;

    public int damageToVillages;
}
