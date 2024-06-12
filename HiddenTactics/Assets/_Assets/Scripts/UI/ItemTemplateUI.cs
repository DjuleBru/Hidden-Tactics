using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemTemplateUI : MonoBehaviour
{
    [SerializeField] protected Image illustrationImage;
    [SerializeField] protected Image outlineImage;
    [SerializeField] protected Sprite comingSoonSprite;

    protected TroopSO troopSO;
    protected BuildingSO buildingSO;

    public void SetTroopSO(TroopSO troopSO) {
        this.troopSO = troopSO;

        if(troopSO.troopIllustrationSlotSprite != null  && troopSO.troopIsImplemented) {
            illustrationImage.sprite = troopSO.troopIllustrationSlotSprite;
        }
        else
        {
            illustrationImage.sprite = comingSoonSprite;
        }

        if (troopSO.troopIllustrationSlotSprite == null && troopSO.troopIsImplemented)
        {
            illustrationImage.sprite = null;
        }

    }

    public void SetBuildingSO(BuildingSO buildingSO) {
        this.buildingSO = buildingSO;

        if (buildingSO.buildingIllustrationSlotSprite != null && buildingSO.buildingIsImplemented) {
            illustrationImage.sprite = buildingSO.buildingIllustrationSlotSprite;
        }
        else {
            illustrationImage.sprite = comingSoonSprite;
        }

        if (buildingSO.buildingIllustrationSlotSprite == null && buildingSO.buildingIsImplemented)
        {
            illustrationImage.sprite = null;
        }
    }

}
