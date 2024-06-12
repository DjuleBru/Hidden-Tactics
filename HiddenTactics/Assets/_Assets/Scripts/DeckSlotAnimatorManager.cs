using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSlotAnimatorManager : MonoBehaviour
{


    private Animator deckSlotVisualAnimator;
    private DeckSlot deckSlot;

    private bool deckSlotHovered;
    private bool deckSlotSelected;

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
        deckSlotVisualAnimator.SetBool("Idle", false);
        deckSlotVisualAnimator.SetTrigger("Hovered");
        deckSlotHovered = true;
    }

    public void SetDeckSlotAnimationUnhovered()
    {
        if (deckSlotSelected) return;

        if (!deckSlotHovered) return;
        deckSlotVisualAnimator.SetTrigger("Unhovered");
        deckSlotVisualAnimator.SetBool("Idle", true);
        deckSlotHovered = false;
    }
    public void SetSelectingTroop(bool selectingTroop)
    {
        this.deckSlotSelected = selectingTroop;
    }
}
