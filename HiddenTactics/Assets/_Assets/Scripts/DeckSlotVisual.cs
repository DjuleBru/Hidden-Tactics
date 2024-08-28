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

    [SerializeField] private GameObject troopTypeSlot;
    [SerializeField] private GameObject buildingTypeSlot;
    [SerializeField] private GameObject heroTypeSlot;
    [SerializeField] private GameObject troopOrBuildingSlot;
    [SerializeField] private GameObject troopOrSpellSlot;
    [SerializeField] private GameObject buildingOrSpellSlot;

    private DeckSlot deckSlot;
    private bool deckSlotSelected;
    private bool deckSlotSelectionEnabled = true;

    private void Awake()
    {
        deckSlot = GetComponentInParent<DeckSlot>();
        deckSlotAnimatorManager = GetComponentInParent<DeckSlotAnimatorManager>();
        visualCollider = GetComponent<Collider2D>();
        deckSlotVisualSpriteRenderer = GetComponent<SpriteRenderer>();

        EnableSlotTypeUI(false);
    }

    private void Start()
    {
        DeckManager.LocalInstance.OnSelectedDeckChanged += DeckManager_OnSelectedDeckChanged;
        DeckEditUI.Instance.OnDeckEditMenuClosed += DeckEditUI_OnDeckEditMenuClosed;
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
        if (!deckSlotSelectionEnabled) return;
        deckSlotAnimatorManager.SetDeckSlotAnimationHovered();
        deckSlotHovered = true;
        deckSlotVisualSpriteRenderer.material = hoveredMaterial;
        GetComponentInChildren<DeckSlotUI>().SetAddTroopTextHovered();
    }

    public void SetDeckSlotUnhovered()
    {
        if (deckSlotSelected) return;
        if (!deckSlotSelectionEnabled) return;

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

    private void DeckEditUI_OnDeckEditMenuClosed(object sender, EventArgs e)
    {
        deckSlot.SetSelecting(false);
        deckSlotVisualSpriteRenderer.material = cleanMaterial;
    }

    public SpriteRenderer GetSlotVisualSpriteRenderer()
    {
        return deckSlotVisualSpriteRenderer;
    }

    public void EnableDeckSlotHover() {
        deckSlotSelectionEnabled = true;
    }

    public void DisableDeckSlotHover() {
        deckSlotSelectionEnabled = false;
    }

    private void SetSlotTypeVisuals() {

        if (deckSlot.GetCanHostBuilding() && deckSlot.GetCanHostTroop()) {
            troopOrBuildingSlot.SetActive(true);
            return;
        }
        if (deckSlot.GetCanHostSpell() && deckSlot.GetCanHostTroop()) {
            troopOrSpellSlot.SetActive(true);
            return;
        }
        if (deckSlot.GetCanHostSpell() && deckSlot.GetCanHostBuilding()) {
            buildingOrSpellSlot.SetActive(true);
            return;
        }
        if (deckSlot.GetCanHostTroop()) {
            troopTypeSlot.gameObject.SetActive(true);
            return;
        }
        if (deckSlot.GetCanHostBuilding()) {
            buildingTypeSlot.gameObject.SetActive(true);
            return;
        }
        if (deckSlot.GetCanHostHero()) {
            heroTypeSlot.gameObject.SetActive(true);
            return;
        }

    }

    public void EnableSlotTypeUI(bool enable) {
        if (enable) {
            SetSlotTypeVisuals();
        }
        else {
            troopTypeSlot.SetActive(false);
            buildingTypeSlot.SetActive(false);
            heroTypeSlot.SetActive(false);
            troopOrSpellSlot.SetActive(false);
            buildingOrSpellSlot.SetActive(false);
            troopOrBuildingSlot.SetActive(false);
        }
    }
}
