using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UCW;

public class UnitAnimatorManager : MonoBehaviour
{

    protected Animator unitAnimator;
    protected Unit unit;
    protected UnitAI unitAI;

    protected bool walking;
    protected bool idle;

    protected float X;
    protected float Y;

    protected virtual void Awake()
    {
        unit = GetComponentInParent<Unit>();
        unitAI = GetComponentInParent<UnitAI>();
        unitAnimator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        // Randomize Idle animation starting frame
        float randomOffset = Random.Range(0f, 1f);
        unitAnimator.Play("Idle", 0, randomOffset);
        unitAnimator.SetBool("Idle", true);

        unitAI.OnStateChanged += UnitAI_OnStateChanged;
        unit.GetParentTroop().OnTroopPlaced += UnitParentTroop_OnTroopPlaced;
    }



    protected virtual void Update() {
        if (walking) {
            Vector2 moveDir = unitAI.GetMoveDir2D();
            SetXY(moveDir.x, moveDir.y);
        }
    }

    private void UnitParentTroop_OnTroopPlaced(object sender, System.EventArgs e) {
        SetUnitWatchDirection();
    }

    private void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        SetUnitWatchDirection();
        UpdateAnimatorParameters();
    }

    private void SetUnitWatchDirection() {
        if(unit.GetParentTroop().GetTroopGridPosition().x >= BattleGrid.Instance.GetGridWidth()/2) {
            SetXY(-1,-1);
        } else {
            SetXY(1,-1);
        };
    }

    public void SetXY(float xValue, float yValue) {
        X = xValue;
        Y = yValue;
        unitAnimator.SetFloat("X", xValue);
        unitAnimator.SetFloat("Y", yValue);
    }

    public virtual void SetWalking()
    {
        idle = walking;
        walking = !walking;

        string animationName = "Walk";

        if(idle) {
            animationName = "Idle";
        }

        unitAnimator.SetBool("Walking", walking);
        unitAnimator.SetBool("Idle", idle);


        float randomOffset = Random.Range(0f, 1f);
        unitAnimator.Play(animationName, 0, randomOffset);
    }

    private void UpdateAnimatorParameters() {
        string animationName = "Walk";

        if (unitAI.isWalking()) {
            walking = true;
            idle = false;

            animationName = "Walk";
        }
        if(unitAI.isIdle()) {
            walking = false;
            idle = true;

            animationName = "Idle";
        }

        unitAnimator.SetBool("Walking", walking);
        unitAnimator.SetBool("Idle", idle);

        float randomOffset = Random.Range(0f, 1f);
        unitAnimator.Play(animationName, 0, randomOffset);

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
