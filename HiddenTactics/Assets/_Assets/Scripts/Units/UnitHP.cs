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

    protected bool burning;
    protected float burningTimer;
    protected float burningTick = .5f;
    protected float burningDamage = 1f;

    protected bool poisoned;
    protected float poisonedTimer;
    protected float poisonedTick = 1f;
    protected float poisonedDamage = .5f;

    protected int garrisonedProtectionChance = 50;

    public event EventHandler<OnHealthChangedEventArgs> OnHealthChanged;
    public event EventHandler OnUnitArmorUsed;

    [SerializeField] protected string debug;

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
        unit.OnUnitFlamed += Unit_OnUnitFlamed;
        unit.OnUnitFlamedEnded += Unit_OnUnitFlamedEnded;
        unit.OnUnitPoisoned += Unit_OnUnitPoisoned;
        unit.OnUnitPoisonedEnded += Unit_OnUnitPoisonedEnded;
    }


    protected virtual void Update() {
        if (!IsServer) return;

        if (burning) {
            burningTimer += Time.deltaTime;

            if(burningTimer >= burningTick) {
                burningTimer = 0f;
                TakeDamageServerRpc(burningDamage, false, true);
            }
        }

        if (poisoned) {
            poisonedTimer += Time.deltaTime;

            if (poisonedTimer >= poisonedTick) {
                poisonedTimer = 0f;
                TakeDamageServerRpc(poisonedDamage, false, true);
            }
        }
    }

    private void Unit_OnUnitFlamedEnded(object sender, EventArgs e) {
        burning = false;
    }

    private void Unit_OnUnitFlamed(object sender, Unit.OnUnitSpecialEventArgs e) {
        if(!burning) {
            burningTimer = 0f;
        }

        burning = true;
    }

    private void Unit_OnUnitPoisonedEnded(object sender, EventArgs e) {
        poisoned = false;
    }

    private void Unit_OnUnitPoisoned(object sender, Unit.OnUnitSpecialEventArgs e) {
        if (!poisoned) {
            poisonedTimer = 0f;
        }

        poisoned = true;
    }

    protected void Unit_OnUnitReset(object sender, System.EventArgs e) {
        unitHP = unit.GetUnitSO().HP;

        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs {
            previousHealth = 0,
            newHealth = unitHP
        });

    }


    public void TakeDamage(float damage, IDamageSource damageSource, bool attackIgnoresArmor) {
        UnitAttack unitAttack = damageSource as UnitAttack;
        bool isRangedAttack = false;

        if(unitAttack != null) {
            isRangedAttack = unitAttack.GetActiveAttackSO().attackType == AttackSO.AttackType.ranged;
        }

        TakeDamageServerRpc(damage, isRangedAttack, attackIgnoresArmor);
    }

    public void Heal(float healAmount) {
        HealServerRpc(healAmount);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void TakeDamageServerRpc(float damage, bool isRangedAttack, bool attackIgnoresArmor) {
        if(unit.GetUnitSO().doesNotMoveGarrisonedUnit) {
            // There is a chance the unit is not damaged
            int damagedChance = UnityEngine.Random.Range(0, 100);

            if(damagedChance >= garrisonedProtectionChance) {
                TakeDamageClientRpc(damage, isRangedAttack, attackIgnoresArmor);
            }
        } else {
            TakeDamageClientRpc(damage, isRangedAttack, attackIgnoresArmor);
        }
    }

    [ClientRpc]
    protected virtual void TakeDamageClientRpc(float damage, bool isRangedAttack, bool attackIgnoresArmor) {

        if (attackIgnoresArmor) {
            unitHP -= damage;
        } else {
            unitHP -= (damage - unitArmor);
        }

        if(unitHP < 0) {
            unitHP = 0;
        }

        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs {
            previousHealth = unitHP + (damage - unitArmor),
            newHealth = unitHP
        });

        if (unitHP <= 0) {
            unit.Die();
            return;
        }

        if (!attackIgnoresArmor && unitArmor != 0 && unitArmor >= damage) {
            OnUnitArmorUsed?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void HealServerRpc(float healAmount) {
        HealClientRpc(healAmount);
    }

    [ClientRpc]
    protected virtual void HealClientRpc(float healAmount) {

        if (unitHP + healAmount > GetMaxHP()) {
            healAmount = GetMaxHP() - unitHP;
        }

        unitHP += healAmount;

        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs {
            previousHealth = unitHP - healAmount,
            newHealth = unitHP
        });
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
