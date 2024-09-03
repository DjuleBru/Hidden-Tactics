using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCustomizationUI : MonoBehaviour
{

    public static PlayerCustomizationUI Instance;

    [SerializeField] private TMP_InputField playerNameInputField;

    [SerializeField] private GameObject playerIconsPanel;
    [SerializeField] private Transform playerIconsContainer;
    [SerializeField] private Transform playerIconsTemplate;
    private List<PlayerIconSelectSingleUI> playerIcons = new List<PlayerIconSelectSingleUI>();

    private PlayerIconSO selectedPlayerIconSO;
    private bool playerIconsContainerActive;

    [SerializeField] private Image playerIcon;
    [SerializeField] private Image playerIconBackground;
    [SerializeField] private Image playerIconOuline;
    [SerializeField] private Image playerIconPanelOuline;

    public event EventHandler OnDeckEditMenuClosed;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;

        playerIcon.sprite = PlayerCustomizationDataManager.Instance.GetPlayerIconSpriteFromSpriteId(HiddenTacticsMultiplayer.Instance.GetPlayerIconSpriteId());
        RefreshDeckVisual(DeckManager.LocalInstance.GetDeckSelected());
        RefreshPlayerIconsPanel();

        playerNameInputField.onValueChanged.AddListener((string newText) => {
            HiddenTacticsMultiplayer.Instance.SetPlayerName(newText);
            MainMenuUI_PlayerPanel.Instance.RefreshPlayerName(newText);
        });

        string playerName = SavingManager.Instance.LoadPlayerName();

        playerNameInputField.text = playerName;
    }

    private void RefreshDeckVisual(Deck deckSelected) {
        // Player Icon
        playerIconBackground.sprite = deckSelected.deckFactionSO.slotBackgroundSquare;
        playerIconOuline.sprite = deckSelected.deckFactionSO.slotBorder;
        playerIconPanelOuline.sprite = deckSelected.deckFactionSO.slotBorder;
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshDeckVisual(e.selectedDeck);
        RefreshPlayerIconsPanel();
    }

    private void RefreshPlayerIconsPanel() {
        playerIconsTemplate.gameObject.SetActive(false);

        selectedPlayerIconSO = PlayerCustomizationDataManager.Instance.GetPlayerIconSOFromSpriteId(HiddenTacticsMultiplayer.Instance.GetPlayerIconSpriteId());

        foreach (PlayerIconSelectSingleUI child in playerIcons) {
            if (child == playerIconsTemplate) continue;
            Destroy(child.gameObject);
        }

        playerIcons.Clear();
        foreach (PlayerIconSO iconSO in PlayerCustomizationDataManager.Instance.GetplayerIconSOList()) {
            PlayerIconSelectSingleUI icon = Instantiate(playerIconsTemplate, playerIconsContainer).GetComponent<PlayerIconSelectSingleUI>();
            icon.gameObject.SetActive(true);
            icon.SetFactionVisuals(DeckManager.LocalInstance.GetDeckSelected());
            icon.SetPlayerIcon(iconSO);

            if(iconSO == selectedPlayerIconSO) {
                icon.SetIsSelected(true);
            } else {
                icon.SetIsSelected(false);
            }

            playerIcons.Add(icon);
        };
    }

    public void SetSelectedIconSO(PlayerIconSO iconSO) {
        foreach(PlayerIconSelectSingleUI playerIconSelectSingleUI in playerIcons) {
            if(playerIconSelectSingleUI.GetPlayerIconSO() != iconSO) {
                playerIconSelectSingleUI.SetIsSelected(false);
            }
            else
            {
                playerIconSelectSingleUI.SetIsSelected(true);
            }
        }

        selectedPlayerIconSO = iconSO;
        playerIcon.sprite = iconSO.iconSprite;
    }

    public List<PlayerIconSelectSingleUI> GetPlayerIconsArray() {
        return playerIcons;
    }
}
