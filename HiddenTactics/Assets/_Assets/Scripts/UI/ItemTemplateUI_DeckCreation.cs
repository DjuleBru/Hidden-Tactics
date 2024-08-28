using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemTemplateUI_DeckCreation : ItemTemplateUI
{

    [SerializeField] protected Color unSelectedColor;
    [SerializeField] protected Material selectedMaterial;
    [SerializeField] protected Material cleanMaterial;
    [SerializeField] protected Material hoveredMaterial;
    private Button selectButton;

    private bool selected;
    private bool interactable;

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

            if (DeckEditUI.Instance.GetDeckSlotSelected() == null) {
                // Player has not selected a deck slot
                int troopIndex = DeckManager.LocalInstance.GetTroopSOIndex(troopSO);
                DeckManager.LocalInstance.RemoveTroopFromDeckSelected(troopSO, troopIndex);

            } else {
                // Player has selected a deck slot
                DeckManager.LocalInstance.RemoveTroopFromDeckSelected(troopSO, DeckEditUI.Instance.GetDeckSlotSelectedIndex());
            }

        } else {

            if(DeckManager.LocalInstance.GetEmptyDeckSlots() > 0) {
                // There are deck slots available

                if (DeckEditUI.Instance.GetDeckSlotSelected() == null) return;
                // Player has not selected a deck slot

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
            if (DeckEditUI.Instance.GetDeckSlotSelected() == null) {
                // Player has not selected a deck slot
                int troopIndex = DeckManager.LocalInstance.GetBuildingSOIndex(buildingSO);
                DeckManager.LocalInstance.RemoveBuildingFromDeckSelected(buildingSO, troopIndex);

            }
            else {
                // Player has selected a deck slot
                DeckManager.LocalInstance.RemoveBuildingFromDeckSelected(buildingSO, DeckEditUI.Instance.GetDeckSlotSelectedIndex());
            }
        }

        else {
            if (DeckManager.LocalInstance.GetEmptyDeckSlots() > 0) {
                // There are deck slots available

                if (DeckEditUI.Instance.GetDeckSlotSelected() == null) return;
                // Player has not selected a deck slot

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
            outlineImage.material = selectedMaterial;
            backgroundImage.material = selectedMaterial;

            illustrationImage.color = Color.white;
        }
        else {
            outlineImage.color = unSelectedColor;
            outlineImage.material = cleanMaterial;
            backgroundImage.material = cleanMaterial;

            illustrationImage.color = unSelectedColor;
        }

        if((buildingSO != null && !buildingSO.buildingIsImplemented) || troopSO != null && !troopSO.troopIsImplemented) {
            Color semitransparent = Color.white;
            semitransparent.a = .4f;
            illustrationImage.color = semitransparent;
        }
    }

    public void EnableButton(bool interactable) {
        selectButton.interactable = interactable;
        this.interactable = interactable;
    }

    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        if (!interactable) return;

        if(selected) {
            backgroundImage.material = selectedMaterial;
            outlineImage.material = selectedMaterial;
        } else {
            backgroundImage.material = cleanMaterial;
            outlineImage.material = cleanMaterial;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        if (!interactable) return;

        backgroundImage.material = hoveredMaterial;
        outlineImage.material = hoveredMaterial;
    }
}
