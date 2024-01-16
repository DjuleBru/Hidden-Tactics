using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UCW;

public class UnitAnimatorManager : MonoBehaviour
{

    protected Animator unitAnimator;
    protected Unit unit;

    protected bool walking;
    protected bool idle;

    protected float X;
    protected float Y;

    public void SetXY(float xValue, float yValue)
    {
        X = xValue; 
        Y = yValue;
        unitAnimator.SetFloat("X", xValue);
        unitAnimator.SetFloat("Y", yValue);
    }

    protected virtual void Awake()
    {
        unit = GetComponentInParent<Unit>();
        unitAnimator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        idle = true;
        unitAnimator.SetBool("Idle", idle);
    }

    public virtual void SetWalking()
    {
        idle = walking;
        walking = !walking;
        unitAnimator.SetBool("Walking", walking);
        unitAnimator.SetBool("Idle", idle);
    }

    public virtual void SetAttackTrigger()
    {
        if(!unit.GetHasAttack())
        {
            return;
        }
        unitAnimator.SetTrigger("BaseAttack");
    }

    public virtual void SetSideAttackTrigger()
    {
        if (!unit.GetHasSideAttack())
        {
            return;
        }
        unitAnimator.SetTrigger("SideAttack");
    }

    public virtual void SetSpecial1Trigger()
    {
        if (!unit.GetHasSpecial1())
        {
            return;
        }
        unitAnimator.SetTrigger("Special1");
    }

    public virtual void SetSpecial2Trigger()
    {
        if (!unit.GetHasSpecial2())
        {
            return;
        }
        unitAnimator.SetTrigger("Special2");
    }
}
