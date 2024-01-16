using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : Unit
{
    [SerializeField] private bool battleMage;
    [SerializeField] private bool supportMage;
    private MageAnimatorManager mageAnimatorManager;

    private int orbNumber;
    private void Awake()
    {
        mageAnimatorManager = GetComponentInChildren<MageAnimatorManager>();
    }

    public void CastShield()
    {
        mageAnimatorManager.SetShieldTrigger();
        orbNumber = 4;
    }

    public void LooseShieldOrb() { 
        if(orbNumber == 0) {
            return;
        } else {
            orbNumber--;
            mageAnimatorManager.SetOrbTrigger(orbNumber);
        }
    }

    public void CastMainSpell()
    {
        mageAnimatorManager.SetMainSpellCastTrigger();
    }

    public void ThrowMainSpell()
    {
        mageAnimatorManager.SetMainSpellThrowTrigger();
    }

    public void CastSideSpell()
    {
        mageAnimatorManager.SetSideSpellCastTrigger();
    }

    public void ThrowSideSpell()
    {
        mageAnimatorManager.SetSideSpellThrowTrigger();
    }
}
