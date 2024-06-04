using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionSlotTemplateUI : ItemTemplateVisualUI
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
            DeckManager.Instance.RemoveTroopFromDeckSelected(troopSO);
        } else {

            if(DeckManager.Instance.GetDeckSelected().troopsInDeck.Count < 4) {
                DeckManager.Instance.AddTroopToDeckSelected(troopSO);
            } else {
                // There are no more slots available in deck
                return;
            }
        }
    }

    public void TryAddOrRemoveBuildingToDeck() {
        if (selected) {
            DeckManager.Instance.RemoveBuildingFromDeckSelected(buildingSO);
        }

        else {
            if (DeckManager.Instance.GetDeckSelected().buildingsInDeck.Count < 1) {
                DeckManager.Instance.AddBuildingToDeckSelected(buildingSO);
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
