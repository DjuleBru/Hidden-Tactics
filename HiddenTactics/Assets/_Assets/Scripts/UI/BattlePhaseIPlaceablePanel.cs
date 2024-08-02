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

        Deck playerDeck = DeckManager.LocalInstance.GetDeckSelected();
        SetPanelVisuals(playerDeck);
    }
    private void SetPanelVisuals(Deck playerDeck) {
        backgroundImage.sprite = playerDeck.deckFactionSO.panelBackground;
        borderShadow.sprite = playerDeck.deckFactionSO.panelBackgroundBorderSimple;
        border.sprite = playerDeck.deckFactionSO.panelBackgroundBorderSimple;

        cardBackgroundImage.sprite = playerDeck.deckFactionSO.panelBackground;
        cardOutlineImage.sprite = playerDeck.deckFactionSO.panelBackgroundBorderSimple;
    }

    private void Troop_OnAnyTroopSelled(object sender, System.EventArgs e) {
        Troop troop = (Troop)sender;

        if (troop.IsOwnedByPlayer()) {
            playerIPlaceableList.Remove(troop);
            RemoveIPlaceableCard(troop);
        }
    }

    private void Troop_OnAnyTroopPlaced(object sender, System.EventArgs e) {
        Troop troop = (Troop)sender;

        if (troop.IsOwnedByPlayer() && !playerIPlaceableList.Contains(troop)) {
            playerIPlaceableList.Add(troop);
            AddIPlaceableCard(troop);
        }
    }

    private void AddIPlaceableCard(IPlaceable iPlaceableAdded) {
        iPlaceableCardTemplate.gameObject.SetActive(true);

        Transform template = Instantiate(iPlaceableCardTemplate, iPlaceableCardContainer);
        BattlePhaseIPlaceableSlotTemplateUI iPlaceableSlot = template.GetComponent<BattlePhaseIPlaceableSlotTemplateUI>();

        iPlaceableSlot.SetIPlaceable(iPlaceableAdded);
        iPlaceableSlotTemplateUIList.Add(iPlaceableSlot);

        RefreshContainerSpacing();

        iPlaceableCardTemplate.gameObject.SetActive(false);
    }

    private void RemoveIPlaceableCard(IPlaceable iPlaceableRemoved) {
        foreach (BattlePhaseIPlaceableSlotTemplateUI slot in iPlaceableSlotTemplateUIList) {

            if (slot.GetIPlaceable() == iPlaceableRemoved) {
                iPlaceableSlotTemplateUIList.Remove(slot);
                Destroy(slot.gameObject);
            }

        }
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
