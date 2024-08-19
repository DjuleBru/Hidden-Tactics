using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemTemplateUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] protected Image illustrationImage;
    [SerializeField] protected Image outlineImage;
    [SerializeField] protected Image outlineShadowImage;
    [SerializeField] protected Image backgroundImage;
    [SerializeField] protected GameObject comingSoonText;

    protected TroopSO troopSO;
    protected BuildingSO buildingSO;

    protected bool pointerEntered;
    public static bool cardOpen;
    public static ItemTemplateUI lastHoveredItemTemplateUI;

    private void Start() {
        GameInput.Instance.OnLeftClickPerformed += GameInput_OnLeftClickPerformed;
        GameInput.Instance.OnRightClickPerformed += GameInput_OnRightClickPerformed;
    }


    public virtual void SetDeckVisuals(Deck deck) {
        outlineImage.sprite = deck.deckFactionSO.slotBorder;
        outlineShadowImage.sprite = deck.deckFactionSO.slotBorder;
        backgroundImage.sprite = deck.deckFactionSO.slotBackground;
    }

    public virtual void SetTroopSO(TroopSO troopSO) {
        this.troopSO = troopSO;
        illustrationImage.gameObject.SetActive(true);

        if (troopSO.troopIllustrationSlotSprite != null  && troopSO.troopIsImplemented) {
            illustrationImage.sprite = troopSO.troopIllustrationSlotSprite;
        }
        else {
            illustrationImage.gameObject.SetActive(false);
            comingSoonText.SetActive(true);
        }

        if (troopSO.troopIllustrationSlotSprite == null && troopSO.troopIsImplemented)
        {
            illustrationImage.sprite = null;
        }

    }

    public virtual void SetBuildingSO(BuildingSO buildingSO) {
        this.buildingSO = buildingSO;
        illustrationImage.gameObject.SetActive(true);

        if (buildingSO.buildingIllustrationSlotSprite != null && buildingSO.buildingIsImplemented) {
            illustrationImage.sprite = buildingSO.buildingIllustrationSlotSprite;
        }
        else {
            illustrationImage.gameObject.SetActive(false);
            comingSoonText.SetActive(true);
        }

        if (buildingSO.buildingIllustrationSlotSprite == null && buildingSO.buildingIsImplemented)
        {
            illustrationImage.sprite = null;
        }
    }


    private void GameInput_OnLeftClickPerformed(object sender, System.EventArgs e) {
        if (!pointerEntered && cardOpen && lastHoveredItemTemplateUI == this) {
            cardOpen = false;
            IPlaceableDescriptionSlotTemplate.Instance.Hide();
        }
    }

    private void GameInput_OnRightClickPerformed(object sender, System.EventArgs e) {
        if (pointerEntered) {
            if (troopSO != null) {
                cardOpen = true;
                IPlaceableDescriptionSlotTemplate.Instance.Show();
                IPlaceableDescriptionSlotTemplate.Instance.SetDescriptionSlot(troopSO, troopSO.unitPrefab.GetComponent<Unit>().GetUnitSO());
            }
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        pointerEntered = true;
        lastHoveredItemTemplateUI = this;

        if (cardOpen) {
            if (troopSO != null) {
                IPlaceableDescriptionSlotTemplate.Instance.SetDescriptionSlot(troopSO, troopSO.unitPrefab.GetComponent<Unit>().GetUnitSO());
            }
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        pointerEntered = false;
    }
}
