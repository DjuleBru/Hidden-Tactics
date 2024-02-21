using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using static UCW;

public class UnitAnimatorManager : MonoBehaviour
{

    protected Animator unitAnimator;
    [SerializeField] protected Animator unitShaderAnimator;
    protected Unit unit;
    protected UnitAI unitAI;
    protected UnitMovement unitMovement;
    protected UnitAttack unitAttack;

    protected bool walking;
    protected bool idle;
    protected bool dead;
    protected bool unitAIStateHasChanged;

    protected float X;
    protected float Y;

    protected virtual void Awake() {
        unit = GetComponentInParent<Unit>();
        unitAI = GetComponentInParent<UnitAI>();
        unitMovement = GetComponentInParent<UnitMovement>();
        unitAnimator = GetComponent<Animator>();
        unitAttack = GetComponentInParent<UnitAttack>();
    }

    protected virtual void Start() {
        // Randomize Idle animation starting frame
        float randomOffset = Random.Range(0f, 1f);
        unitAnimator.Play("Idle", 0, randomOffset);
        unitAnimator.SetBool("Idle", true);

        unit.OnHealthChanged += Unit_OnHealthChanged;
        unit.OnUnitReset += Unit_OnUnitReset;
        unitAI.OnStateChanged += UnitAI_OnStateChanged;
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        unitAttack.OnUnitAttack += UnitAttack_OnUnitAttacked;
    }


    protected virtual void Update() {
        if (dead) {
            if (unitAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > .95) {
                // Unit animator has finished playing die animation
                unitAnimator.speed = 0;
            }
            return;
        }

        if (unitAIStateHasChanged) {
            if(unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") | unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk")) {
                UpdateAnimatorParameters();
            }
            if (unitAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > .95 && unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
                // Unit animator has finished playing Attack animation
                UpdateAnimatorParameters();
            }
        }

        if (walking) {
            Vector2 moveDir = unitMovement.GetMoveDir2D();
            SetXY(moveDir.x, moveDir.y);
        }
    }

    private void Unit_OnUnitReset(object sender, System.EventArgs e) {
        dead = false;
        unitAnimator.speed = 1;
        ResetAnimatorParameters();
    }

    private void Unit_OnUnitPlaced(object sender, System.EventArgs e) {
        SetUnitWatchDirection();
    }

    private void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        if (unitAI.IsDead()) {
            UpdateAnimatorParameters();
            return;
        }

        if (unitAI.IsIdle()) {
            SetUnitWatchDirection();
        }

        // Using this bool to let attack&special animations finish (see update)
        unitAIStateHasChanged = true;
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

        if(yValue <= 0) {
            // Always set y to -1 when <= 0
            Y = -1;
        } else {
            Y = 1;
        }

        if (xValue <= 0) {
            // Always set y to -1 when <= 0
            X = -1;
        }
        else {
            X = 1;
        }

        unitAnimator.SetFloat("X", X);
        unitAnimator.SetFloat("Y", Y);
    }

    private void UnitAttack_OnUnitAttacked(object sender, System.EventArgs e) {
        unitAnimator.SetTrigger("BaseAttack");
    }

    private void Unit_OnHealthChanged(object sender, Unit.OnHealthChangedEventArgs e) {
        if(e.newHealth < e.previousHealth) {
            unitShaderAnimator.SetTrigger("Damaged");
        }
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

        if (unitAI.IsWalking() | unitAI.IsMovingToTarget()) {
            walking = true;
            idle = false;

            animationName = "Walk";
            float randomOffset = Random.Range(0f, 1f);
            unitAnimator.Play(animationName, 0, randomOffset);
        }
        if(unitAI.IsIdle()) {
            walking = false;
            idle = true;

            animationName = "Idle";
            float randomOffset = Random.Range(0f, 1f);
            unitAnimator.Play(animationName, 0, randomOffset);
        }

        if(unitAI.IsDead()) {
            unitAnimator.SetTrigger("Die");
            dead = true;
            walking = false;
            idle = false;
        }

        if (unitAI.IsAttacking()) {
            walking = false;
            idle = true;
        }

        unitAnimator.SetBool("Walking", walking);
        unitAnimator.SetBool("Idle", idle);

        unitAIStateHasChanged = false;
    }

    private void ResetAnimatorParameters() {
        walking = false;
        idle = true;
        float randomOffset = Random.Range(0f, 1f);
        unitAnimator.Play("Idle", 0, randomOffset);

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
