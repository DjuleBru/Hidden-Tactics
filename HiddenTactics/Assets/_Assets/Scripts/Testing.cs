using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Testing : MonoBehaviour
{
    UCW[] ucws;
    WeaponVisual[] weaponVisuals;
    UCWAnimatorManager[] UCWAnimatorManagers;
    UnitAnimatorManager[] unitAnimatorManagers;
    Mage[] mages;
    Unit[] units;

    [SerializeField] float unitSpeed;
    private float statSpeedToGameSpeed = 6.25f;

    [OnValueChanged("MyCallback"), Range(-1, 1)]
    [SerializeField] private float X;
    [OnValueChanged("MyCallback"), Range(-1, 1)]
    [SerializeField] private float Y;

    [OnValueChanged("MyCallback"), TabGroup("Units Carrying Weapons")]
    [SerializeField] UCW.LegendaryState legendaryState;
    [OnValueChanged("MyCallback"), TabGroup("Units Carrying Weapons")]
    [SerializeField] UCW.MagicState magicState;

    UCW.LegendaryState legendaryStateUpdated;
    UCW.MagicState magicStateUpdated;

    bool walking;

    BattlefieldAnimationManager battlefieldAnimationManager;
    Battlefield battlefieldParent;

    private void Start() {
        ucws = GetComponentsInChildren<UCW>();
        weaponVisuals = GetComponentsInChildren<WeaponVisual>();
        UCWAnimatorManagers = GetComponentsInChildren<UCWAnimatorManager>();
        unitAnimatorManagers = GetComponentsInChildren<UnitAnimatorManager>();
        mages = GetComponentsInChildren<Mage>();
        units = GetComponentsInChildren<Unit>();

        SetUnitWatchingDirection();

        unitSpeed = units[0].GetUnitSO().unitMoveSpeed;

        BattlefieldAnimationManager.Instance.OnBattlefieldSlammed += Instance_OnBattlefieldSlammed;
        battlefieldParent = GetComponentInParent<Battlefield>();
    }

    private void Instance_OnBattlefieldSlammed(object sender, System.EventArgs e) {
        Invoke("SetWalkingSpeed", 1.5f);
    }

    private void Update() {
        if (walking) {
            foreach (var unit in units) {
                if(battlefieldParent.playerNumber == 1) {
                    unit.transform.position += new Vector3(1, 0, 0) * unitSpeed / statSpeedToGameSpeed * Time.deltaTime;
                } else {
                    unit.transform.position += new Vector3(-1, 0, 0) * unitSpeed / statSpeedToGameSpeed * Time.deltaTime;
                }
            }
        }
    }

    private void MyCallback() {
        foreach (var weaponVisual in weaponVisuals) {
            weaponVisual.SetXY(X, Y);
        }

        foreach (var UnitAnimatorManager in unitAnimatorManagers) {
            UnitAnimatorManager.SetXY(X, Y);
        }

        legendaryStateUpdated = legendaryState;
        magicStateUpdated = magicState;
        SetMagicState();
        SetLedengaryState();
    }

    #region UNITS GENERAL

    [Button, TabGroup("Unit")]
    public void SetWalking()
    {
        foreach (var unitAnimatorManager in unitAnimatorManagers)
        {
            unitAnimatorManager.SetWalking();
        }
    }

    [Button, TabGroup("Unit")]
    public void SetWalkingSpeed() {
        foreach (var unitAnimatorManager in unitAnimatorManagers) {
            unitAnimatorManager.SetWalking();
            walking = true;
        }
    }


    [Button, TabGroup("Unit")]
    public void UpgradeUnits()
    {
        foreach (var ucw in ucws)
        {
            ucw.UpgradeUnit();
        }
    }

    [Button, TabGroup("Unit")]
    public void SetAttackTriggerUnits()
    {
        foreach (var unitAnimatorManager in unitAnimatorManagers)
        {
            unitAnimatorManager.SetAttackTrigger();
        }
    }

    [Button, TabGroup("Unit")]
    public void SetSideAttackTriggerUnits()
    {
        foreach (var unitAnimatorManager in unitAnimatorManagers)
        {
            unitAnimatorManager.SetSideAttackTrigger();
        }
    }

    [Button, TabGroup("Unit")]
    public void SetSpecial1TriggerUnits()
    {
        foreach (var unitAnimatorManager in unitAnimatorManagers)
        {
            unitAnimatorManager.SetSpecial1Trigger();
        }
    }

    [Button, TabGroup("Unit")]
    public void SetSpecial2TriggerUnits()
    {
        foreach (var unitAnimatorManager in unitAnimatorManagers)
        {
            unitAnimatorManager.SetSpecial2Trigger();
        }
    }

    #endregion

    #region UNITS CARRYING WEAPONS


    [Button, TabGroup("Units Carrying Weapons")]
    public void SetAttackTrigger()
    {
        foreach (var weaponVisual in weaponVisuals)
        {
            weaponVisual.SetAttackTrigger();
        }

        foreach (var UCWAnimatorManager in UCWAnimatorManagers)
        {
            UCWAnimatorManager.SetAttackTrigger();
        }
    }

    [Button,TabGroup("Units Carrying Weapons"), TabGroup("Weapon")]
    public void SetAttackTriggerWeapon()
    {
        foreach (var weaponVisual in weaponVisuals)
        {
            weaponVisual.SetAttackTrigger();
        }
    }

    [Button,TabGroup("Units Carrying Weapons"), TabGroup("Mount")]
    public void SetAttackTriggerMount()
    {

        foreach (var UCWAnimatorManager in UCWAnimatorManagers)
        {
            UCWAnimatorManager.SetAttackTrigger();
        }
    }

    [Button, TabGroup("Units Carrying Weapons"), TabGroup("Attack Decomposition")]
    public void SetAttackStartTrigger()
    {
        foreach (var weaponVisual in weaponVisuals)
        {
            weaponVisual.SetAttackStartTrigger();
        }

        foreach (var UCWAnimatorManager in UCWAnimatorManagers)
        {
            UCWAnimatorManager.SetAttackStartTrigger();
        }
    }

    [Button, TabGroup("Units Carrying Weapons"), TabGroup("Attack Decomposition")]
    public void SetAttackEndTrigger()
    {
        foreach (var weaponVisual in weaponVisuals)
        {
            weaponVisual.SetAttackEndTrigger();
        }

        foreach (var UCWAnimatorManager in UCWAnimatorManagers)
        {
            UCWAnimatorManager.SetAttackEndTrigger();
        }
    }

    [Button, TabGroup("Units Carrying Weapons"), TabGroup("Attack Decomposition"), TabGroup("Weapon")]
    public void SetAttackStartTriggerWeapon()
    {
        foreach (var weaponVisual in weaponVisuals)
        {
            weaponVisual.SetAttackStartTrigger();
        }
    }

    [Button,  TabGroup("Units Carrying Weapons"), TabGroup("Attack Decomposition"), TabGroup("Weapon")]
    public void SetAttackEndTriggerWeapon()
    {
        foreach (var weaponVisual in weaponVisuals)
        {
            weaponVisual.SetAttackEndTrigger();
        }

    }

    [Button, TabGroup("Units Carrying Weapons")]
    public void SetMainWeaponActive()
    {
        foreach (var weaponVisual in weaponVisuals)
        {
            weaponVisual.SetMainWeaponActive();
        }
    }

    [Button, TabGroup("Units Carrying Weapons")]
    public void SetSideWeaponActive()
    {
        foreach (var weaponVisual in weaponVisuals)
        {
            weaponVisual.SetSideWeaponActive();
        }
    }

    #endregion

    #region MAGES
    [Button, TabGroup("Mages")]
    public void ShieldCast()
    {
        foreach (var mage in mages)
        {
            mage.CastShield();
        }
    }

    [Button, TabGroup("Mages")]
    public void LooseOrb()
    {
        foreach (var mage in mages)
        {
            mage.LooseShieldOrb();
        }
    }

    [Button, TabGroup("Mages")]
    public void CastMainSpell()
    {
        foreach (var mage in mages)
        {
            mage.CastMainSpell();
        }
    }

    [Button, TabGroup("Mages")]
    public void ThrowMainSpell()
    {
        foreach (var mage in mages)
        {
            mage.ThrowMainSpell();
        }
    }

    [Button, TabGroup("Mages")]
    public void CastSideSpell()
    {
        foreach (var mage in mages)
        {
            mage.CastSideSpell();
        }
    }

    [Button, TabGroup("Mages")]
    public void ThrowSideSpell()
    {
        foreach (var mage in mages)
        {
            mage.ThrowSideSpell();
        }
    }
    #endregion

    public void SetMagicState() {
        foreach (var ucw in ucws) {
            ucw.SetMagicState(magicStateUpdated);
        }
    }

    public void SetLedengaryState() {
        foreach (var ucw in ucws) {
            ucw.SetLegendaryState(legendaryStateUpdated);
        }
    }

    protected void SetUnitWatchingDirection() {
        X = 1;
        Y = -1;
        MyCallback();
    }

}
