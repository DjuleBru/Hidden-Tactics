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

    protected override void Awake() {
        base.Awake();
        ucw = GetComponentInParent<UCW>();
        ucwAnimator = GetComponent<Animator>();

        magicState = UCW.MagicState.Base;
        legendaryState = UCW.LegendaryState.Base;
    }

    protected override void Start() {
        base.Start();
        ucw.OnLegendaryStateChanged += Ucw_OnLegendaryStateChanged;
        ucw.OnMagicStateChanged += Ucw_OnMagicStateChanged;
    }

    public override void SetAttackTrigger() {
        if(ucw.GetIsMountedUnit() & !ucw.GetMountAttackAnimation()) {
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
        if (!ucw.GetHasAttackStart_End() | ucw.GetStartIsWeaponSprite())
        {
            return;
        }
        ucwAnimator.SetTrigger("Attack_Start");
        ucwAnimator.SetBool("Walking", false);
        ucwAnimator.SetBool("Idle", false);
    }

    public void SetAttackEndTrigger()
    {
        if (!ucw.GetHasAttackStart_End() | ucw.GetStartIsWeaponSprite())
        {
            return;
        }
        ucwAnimator.SetTrigger("Attack_End");
    }

    private void Ucw_OnMagicStateChanged(object sender, System.EventArgs e) {
        magicState = ucw.GetMagicState();
    }

    private void Ucw_OnLegendaryStateChanged(object sender, System.EventArgs e) {
        legendaryState = ucw.GetLegendaryState();
    }

}
