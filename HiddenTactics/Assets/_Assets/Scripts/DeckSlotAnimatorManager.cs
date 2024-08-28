using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSlotAnimatorManager : MonoBehaviour
{


    private Animator deckSlotVisualAnimator;
    private DeckSlot deckSlot;

    private bool deckSlotHovered;
    private bool deckSlotSelected;
    private bool animatorActive = true;

    private void Awake()
    {
        deckSlot = GetComponent<DeckSlot>();
        deckSlotVisualAnimator = GetComponent<Animator>();
    }
    private void Start()
    {
        // Randomize Idle animation starting frame
        float randomOffset = Random.Range(0f, 1f);
        deckSlotVisualAnimator.Play("Idle", 0, randomOffset);
        deckSlotVisualAnimator.SetBool("Idle", true);
    }

    public void SetDeckSlotAnimationHovered()
    {
        if (deckSlotSelected) return;
        if (!animatorActive) return;

        deckSlotVisualAnimator.SetBool("Idle", false);
        deckSlotVisualAnimator.SetTrigger("Hovered");
        deckSlotHovered = true;
    }

    public void SetDeckSlotAnimationUnhovered()
    {
        if (deckSlotSelected) return;
        if (!animatorActive) return;
        if (!deckSlotHovered) return;

        deckSlotVisualAnimator.SetTrigger("Unhovered");
        deckSlotVisualAnimator.SetBool("Idle", true);
        deckSlotHovered = false;
    }

    public void SetDeckSlotAnimationUnhoveredAbsolute() {
        deckSlotVisualAnimator.SetTrigger("Unhovered");
        deckSlotVisualAnimator.SetBool("Idle", true);
        deckSlotHovered = false;
    }

    public void SetSelectingTroop(bool selectingTroop)
    {
        this.deckSlotSelected = selectingTroop;
    }

    public void SetAnimatorActive(bool active) {
        animatorActive = active;
    }

    public void SetIdle(bool idle) {
        deckSlotVisualAnimator.SetBool("Idle", idle);
    }

    public void TriggerFlyUp() {
        deckSlotVisualAnimator.SetTrigger("FlyUp");
        deckSlotVisualAnimator.SetBool("Idle", false);
    }

    public void TriggerFlyDown() {
        deckSlotVisualAnimator.SetTrigger("FlyDown");
        deckSlotVisualAnimator.SetBool("Idle", true);
    }
}
