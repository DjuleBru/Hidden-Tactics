using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitBuffManager : NetworkBehaviour
{

    protected int kingBuffNumber;
    protected int flagBearerBuffNumber;
    protected int yellowBeardDwarfNumber;

    public event EventHandler OnAttackRateBuffed;
    public event EventHandler OnAttackRateDebuffed;
    public event EventHandler OnAttackDamageBuffed;
    public event EventHandler OnAttackDamageDebuffed;
    public event EventHandler OnMoveSpeedBuffed;
    public event EventHandler OnMoveSpeedDebuffed;

    protected float attackRateMultiplier = 1;
    protected float attackDamageMultiplier = 1;
    protected float moveSpeedMultiplier = 1;
    protected float attackKnockbackBuffAbsolute;

    private UnitAttack unitAttack;
    private UnitMovement unitMovement;
    private UnitAI unitAI;
    private Unit unit;

    private bool webbed;

    private void Awake() {
        unitAttack = GetComponent<UnitAttack>();
        unitMovement = GetComponent<UnitMovement>();
        unitAI = GetComponent<UnitAI>();
        unit = GetComponent<Unit>();
    }

    public override void OnNetworkSpawn() {
        if (unit.GetUnitIsOnlyVisual()) return;
        unitAI.OnStateChanged += UnitAI_OnStateChanged;
        unit.OnUnitReset += Unit_OnUnitReset;
        unit.OnUnitWebbed += Unit_OnUnitWebbed;
        unit.OnUnitWebbedEnded += Unit_OnUnitWebbedEnded;
    }

    private void Unit_OnUnitReset(object sender, EventArgs e) {
        ResetBuffs();
    }

    private void UnitAI_OnStateChanged(object sender, EventArgs e) {
        if (unitAI.IsDead()) {
            ResetBuffs();
        }
    }

    private void Unit_OnUnitWebbedEnded(object sender, EventArgs e) {
        if(webbed) {
            BuffMoveSpeed(.75f);
        }
        webbed = false;
    }

    private void Unit_OnUnitWebbed(object sender, Unit.OnUnitSpecialEventArgs e) {

        if(!webbed) {
            BuffMoveSpeed(-.75f);
        }

        webbed = true;
    }

    public void AddBuffedBySupportUnit(float buffMultiplier, SupportUnit.SupportUnitType unitType) {
        if (unitType == SupportUnit.SupportUnitType.King) {
            kingBuffNumber++;
            if (kingBuffNumber == 1) {
                BuffAttackRate(buffMultiplier);
            }
        }

        if (unitType == SupportUnit.SupportUnitType.FlagBearer) {
            flagBearerBuffNumber++;
            if (flagBearerBuffNumber == 1) {
                BuffAttackDamage(buffMultiplier);
            }
        }

        if (unitType == SupportUnit.SupportUnitType.YellowBeardDwarf) {
            yellowBeardDwarfNumber++;
            if (yellowBeardDwarfNumber == 1) {
                BuffMoveSpeed(buffMultiplier);
            }
        }
    }

    public void BuffAttackDamage(float attackDamagebuff) {
        BuffAttackDamageServerRpc(attackDamagebuff);
    }

    public void ResetAttackDamage() {
        ResetAttackDamageServerRpc();
    }

    public void BuffAttackRate(float attackRatebuff) {
        BuffAttackRateServerRpc(attackRatebuff);
    }

    public void ResetAttackRate() {
        ResetAttackRateServerRpc();
    }

    public void BuffMoveSpeed(float moveSpeedbuff) {
        BuffMoveSpeedServerRpc(moveSpeedbuff);
    }

    public void ResetMoveSpeed() {
        ResetMoveSpeedServerRpc();
    }

    public void RemoveBuffedSupportUnit(SupportUnit.SupportUnitType unitType) {
        if (unitType == SupportUnit.SupportUnitType.King) {
            kingBuffNumber--;
            if (kingBuffNumber <= 0) {
                ResetAttackRate();
            }
        }

        if (unitType == SupportUnit.SupportUnitType.FlagBearer) {
            flagBearerBuffNumber--;
            if (flagBearerBuffNumber <= 0) {
                ResetAttackDamage();
            }
        }

        if (unitType == SupportUnit.SupportUnitType.YellowBeardDwarf) {
            flagBearerBuffNumber--;
            if (flagBearerBuffNumber <= 0) {
                ResetMoveSpeed();
            }
        }
    }

    public void BuffAttackKnockbackAbsolute(float attackKnockbackbuff) {
        attackKnockbackBuffAbsolute += attackKnockbackbuff;
        unitAttack.SetAttackKnockbackAbsolute(attackKnockbackBuffAbsolute);
    }

    public void DebuffAttackKnockbackAbsolute(float attackKnockbackDebuff) {
        attackKnockbackBuffAbsolute -= attackKnockbackDebuff;
        unitAttack.SetAttackKnockbackAbsolute(attackKnockbackBuffAbsolute);
    }

    private void ResetBuffs() {
        if (attackRateMultiplier != 1) {
            ResetAttackRate();
        }

        if (attackDamageMultiplier != 1) {
            ResetAttackDamage();
        }

        if (moveSpeedMultiplier != 1) {
            ResetMoveSpeed();
        }

        if (attackKnockbackBuffAbsolute != 0) {
            attackKnockbackBuffAbsolute = 0;
        }
    }

    #region ATTACK RATE

    [ServerRpc(RequireOwnership = false)]
    private void BuffAttackRateServerRpc(float attackRatebuff) {

        attackRateMultiplier -= attackRatebuff;
        unitAttack.SetAttackRateMultiplier(attackRateMultiplier);
        BuffAttackRateClientRpc();
    }

    [ClientRpc]
    private void BuffAttackRateClientRpc() {
        OnAttackRateBuffed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetAttackRateServerRpc() {
        attackRateMultiplier = 1f;
        unitAttack.SetAttackRateMultiplier(attackRateMultiplier);
        ResetAttackRateClientRpc();
    }

    [ClientRpc]
    private void ResetAttackRateClientRpc() {
        OnAttackRateDebuffed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region ATTACK DAMAGE
    [ServerRpc(RequireOwnership = false)]
    private void BuffAttackDamageServerRpc(float attackDamageBuff) {
        attackDamageMultiplier += attackDamageBuff;
        unitAttack.SetAttackDamageMultiplier(attackDamageMultiplier);
        BuffAttackDamageClientRpc();
    }

    [ClientRpc]
    private void BuffAttackDamageClientRpc() {
        OnAttackDamageBuffed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetAttackDamageServerRpc() {
        attackDamageMultiplier = 1f;
        unitAttack.SetAttackDamageMultiplier(attackDamageMultiplier);
        ResetAttackDamageClientRpc();
    }

    [ClientRpc]
    private void ResetAttackDamageClientRpc() {
        OnAttackDamageDebuffed?.Invoke(this, EventArgs.Empty);
    }


    #endregion

    #region MOVE SPEED
    [ServerRpc(RequireOwnership = false)]
    private void BuffMoveSpeedServerRpc(float moveSpeedBuff) {
        moveSpeedMultiplier += moveSpeedBuff;
        unitMovement.SetMoveSpeedMultiplier(moveSpeedMultiplier);

        if(unitMovement.GetMoveSpeedMultiplier() > 1) {
            BuffMoveSpeedClientRpc();
        } else {
            ResetMoveSpeedClientRpc();
        }
    }

    [ClientRpc]
    private void BuffMoveSpeedClientRpc() {
        OnMoveSpeedBuffed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetMoveSpeedServerRpc() {
        moveSpeedMultiplier = 1f;
        unitMovement.SetMoveSpeedMultiplier(moveSpeedMultiplier);
        ResetMoveSpeedClientRpc();
    }

    [ClientRpc]
    private void ResetMoveSpeedClientRpc() {
        OnMoveSpeedDebuffed?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    public float GetMoveSpeedMultiplier() {
        return moveSpeedMultiplier;
    }

    public float GetAttackRateMultiplier() {
        return attackRateMultiplier;
    }

    public float GetAttackDamageMultiplier() {
        return attackDamageMultiplier;
    }

}
