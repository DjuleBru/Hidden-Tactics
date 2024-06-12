using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSlotVisual : MonoBehaviour
{
    public static int deckSlotVisualNumberHovered;
    private bool deckSlotHovered;

    private SpriteRenderer deckSlotVisualSpriteRenderer;
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] private Material selectedMaterial;

    private Collider2D visualCollider;

    private DeckSlotAnimatorManager deckSlotAnimatorManager;

    private DeckSlot deckSlot;
    private bool deckSlotSelected;

    private void Awake()
    {
        deckSlot = GetComponentInParent<DeckSlot>();
        deckSlotAnimatorManager = GetComponentInParent<DeckSlotAnimatorManager>();
        visualCollider = GetComponent<Collider2D>();
        deckSlotVisualSpriteRenderer = GetComponent<SpriteRenderer>();  
    }

    private void Start()
    {
        DeckManager.LocalInstance.OnSelectedDeckChanged += DeckManager_OnSelectedDeckChanged;
        DeckVisualUI.Instance.OnDeckEditMenuClosed += Instance_OnDeckEditMenuClosed;
        RefreshDeckSlotSprite();
    }


    private void DeckManager_OnSelectedDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e)
    {
        RefreshDeckSlotSprite();
    }

    private void RefreshDeckSlotSprite()
    {
        FactionSO selectedDeckFactionSO = DeckManager.LocalInstance.GetDeckSelected().deckFactionSO;

        if (selectedDeckFactionSO.deckSlotSpritesInFaction[deckSlot.GetDeckSlotNumber()] != deckSlotVisualSpriteRenderer.sprite)
        {
            deckSlotVisualSpriteRenderer.sprite = selectedDeckFactionSO.deckSlotSpritesInFaction[deckSlot.GetDeckSlotNumber()];
        }
    }

    public void SetDeckSlotHovered()
    {
        if (deckSlotSelected) return;
        deckSlotAnimatorManager.SetDeckSlotAnimationHovered();
        deckSlotHovered = true;
        deckSlotVisualSpriteRenderer.material = hoveredMaterial;
        GetComponentInChildren<DeckSlotUI>().SetAddTroopTextHovered();
    }

    public void SetDeckSlotUnhovered()
    {
        if (deckSlotSelected) return;
        if (deckSlotHovered)
        {
            deckSlotAnimatorManager.SetDeckSlotAnimationUnhovered();
            deckSlotHovered = false;
            deckSlotVisualSpriteRenderer.material = cleanMaterial;
            GetComponentInChildren<DeckSlotUI>().SetAddTroopTextUnhovered();
        }
    }

    public void SetDeckSlotUnhoveredWithoutConditions()
    {
        deckSlotAnimatorManager.SetDeckSlotAnimationUnhovered();
        deckSlotHovered = false;
        deckSlotVisualSpriteRenderer.material = cleanMaterial;
    }

    public bool GetDeckSlotHovered()
    {
        return deckSlotHovered;
    }

    public void SetSelectingTroop(bool selectingTroop)
    {
        this.deckSlotSelected = selectingTroop;
        if(selectingTroop)
        {
            deckSlotVisualSpriteRenderer.material = selectedMaterial;
        }
    }

    private void Instance_OnDeckEditMenuClosed(object sender, EventArgs e)
    {
        deckSlot.SetSelecting(false);

        deckSlotVisualSpriteRenderer.material = cleanMaterial;
    }

    public SpriteRenderer GetSlotVisualSpriteRenderer()
    {
        return deckSlotVisualSpriteRenderer;
    }

}
