using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitHP : NetworkBehaviour, IDamageable
{
    protected Unit unit;
    protected float unitHP;
    protected int unitArmor;

    protected int garrisonedProtectionChance = 50;

    public event EventHandler<OnHealthChangedEventArgs> OnHealthChanged;

    public class OnHealthChangedEventArgs : EventArgs {
        public float previousHealth;
        public float newHealth;
    }

    protected virtual void Awake() {
        unit = GetComponent<Unit>();    
        unitHP = unit.GetUnitSO().HP;
        unitArmor = unit.GetUnitSO().armor;
    }

    protected virtual void Start() {
        unit.OnUnitReset += Unit_OnUnitReset;
    }

    protected void Unit_OnUnitReset(object sender, System.EventArgs e) {
        unitHP = unit.GetUnitSO().HP;

        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs {
            previousHealth = 0,
            newHealth = unitHP
        });

    }


    public void TakeDamage(float damage) {
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void TakeDamageServerRpc(float damage) {
        if(unit.GetUnitSO().isGarrisonedUnit) {
            // There is a chance the unit is not damaged
            int damagedChance = UnityEngine.Random.Range(0, 100);

            if(damagedChance >= garrisonedProtectionChance) {
                TakeDamageClientRpc(damage);
            }
        } else {
            TakeDamageClientRpc(damage);
        }
    }

    [ClientRpc]
    protected virtual void TakeDamageClientRpc(float damage) {
        unitHP -= (damage - unitArmor);

        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs {
            previousHealth = unitHP + (damage - unitArmor),
            newHealth = unitHP
        });

        if (unitHP <= 0) {
            unit.Die();
        }
    }

    public void BuffUnitArmor(int unitArmorBuff) {
        unitArmor += unitArmorBuff;
    }

    public void DebuffUnitArmor(int unitArmorDebuff) {
        unitArmor -= unitArmorDebuff;
    }

    public float GetMaxHP() {
        return unit.GetUnitSO().HP;
    }
    public float GetHP() {
        return unitHP;
    }

}
