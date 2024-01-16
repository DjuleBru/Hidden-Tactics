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
    [FoldoutGroup("Mounted units parameters"), ShowIf("upgradedIsMountedUnit")]
    [SerializeField] private bool upgradedMountAttackAnimation;


    [FoldoutGroup("Attack decomposition")]
    [SerializeField] private bool hasAttackStart_End;
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
    public event EventHandler OnMainWeaponActivated;
    public event EventHandler OnSideWeaponActivated;

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

    public void SetMainWeaponActive() {
        OnMainWeaponActivated?.Invoke(this, new EventArgs());
    }
    public void SetSideWeaponActive() {
        OnSideWeaponActivated?.Invoke(this, new EventArgs());
    }

    public bool GetIsMountedUnit() {
        return isMountedUnit;
    }
    public bool GetMountAttackAnimation()
    {
        return mountAttackAnimation;
    }

    public bool GetHasAttackStart_End()
    {
        return hasAttackStart_End;
    }

    public bool GetStartIsWeaponSprite()
    {
        return startIsWeaponSprite;
    }

}
