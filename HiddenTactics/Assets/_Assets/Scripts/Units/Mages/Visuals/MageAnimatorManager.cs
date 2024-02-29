using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageAnimatorManager : UnitAnimatorManager
{
    [SerializeField] private Animator mainSideSpellAnimator;
    [SerializeField] private Animator shieldAnimator;

    [SerializeField] private Transform mageBodyVisual;
    [SerializeField] private Transform spellVisual;

    private bool castingSideSpell;

    protected override void Update()
    {
        
        if (unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("SideSpell_Cast") | unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("SideSpell_Hold") | unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("SideSpell_Throw"))
        {
            // Unit is casting side spell
            castingSideSpell = true;
            SetVisualScaleX();
        } else
        {
            if (!castingSideSpell)
            {
                return;
            }
            castingSideSpell = false;
            mageBodyVisual.localScale = new Vector3(1, 1, 1);
            spellVisual.localScale = new Vector3(1, 1, 1);
        }
    }

    public void SetMainSpellCastTrigger()
    {
        unitAnimator.SetBool("Idle", false);
        unitAnimator.SetBool("Walking", false);

        mainSideSpellAnimator.SetTrigger("MainSpell_Cast");
        unitAnimator.SetTrigger("MainSpell_Cast");

        mainSideSpellAnimator.SetTrigger("MainSpell_Hold");
        unitAnimator.SetTrigger("MainSpell_Hold");
    }

    public void SetMainSpellThrowTrigger()
    {
        mainSideSpellAnimator.SetTrigger("MainSpell_Throw");
        unitAnimator.SetTrigger("MainSpell_Throw");

        unitAnimator.SetBool("Idle", idle);
        unitAnimator.SetBool("Walking", walking);
    }

    public void SetSideSpellCastTrigger()
    {
        unitAnimator.SetBool("Idle", false);
        unitAnimator.SetBool("Walking", false);

        mainSideSpellAnimator.SetTrigger("SideSpell_Cast");
        unitAnimator.SetTrigger("SideSpell_Cast");

        mainSideSpellAnimator.SetTrigger("SideSpell_Hold");
        unitAnimator.SetTrigger("SideSpell_Hold");
    }

    public void SetSideSpellThrowTrigger()
    {

        mainSideSpellAnimator.SetTrigger("SideSpell_Throw");
        unitAnimator.SetTrigger("SideSpell_Throw");

        unitAnimator.SetBool("Idle", idle);
        unitAnimator.SetBool("Walking", walking);
    }

    private void SetVisualScaleX()
    {
        if (X < 0)
        {
            mageBodyVisual.localScale = new Vector3(-1, 1, 1);
            spellVisual.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            mageBodyVisual.localScale = new Vector3(1, 1, 1);
            spellVisual.localScale = new Vector3(1, 1, 1);
        }
    }

    public void SetShieldTrigger()
    {
        shieldAnimator.SetTrigger("ShieldSpell_Cast");
        unitAnimator.SetTrigger("ShieldSpell_Cast");
    }

    public void SetOrbTrigger(int orbNumber)
    {
        string trigger = "ShieldSpell_" + orbNumber + "Orb";
        shieldAnimator.SetTrigger(trigger);
    }

}
