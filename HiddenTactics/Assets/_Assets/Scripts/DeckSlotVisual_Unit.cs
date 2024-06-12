using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSlotVisual_Unit : MonoBehaviour
{
    [SerializeField] private Animator unitAnimator;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private List<SpriteRenderer> shadowSpriteRendererList;
    [SerializeField] private List<SpriteRenderer> unitSpriteRendererList;

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

    public void SetUnitSpriteSortingOrder(int layerOrder)
    {
        Debug.Log(layerOrder);
        foreach (SpriteRenderer spriteRenderer in unitSpriteRendererList)
        {
            spriteRenderer.sortingOrder = layerOrder + 2;
        }

        foreach (SpriteRenderer spriteRenderer in shadowSpriteRendererList)
        {
            spriteRenderer.sortingOrder = layerOrder + 1;
        }

        weaponSpriteRenderer.sortingOrder = layerOrder + 3;
    }

}
