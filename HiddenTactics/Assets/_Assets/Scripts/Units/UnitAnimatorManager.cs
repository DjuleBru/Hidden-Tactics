using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UCW;

public class UnitAnimatorManager : NetworkBehaviour
{

    protected Animator unitAnimator;
    [SerializeField] protected List<Animator> unitShaderAnimatorList;
    [SerializeField] protected Animator unitGeneralAnimator;
    protected Unit unit;
    protected UnitAI unitAI;
    protected UnitMovement unitMovement;
    protected UnitAttack unitAttack;
    protected UnitHP unitHP;

    protected bool walking;
    protected bool idle;
    protected bool dead;
    protected bool attacking;
    protected bool movingForwards;
    protected bool unitAIStateHasChanged;

    protected float X;
    protected float Y;
    private bool firstSelectionDone;

    protected virtual void Awake() {
        unit = GetComponentInParent<Unit>();
        unitHP = GetComponentInParent<UnitHP>();
        unitAI = GetComponentInParent<UnitAI>();
        unitMovement = GetComponentInParent<UnitMovement>();
        unitAnimator = GetComponent<Animator>();
        unitAttack = GetComponentInParent<UnitAttack>();


        // Randomize Idle animation starting frame (previously OnNetworkSpawn)
        float randomOffset = Random.Range(0f, 1f);
        unitAnimator.Play("Idle", 0, randomOffset);
        unitAnimator.SetBool("Idle", true);
    }

    public override void OnNetworkSpawn() {
        unitHP.OnHealthChanged += Unit_OnHealthChanged;
        unit.OnUnitReset += Unit_OnUnitReset;
        unitAI.OnStateChanged += UnitAI_OnStateChanged;
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        unit.OnUnitSelected += Unit_OnUnitSelected;

        unit.OnAdditionalUnitActivated += Unit_OnAdditionalUnitActivated;

        unitAttack.OnUnitAttack += UnitAttack_OnUnitAttack;
        unitAttack.OnUnitAttackStarted += UnitAttack_OnUnitAttackStarted;
        unitAttack.OnUnitAttackEnded += UnitAttack_OnUnitAttackEnded;
    }


    protected void Start() {
        if (unit.GetUnitIsOnlyVisual()) return;
        SetUnitWatchDirectionBasedOnPlayerOwnance();
    }

    protected virtual void Update() {
        if (unitAIStateHasChanged) {
            if(unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") | unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk") | unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack_Start")) {
                UpdateAnimatorParameters();
            }

            if (unitAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > .95 && unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
                // Unit animator has finished playing Attack animation
                UpdateAnimatorParameters();
            }
            return;
        }

        if (movingForwards) {
            Vector2 moveDir = unitMovement.GetMoveDir2D();
            SetXY(moveDir.x, -1);
            return;
        }

        if (walking) {
            Vector2 moveDir = unitMovement.GetMoveDir2D();
            SetXY(moveDir.x, moveDir.y);
        }

