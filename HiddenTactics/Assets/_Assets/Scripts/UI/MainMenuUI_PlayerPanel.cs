using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI_PlayerPanel : MonoBehaviour
{

    [SerializeField] private Image playerIcon;
    [SerializeField] private Image playerIconBackground;
    [SerializeField] private Image playerIconOuline;

    [SerializeField] private TextMeshProUGUI playerNameText;

    [SerializeField] private Button customizePlayerButton;

    private void Awake() {
        customizePlayerButton.onClick.AddListener(() => {
            EditBattlefieldUI.Instance.SwitchToEditBattlefield();
        });
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;

        playerIcon.sprite = PlayerCustomizationDataManager.Instance.GetPlayerIconSpriteFromSpriteId(HiddenTacticsMultiplayer.Instance.GetPlayerIconSpriteId());
        RefreshPlayerIconVisual(DeckManager.LocalInstance.GetDeckSelected());

        string playerName = SavingManager.Instance.LoadPlayerName();
        playerNameText.text = playerName;
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshPlayerIconVisual(e.selectedDeck);
    }


    private void RefreshPlayerIconVisual(Deck deckSelected) {
        // Player Icon
        playerIconBackground.sprite = deckSelected.deckFactionSO.slotBackgroundSquare;
        playerIconOuline.sprite = deckSelected.deckFactionSO.slotBorder;
    }
}
