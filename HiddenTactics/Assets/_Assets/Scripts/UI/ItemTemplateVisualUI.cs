using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemTemplateVisualUI : MonoBehaviour
{
    [SerializeField] protected Image illustrationImage;
    [SerializeField] protected Image outlineImage;

    protected TroopSO troopSO;
    protected BuildingSO buildingSO;

    public void SetTroopSO(TroopSO troopSO) {
        this.troopSO = troopSO;

        if(troopSO.troopIllustrationSlotSprite != null ) {
            illustrationImage.sprite = troopSO.troopIllustrationSlotSprite;
        } else {
            illustrationImage.sprite = null;
        }
    }

    public void SetBuildingSO(BuildingSO buildingSO) {
        this.buildingSO = buildingSO;

        if (buildingSO.buildingIllustrationSlotSprite != null) {
            illustrationImage.sprite = buildingSO.buildingIllustrationSlotSprite;
        }
        else {
            illustrationImage.sprite = null;
        }
    }

}
