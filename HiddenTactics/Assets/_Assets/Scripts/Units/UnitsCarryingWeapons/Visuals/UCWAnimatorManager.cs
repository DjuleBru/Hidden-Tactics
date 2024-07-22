using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UCWAnimatorManager : UnitAnimatorManager
{
    [SerializeField] private Animator ucwAnimator;
    private UCW ucw;

    private UCW.LegendaryState legendaryState;
    private UCW.MagicState magicState;

    public event EventHandler OnUcwAttack;
    public event EventHandler OnUcwAttackStart;
    public event EventHandler OnUcwAttackEnd;

    protected override void Awake() {
        base.Awake();
        ucw = GetComponentInParent<UCW>();
        ucwAnimator = GetComponent<Animator>();

        magicState = UCW.MagicState.Base;
        legendaryState = UCW.LegendaryState.Base;
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        ucw.OnLegendaryStateChanged += Ucw_OnLegendaryStateChanged;
        ucw.OnMagicStateChanged += Ucw_OnMagicStateChanged;

    }

    public override void SetAttackTrigger() {
        if(ucw.GetIsMountedUnit() && !ucw.GetMountAttackAnimation()) {
            return;
        }

        if(legendaryState != UCW.LegendaryState.Base) {
            ucwAnimator.SetTrigger("LegendaryAttack");
        } else {
            ucwAnimator.SetTrigger("BaseAttack");
        }
    }

    public void SetAttackStartTrigger()
    {
        if (ucw.GetIsMountedUnit()) {
            if (!ucw.MountedUnit_HasBodyAttackAnimation()) return;
        }

        ucwAnimator.ResetTrigger("Attack_End");
        ucwAnimator.SetTrigger("Attack_Start");
        ucwAnimator.SetBool("Walking", false);
        ucwAnimator.SetBool("Idle", false);
    }

    public void SetAttackEndTrigger()
    {
        if (ucw.GetIsMountedUnit()) {
            if (!ucw.MountedUnit_HasBodyAttackAnimation()) return;
        }
        ucwAnimator.SetTrigger("Attack_End");
    }

    private void Ucw_OnMagicStateChanged(object sender, System.EventArgs e) {
        magicState = ucw.GetMagicState();
    }

    private void Ucw_OnLegendaryStateChanged(object sender, System.EventArgs e) {
        legendaryState = ucw.GetLegendaryState();
    }

    protected override void UnitAttack_OnUnitAttack(object sender, System.EventArgs e) {
        OnUcwAttack?.Invoke(this, e);

        if (ucw.GetIsMountedUnit()) {
            if (!ucw.MountedUnit_HasBodyAttackAnimation()) return;
        }

        unitAnimator.SetTrigger("BaseAttack");
    }
    protected override void UnitAttack_OnUnitAttackStarted(object sender, System.EventArgs e) {
        base.UnitAttack_OnUnitAttackStarted(sender, e);
        OnUcwAttackStart?.Invoke(this, EventArgs.Empty);
        SetAttackStartTrigger();
    }

    protected override void UnitAttack_OnUnitAttackEnded(object sender, System.EventArgs e) {
        base.UnitAttack_OnUnitAttackEnded(sender, e);
        OnUcwAttackEnd?.Invoke(this, EventArgs.Empty);
        SetAttackEndTrigger();
    }

}
