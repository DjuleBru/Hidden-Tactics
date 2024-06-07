using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class DeckSlotVisual_Unit : MonoBehaviour
{
    [SerializeField] private Animator unitAnimator;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;

    public void SetUnitAnimator(RuntimeAnimatorController animatorController) {
        unitAnimator.runtimeAnimatorController = animatorController;

        float randomOffset = Random.Range(0f, 1f);
        unitAnimator.Play("Idle", 0, randomOffset);
        unitAnimator.SetBool("Idle", true);
    }

    public void SetWeaponSprite(Sprite sprite) {
        weaponSpriteRenderer.sprite = sprite;
    }

    public void SetUnitAnimatorXY(float X, float Y) {
        unitAnimator.SetFloat("X", X);
        unitAnimator.SetFloat("Y", Y);
    }

}
