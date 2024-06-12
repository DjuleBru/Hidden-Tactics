using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionSlotTemplateUI : ItemTemplateUI
{

    [SerializeField] protected Color unSelectedColor;
    private Button selectButton;

    private bool selected;

    private void Awake() {
        selectButton = GetComponent<Button>();

        selectButton.onClick.AddListener(() => {
            if(troopSO != null) {
                TryAddOrRemoveTroopToDeck();
            };
            if(buildingSO != null) {;
                TryAddOrRemoveBuildingToDeck();
            }
        });
    }

    public void TryAddOrRemoveTroopToDeck() {
        if (!troopSO.troopIsImplemented) return;
        if (selected) {
            DeckManager.LocalInstance.RemoveTroopFromDeckSelected(troopSO, DeckEditUI.Instance.GetDeckSlotSelectedIndex());
        } else {

            if(DeckManager.LocalInstance.GetEmptyDeckSlots() > 0) {
                DeckManager.LocalInstance.AddTroopToDeckSelected(troopSO, DeckEditUI.Instance.GetDeckSlotSelectedIndex());
            } else {
                // There are no more slots available in deck
                return;
            }
        }
    }

    public void TryAddOrRemoveBuildingToDeck()
    {
        if (!buildingSO.buildingIsImplemented) return;

        if (selected) {
            DeckManager.LocalInstance.RemoveBuildingFromDeckSelected(buildingSO, DeckEditUI.Instance.GetDeckSlotSelectedIndex());
        }

        else {
            if (DeckManager.LocalInstance.GetEmptyDeckSlots() > 0) {
                DeckManager.LocalInstance.AddBuildingToDeckSelected(buildingSO, DeckEditUI.Instance.GetDeckSlotSelectedIndex());
            }
            else {
                // There are no more slots available in deck
                return;
            }
        }
    }

    public void SetSelected(bool selected) {
        this.selected = selected;
        if (selected) {
            outlineImage.color = Color.white;
        }
        else {
            outlineImage.color = unSelectedColor;
        }
    }

}
