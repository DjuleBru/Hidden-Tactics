using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpiritAttack : UnitAttack
{
    [SerializeField] private float dieHealMultiplier = 5f;
    protected override void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        base.UnitAI_OnStateChanged(sender, e);

        if (unitAI.IsDead()) {
            StartCoroutine(HealAOEWhenDying());
        }
    }

    private IEnumerator HealAOEWhenDying() {
        yield return new WaitForSeconds(activeAttackSO.meleeAttackAnimationHitDelay);
        foreach (Unit unitAOETarget in FindAOEAttackTargets(transform.position, 1.5f, true)) {
            // Die effect
            Debug.Log(unitAOETarget);
            unitAOETarget.GetComponent<UnitHP>().Heal(activeAttackSO.attackDamage * dieHealMultiplier);
        }
    }
}
