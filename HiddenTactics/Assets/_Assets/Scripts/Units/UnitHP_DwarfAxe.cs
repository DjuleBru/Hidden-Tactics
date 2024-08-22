using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitHP_DwarfAxe : UnitHP
{
    protected UnitAI_DwarfAxe unitAI_DwarfAxe;

    protected override void Awake() {
        base.Awake();
        unitAI_DwarfAxe = GetComponent<UnitAI_DwarfAxe>();
    }

    [ClientRpc]
    protected override void TakeDamageClientRpc(float damage, bool attackIgnoresArmor) {
        if(unitAI_DwarfAxe.GetWearingShield()) {
            unitAI_DwarfAxe.TakeShieldDamage();
            return;
        }

        base.TakeDamageClientRpc(damage, attackIgnoresArmor);
    }
}
