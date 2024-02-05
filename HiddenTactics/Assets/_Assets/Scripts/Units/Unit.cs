using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public event EventHandler OnUnitUpgraded;

    [SerializeField] protected bool hasAttack;
    [SerializeField] protected bool hasSideAttack;
    [SerializeField] protected bool hasSpecial1;
    [SerializeField] protected bool hasSpecial2;

    [SerializeField] protected UnitSO unitSO;

    public virtual void UpgradeUnit() {
        OnUnitUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public bool GetHasAttack()
    {
        return hasAttack;
    }

    public bool GetHasSpecial1()
    {
        return hasSpecial1;
    }

    public bool GetHasSpecial2()
    {
        return hasSpecial2;
    }

    public bool GetHasSideAttack()
    {
        return hasSideAttack;
    }

    public UnitSO GetUnitSO() {
        return unitSO;
    }
}
