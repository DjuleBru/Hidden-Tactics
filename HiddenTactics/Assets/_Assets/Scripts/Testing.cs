using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] Troop troop;
    List<UCW> ucws = new List<UCW>();
    List<WeaponVisual> weaponVisuals = new List<WeaponVisual>();
    List<UCWAnimatorManager> UCWAnimatorManagers = new List<UCWAnimatorManager>();
    UnitAnimatorManager[] unitAnimatorManagers;
    Mage[] mages;
    List<Unit> units;

    [SerializeField] float unitSpeed;

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


    private void MyCallback() {

        foreach (var uCWAnimatorManagers in UCWAnimatorManagers) {
            uCWAnimatorManagers.SetXY(X,Y);
        }

        foreach (var weaponVisual in weaponVisuals) {
            weaponVisual.SetXY(X, Y);
        }


        legendaryStateUpdated = legendaryState;
        magicStateUpdated = magicState;
        SetMagicState();
        SetLedengaryState();
    }

    [Button]
    public void InitializeTest() {

        units = troop.GetUnitInTroopList();

        foreach (Unit unit in units) {
            ucws.Add(unit as UCW);
            weaponVisuals.Add(unit.GetComponentInChildren<WeaponVisual>());
            UCWAnimatorManagers.Add(unit.GetComponentInChildren<UCWAnimatorManager>());
        }
    }

    #region UNITS GENERAL

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
