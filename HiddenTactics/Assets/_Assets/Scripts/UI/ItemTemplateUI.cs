using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemTemplateUI : MonoBehaviour
{
    [SerializeField] protected Image illustrationImage;
    [SerializeField] protected Image outlineImage;
    [SerializeField] protected Image outlineShadowImage;
    [SerializeField] protected Image backgroundImage;
    [SerializeField] protected GameObject comingSoonText;

    protected TroopSO troopSO;
    protected BuildingSO buildingSO;

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

}