        if(attacking) {
            Vector2 watchDir = unitMovement.GetWatchDir2D();
            SetXY(watchDir.x, watchDir.y);
        }
        
    }

    protected void Unit_OnUnitReset(object sender, System.EventArgs e) {
        if (!unit.GetUnitIsBought()) return;

        dead = false;
        unitAnimator.speed = 1;
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        ResetAnimatorParameters();
    }

    protected void Unit_OnUnitPlaced(object sender, System.EventArgs e) {
        if (!unit.GetUnitIsBought()) return;
        unitGeneralAnimator.SetTrigger("UnitPlaced");
    }

    private void Unit_OnUnitSelected(object sender, System.EventArgs e) {
        if (!unit.GetUnitIsBought()) return;
        if (!firstSelectionDone) {
            firstSelectionDone = true;
            return;
        }
        unitGeneralAnimator.SetTrigger("UnitSelected");
    }

    protected virtual void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        if (unitAI.IsDead() | unitAI.IsFallen()) {
            UpdateAnimatorParameters();
            if(unitAI.IsFallen()) {
                SetUnitWatchDirectionBasedOnPlayerOwnance();
            }
            movingForwards = false;
            return;
        }

        if(unitAI.GetState() == UnitAI.State.jumping) {
            unitAnimator.SetTrigger("Jump");
        }

        if (unitAI.IsIdle()) {
            SetUnitWatchDirectionBasedOnPlayerOwnance();
        }

        if(unitAI.IsMovingForwards()) {
            movingForwards = true;
            SetUnitWatchDirectionBasedOnPlayerOwnance();
        } else {
            movingForwards = false;
        }

        // Using this bool to let attack&special animations finish (see update)
        unitAIStateHasChanged = true;
    }

    public void SetUnitWatchDirectionBasedOnPlayerOwnance() {
        if(!unit.IsOwnedByPlayer()) {
            SetXY(-1,-1);
        } else {
            SetXY(1,-1);
        };
    }

    public void SetXY(float xValue, float yValue) {
        X = xValue;
        Y = yValue;
        unitAnimator.SetFloat("X", X);
        unitAnimator.SetFloat("Y", Y);
    }

    protected virtual void UnitAttack_OnUnitAttack(object sender, System.EventArgs e) {

        if(unitAttack.GetActiveAttackSO().attackType == AttackSO.AttackType.ranged) {
            unitAnimator.SetTrigger("RangedAttack");
        }

        if(unitAttack.GetActiveAttackSO().attackType == AttackSO.AttackType.melee) {
            unitAnimator.SetTrigger("BaseAttack");
        }

        if (unitAttack.GetActiveAttackSO().attackType == AttackSO.AttackType.special1Melee) {
            unitAnimator.SetTrigger("Special1");
        }

        if (unitAttack.GetActiveAttackSO().attackType == AttackSO.AttackType.healAllyMeleeTargeting || unitAttack.GetActiveAttackSO().attackType == AttackSO.AttackType.healAllyRangedTargeting) {
            unitAnimator.SetTrigger("BaseAttack");
        }
    }

    protected virtual void UnitAttack_OnUnitAttackEnded(object sender, System.EventArgs e) {
    }

    protected virtual void UnitAttack_OnUnitAttackStarted(object sender, System.EventArgs e) {
    }

    protected virtual void Unit_OnHealthChanged(object sender, UnitHP.OnHealthChangedEventArgs e) {
        if(e.newHealth < e.previousHealth) {
            foreach(Animator unitShaderAnimator in unitShaderAnimatorList) {
                unitShaderAnimator.SetTrigger("Damaged");
            }
        } else {
            foreach (Animator unitShaderAnimator in unitShaderAnimatorList) {
                unitShaderAnimator.SetTrigger("Healed");
            }
        }
    }

    protected virtual void UpdateAnimatorParameters() {
        string animationName = "Walk";

        if (unitAI.IsWalking() | unitAI.IsMovingToTarget()) {
            walking = true;
            idle = false;
            attacking = false;

            animationName = "Walk";
            float randomOffset = Random.Range(0f, 1f);
            unitAnimator.Play(animationName, 0, randomOffset);
        }

        if (unitAI.IsIdle() | unitAI.IsBlockedByBuilding() | unitAI.IsWaiting()) {
            walking = false;
            idle = true;
            attacking = false;

            animationName = "Idle";
            float randomOffset = Random.Range(0f, 1f);
            unitAnimator.Play(animationName, 0, randomOffset);
        }

        if (unitAI.IsDead()) {
            unitAnimator.SetTrigger("Die");
            dead = true;
            walking = false;
            idle = false;
            attacking = false;
        }

        if (unitAI.IsAttacking()) {
            walking = false;
            idle = true;
            attacking = true;
        }

        if (unitAI.IsFallen()) {
            unitAnimator.SetTrigger("Fall");
            dead = true;
            walking = false;
            idle = false;
            attacking = false;
        }

        unitAnimator.SetBool("Walking", walking);
        unitAnimator.SetBool("Idle", idle);

        unitAIStateHasChanged = false;
    }

    protected void ResetAnimatorParameters() {
        walking = false;
        idle = true;
        float randomOffset = Random.Range(0f, 1f);
        unitAnimator.Play("Idle", 0, randomOffset);

        unitAnimator.SetBool("Walking", walking);
        unitAnimator.SetBool("Idle", idle);
        unitAnimator.ResetTrigger("Die");
        unitAnimator.ResetTrigger("Fall");
        unitAnimator.ResetTrigger("Attack_Start");
        unitAnimator.ResetTrigger("Attack_End");
        unitAnimator.ResetTrigger("BaseAttack");
        unitAnimator.ResetTrigger("SideWeaponAttack");

        SetUnitWatchDirectionBasedOnPlayerOwnance();
    }

    public virtual void SetJumpTrigger() {
        unitAnimator.SetTrigger("Jump");
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

    public virtual void SetAttackTrigger()
    {
        unitAnimator.SetTrigger("BaseAttack");
    }

    public virtual void SetSideAttackTrigger()
    {
        unitAnimator.SetTrigger("SideAttack");
    }

    public virtual void SetSpecial1Trigger()
    {
        unitAnimator.SetTrigger("Special1");
    }

    public virtual void SetSpecial2Trigger()
    {
        unitAnimator.SetTrigger("Special2");
    }

    public float GetX() {
        return X;
    }
    public float GetY() {
        return Y;
    }

    private void Unit_OnAdditionalUnitActivated(object sender, System.EventArgs e) {
        if (!BattleManager.Instance.IsPreparationPhase()) return;
        unitGeneralAnimator.SetTrigger("UnitPlaced");
    }

}
