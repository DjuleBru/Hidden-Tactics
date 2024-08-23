using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePhaseIPlaceablePanel : MonoBehaviour
{

    public static BattlePhaseIPlaceablePanel Instance;

    private List<IPlaceable> playerIPlaceableList = new List<IPlaceable>();

    private List<BattlePhaseIPlaceableSlotTemplateUI> iPlaceableSlotTemplateUIList = new List<BattlePhaseIPlaceableSlotTemplateUI>();


    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image border;
    [SerializeField] private Image borderShadow;
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private Image cardOutlineImage;

    [SerializeField] private Transform iPlaceableCardContainer;
    [SerializeField] private Transform iPlaceableCardTemplate;
    [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;

    [SerializeField] private int maxCardsBeforeHavingToChangeSpacing;

    private void Awake() {
        Instance = this;
        iPlaceableCardTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        Troop.OnAnyTroopPlaced += Troop_OnAnyTroopPlaced;
        Troop.OnAnyTroopSelled += Troop_OnAnyTroopSelled;
        Building.OnAnyBuildingPlaced += Building_OnAnyBuildingPlaced;
        Building.OnAnyBuildingDestroyed += Building_OnAnyBuildingDestroyed;
        Building.OnAnyBuildingSelled += Building_OnAnyBuildingSelled;

        Deck playerDeck = DeckManager.LocalInstance.GetDeckSelected();
        SetPanelVisuals(playerDeck);
    }


    private void SetPanelVisuals(Deck playerDeck) {
        backgroundImage.sprite = playerDeck.deckFactionSO.panelBackground;
        borderShadow.sprite = playerDeck.deckFactionSO.panelBackgroundBorderSimple;
        border.sprite = playerDeck.deckFactionSO.panelBackgroundBorderSimple;

        cardBackgroundImage.sprite = playerDeck.deckFactionSO.slotBackground;
        cardOutlineImage.sprite = playerDeck.deckFactionSO.panelBackgroundBorderSimple;
    }

    private void Troop_OnAnyTroopSelled(object sender, System.EventArgs e) {
        Troop troop = (Troop)sender;

        if (troop.IsOwnedByPlayer()) {
            if (!playerIPlaceableList.Contains(troop)) return;
            playerIPlaceableList.Remove(troop);
            RemoveIPlaceableCard(troop);
        }
    }

    private void Troop_OnAnyTroopPlaced(object sender, System.EventArgs e) {
        Troop troop = (Troop)sender;

        if (troop.IsOwnedByPlayer() && !playerIPlaceableList.Contains(troop) && !troop.GetTroopSO().isGarrisonedTroop) {
            playerIPlaceableList.Add(troop);
            AddIPlaceableCard(troop);
        }
    }


    private void Building_OnAnyBuildingSelled(object sender, System.EventArgs e) {
        Building building = (Building)sender;

        //if (building.IsOwnedByPlayer()) {
        //    playerIPlaceableList.Remove(building);
        //    RemoveIPlaceableCard(building);
        //}
    }

    private void Building_OnAnyBuildingDestroyed(object sender, System.EventArgs e) {
        Building building = (Building)sender;

        if (building.IsOwnedByPlayer()) {
            playerIPlaceableList.Remove(building);
            RemoveIPlaceableCard(building);
        }
    }

    private void Building_OnAnyBuildingPlaced(object sender, System.EventArgs e) {
        Building building = (Building)sender;

        if (building.IsOwnedByPlayer() && !playerIPlaceableList.Contains(building)) {
            playerIPlaceableList.Add(building);
            AddIPlaceableCard(building);
        }
    }


    private void AddIPlaceableCard(IPlaceable iPlaceableAdded) {
        if (!(iPlaceableAdded is Troop)) return;

        iPlaceableCardTemplate.gameObject.SetActive(true);
        Transform template = Instantiate(iPlaceableCardTemplate, iPlaceableCardContainer);
        BattlePhaseIPlaceableSlotTemplateUI iPlaceableSlot = template.GetComponent<BattlePhaseIPlaceableSlotTemplateUI>();

        iPlaceableSlot.SetIPlaceable(iPlaceableAdded);
        iPlaceableSlotTemplateUIList.Add(iPlaceableSlot);

        RefreshContainerSpacing();

        iPlaceableCardTemplate.gameObject.SetActive(false);
    }

    private void RemoveIPlaceableCard(IPlaceable iPlaceableRemoved) {
        BattlePhaseIPlaceableSlotTemplateUI slotToRemove = null;
        foreach (BattlePhaseIPlaceableSlotTemplateUI slot in iPlaceableSlotTemplateUIList) {

            if (slot.GetIPlaceable() == iPlaceableRemoved) {
                slotToRemove = slot;
            }
        }
        iPlaceableSlotTemplateUIList.Remove(slotToRemove);
        Destroy(slotToRemove.gameObject);
    }

    private void RefreshContainerSpacing() {
        if (playerIPlaceableList.Count > maxCardsBeforeHavingToChangeSpacing) {
            float templateSizeToFillContainer = GetComponent<RectTransform>().rect.width / playerIPlaceableList.Count;
            float containerSizeWithLastTemplateOverflow = templateSizeToFillContainer * (playerIPlaceableList.Count - 1) + 200;
            float sizeToDivide = GetComponent<RectTransform>().rect.width - (containerSizeWithLastTemplateOverflow - GetComponent<RectTransform>().rect.width);

            horizontalLayoutGroup.spacing = sizeToDivide / playerIPlaceableList.Count;
        }
    }

    public void OpenIPlaceableCard(IPlaceable iPlaceable) {

        foreach(BattlePhaseIPlaceableSlotTemplateUI slot in iPlaceableSlotTemplateUIList) {
            if(slot.GetIPlaceable() == iPlaceable) {
                slot.OpenIPlaceableCard();
            }
        }
    } 

    public void CloseIPlaceableCard(IPlaceable iPlaceable) {
        foreach (BattlePhaseIPlaceableSlotTemplateUI slot in iPlaceableSlotTemplateUIList) {
            if (slot.GetIPlaceable() == iPlaceable) {
                slot.CloseIPlaceableCard();
            }
        }
    }

}
