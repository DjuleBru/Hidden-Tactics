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

    public enum UnitKeyword {
        Large,
        Mounted,
        Charge,
        Beast,
        AntiLarge,
        Spawner,
        Siege,
        Flying,
        Shielded,
        Jumper,
        RoyalAura,
        BushyBeard,
        Support,
        Hybrid,
        Healer,
        Restricted,
        DeathTrigger,
        Unstoppable,
        BloodFlag,
        Destructible,
        Uncrossable,
        Garrisoned,
    }

    public List<UnitKeyword> unitKeywordsList;

    public ITargetable.TargetType unitTargetType;

    public int HP;
    public int armor;
    public float mass;
    public MoveType moveType;
    public float unitMoveSpeed;

    public bool doesNotMoveGarrisonedUnit;
    public bool isInvisibleGarrisonedUnit;

    public WeaponSO mainWeaponSO;
    public AttackSO mainAttackSO;
    public AttackSO sideAttackSO;
    public AttackSO jumpAttackSO;
    public AttackSO deathTriggerAttackSO;

    public int damageToVillages;
}
