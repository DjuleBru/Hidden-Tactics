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
        if (selected) {
            DeckManager.LocalInstance.RemoveTroopFromDeckSelected(troopSO);
        } else {

            if(DeckManager.LocalInstance.GetDeckSelected().troopsInDeck.Count < 4) {
                DeckManager.LocalInstance.AddTroopToDeckSelected(troopSO);
            } else {
                // There are no more slots available in deck
                return;
            }
        }
    }

    public void TryAddOrRemoveBuildingToDeck() {
        if (selected) {
            DeckManager.LocalInstance.RemoveBuildingFromDeckSelected(buildingSO);
        }

        else {
            if (DeckManager.LocalInstance.GetDeckSelected().buildingsInDeck.Count < 1) {
                DeckManager.LocalInstance.AddBuildingToDeckSelected(buildingSO);
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
