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
    public static ItemTemplateUI lastHoveredItemTemplateUI;

    private void Start() {
        GameInput.Instance.OnLeftClickPerformed += GameInput_OnLeftClickPerformed;
        GameInput.Instance.OnRightClickPerformed += GameInput_OnRightClickPerformed;
    }

    public virtual void SetDeckVisuals(Deck deck) {
        outlineImage.sprite = deck.deckFactionSO.slotBorder;
        outlineShadowImage.sprite = deck.deckFactionSO.slotBorder;
        backgroundImage.sprite = deck.deckFactionSO.slotBackgroundSquare;
    }

    public virtual void SetTroopSO(TroopSO troopSO) {
        this.troopSO = troopSO;

        if (troopSO.troopIllustrationSlotSprite != null  && troopSO.troopIsImplemented) {
            illustrationImage.sprite = troopSO.troopIllustrationSlotSprite;
        }
        else {
            illustrationImage.sprite = troopSO.troopTypeIconSprite;

            Color semitransparent = Color.white;
            semitransparent.a = .7f;
            illustrationImage.color = semitransparent;

            comingSoonText.SetActive(true);
        }

        if (troopSO.troopIllustrationSlotSprite == null && troopSO.troopIsImplemented)
        {
            illustrationImage.sprite = null;
        }

    }

    public virtual void SetBuildingSO(BuildingSO buildingSO) {
        this.buildingSO = buildingSO;

        if (buildingSO.buildingRecruitmentSlotSprite != null && buildingSO.buildingIsImplemented) {
            illustrationImage.sprite = buildingSO.buildingRecruitmentSlotSprite;
        }
        else {
            illustrationImage.sprite = buildingSO.buildingTypeSprite;
            Color semitransparent = Color.white;
            semitransparent.a = .7f;
            illustrationImage.sprite = buildingSO.buildingTypeSprite;
            illustrationImage.color = semitransparent;
            comingSoonText.SetActive(true);
        }

        if (buildingSO.buildingRecruitmentSlotSprite == null && buildingSO.buildingIsImplemented)
        {
            illustrationImage.sprite = null;
        }
    }

    private void GameInput_OnLeftClickPerformed(object sender, System.EventArgs e) {

        if (!pointerEntered && IPlaceableDescriptionSlotTemplate.Instance.GetCardOpen() && lastHoveredItemTemplateUI == this && !IPlaceableDescriptionSlotTemplate.Instance.GetPointerEntered()) {
            IPlaceableDescriptionSlotTemplate.Instance.Hide();
        }
    }

    private void GameInput_OnRightClickPerformed(object sender, System.EventArgs e) {
        if (pointerEntered) {
            if (troopSO != null && troopSO.troopIsImplemented) {
                lastHoveredItemTemplateUI = this;
                IPlaceableDescriptionSlotTemplate.Instance.Show();
                IPlaceableDescriptionSlotTemplate.Instance.SetDescriptionSlot(troopSO, troopSO.unitPrefab.GetComponent<Unit>().GetUnitSO());
                return;
            }

            if (buildingSO != null && buildingSO.buildingIsImplemented) {
                lastHoveredItemTemplateUI = this;
                IPlaceableDescriptionSlotTemplate.Instance.Show();
                IPlaceableDescriptionSlotTemplate.Instance.SetDescriptionSlot(buildingSO);
                return;
            }
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        pointerEntered = true;

        if (lastHoveredItemTemplateUI == this) return;

        if (troopSO != null && troopSO.troopIsImplemented) {
            lastHoveredItemTemplateUI = this;
        }

        if (buildingSO != null && buildingSO.buildingIsImplemented) {
            lastHoveredItemTemplateUI = this;
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        pointerEntered = false;
    }

    public void OnDestroy() {
        GameInput.Instance.OnLeftClickPerformed -= GameInput_OnLeftClickPerformed;
        GameInput.Instance.OnRightClickPerformed -= GameInput_OnRightClickPerformed;
    }
}
