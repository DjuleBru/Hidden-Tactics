using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class UCW : Unit {

    [FoldoutGroup("Mounted units parameters")]
    [SerializeField] private bool isMountedUnit;
    [FoldoutGroup("Mounted units parameters")]
    [SerializeField] private bool upgradedIsMountedUnit;
    [FoldoutGroup("Mounted units parameters"), ShowIf("isMountedUnit")]
    [SerializeField] private bool mountAttackAnimation;
    [FoldoutGroup("Mounted units parameters"), ShowIf("isMountedUnit")]
    [SerializeField] private bool mountedUnit_HasBodyAttackAnimation;
    [FoldoutGroup("Mounted units parameters"), ShowIf("upgradedIsMountedUnit")]
    [SerializeField] private bool upgradedMountAttackAnimation;


    [FoldoutGroup("Attack decomposition"), ShowIf("hasAttackStart_End")]
    [SerializeField] private bool startIsWeaponSprite;

    public enum MagicState {
        Base,
        Bleed,
        Fear,
        Fire,
        Ice,
        Poison,
        Shock,
    }

    public enum LegendaryState {
        Base,
        IcebergBlade,
        ThunderboltSword,
        ViperScimitar,
        VolcanoMace,
    }

    private MagicState magicState;
    private LegendaryState legendaryState;

    public event EventHandler OnMagicStateChanged;
    public event EventHandler OnLegendaryStateChanged;

    public override void UpgradeUnit()
    {
        isMountedUnit = upgradedIsMountedUnit;
        mountAttackAnimation = upgradedMountAttackAnimation;
        base.UpgradeUnit();
    }

    public void SetLegendaryState(LegendaryState legendaryState) {
        this.legendaryState = legendaryState;
        OnLegendaryStateChanged?.Invoke(this, new EventArgs());
    }

    public void SetMagicState(MagicState magicState) {
        this.magicState = magicState;
        OnMagicStateChanged?.Invoke(this, new EventArgs());
    }

    public LegendaryState GetLegendaryState() {
        return legendaryState;
    }

    public MagicState GetMagicState() {
        return magicState;
    }

    public bool GetIsMountedUnit() {
        return isMountedUnit;
    }

    public bool MountedUnit_HasBodyAttackAnimation() {
        return mountedUnit_HasBodyAttackAnimation;
    }

    public bool GetMountAttackAnimation()
    {
        return mountAttackAnimation;
    }

    public bool GetStartIsWeaponSprite()
    {
        return startIsWeaponSprite;
    }

}
