using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UCWAnimatorManager_BoarRider : UCWAnimatorManager
{
    protected override void Update() {
        if (unitAIStateHasChanged) {
            if (unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") | unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk") | unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack_Start")) {
                UpdateAnimatorParameters();
            }
            if (unitAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > .95 && unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
                // Unit animator has finished playing Attack animation
                UpdateAnimatorParameters();
            }
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

        if (attacking) {
            Vector2 watchDir = unitMovement.GetMoveDir2D();
            SetXY(watchDir.x, watchDir.y);
        }

    }

    protected override void UpdateAnimatorParameters() {
        string animationName = "Walk";

        if (unitAI.IsWalking() | unitAI.IsMovingToTarget()) {
            walking = true;
            idle = false;
            attacking = false;

            animationName = "Walk";
            float randomOffset = Random.Range(0f, 1f);
            unitAnimator.Play(animationName, 0, randomOffset);
        }

        if (unitAI.IsIdle() | unitAI.IsBlockedByBuilding()) {
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
            walking = true;
            idle = false;
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
}
