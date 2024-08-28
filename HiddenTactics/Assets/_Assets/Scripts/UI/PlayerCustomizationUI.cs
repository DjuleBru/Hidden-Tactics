using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCustomizationUI : MonoBehaviour
{

    public static PlayerCustomizationUI Instance;

    [SerializeField] GameObject editDeckMenu;

    [SerializeField] TMP_InputField deckNameInputField;
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

    [SerializeField] private Image panelBackground;
    [SerializeField] private Image panelBorder;
    [SerializeField] private Image panelShadowBorder;

    [SerializeField] private Image selectIconPanelBackground;
    [SerializeField] private Image selectIconPanelBorder;

    [SerializeField] private ChangeDeckFactionButtonUI deckFactionChangeButton;
    [SerializeField] private Transform changeDeckFactionContainer;
    [SerializeField] private Transform changeDeckFactionTemplate;

    public event EventHandler OnDeckEditMenuClosed;

    private void Awake() {
        Instance = this;

        changeDeckFactionTemplate.gameObject.SetActive(false);
        changeDeckFactionContainer.gameObject.SetActive(false);
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;
        deckNameInputField.onValueChanged.AddListener(delegate { deckNameInputField_OnValueChanged(); });

        playerIcon.sprite = PlayerCustomizationDataManager.Instance.GetPlayerIconSpriteFromSpriteId(HiddenTacticsMultiplayer.Instance.GetPlayerIconSpriteId());
        playerIconsPanel.gameObject.SetActive(false);
        RefreshDeckVisual(DeckManager.LocalInstance.GetDeckSelected());
        RefreshPlayerIconsPanel();

        playerNameInputField.onValueChanged.AddListener((string newText) => {
            HiddenTacticsMultiplayer.Instance.SetPlayerName(newText);
        });

        string playerName = SavingManager.Instance.LoadPlayerName();

        playerNameInputField.text = playerName;
    }

    private void RefreshDeckVisual(Deck deckSelected) {
        // Deck faction
        deckFactionChangeButton.SetFactionSO(deckSelected.deckFactionSO);

        // Deck Name
        deckNameInputField.text = DeckManager.LocalInstance.GetDeckSelected().deckName;

        // Panel
        panelBackground.sprite = deckSelected.deckFactionSO.panelBackground;
        panelBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        selectIconPanelBackground.sprite = deckSelected.deckFactionSO.panelBackground;
        selectIconPanelBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        panelShadowBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;

        // Player Icon
        playerIconBackground.sprite = deckSelected.deckFactionSO.slotBackgroundSquare;
        playerIconOuline.sprite = deckSelected.deckFactionSO.slotBorder;
    }

    private void deckNameInputField_OnValueChanged() {
        DeckManager.LocalInstance.SetDeckName(deckNameInputField.text);
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

    public void OpenChangeDeckFactionContainer() {
        changeDeckFactionContainer.gameObject.SetActive(true);
        changeDeckFactionTemplate.gameObject.SetActive(true);

        foreach (Transform child in changeDeckFactionContainer) {
            if (child == changeDeckFactionTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(FactionSO factionSO in DeckManager.LocalInstance.GetFactionSOList()) {
            if (factionSO == DeckManager.LocalInstance.GetDeckSelected().deckFactionSO) continue;

            Transform changeDeckFactionTemplateInstantiated = Instantiate(changeDeckFactionTemplate, changeDeckFactionContainer);
            FactionSelectionButtonUI factionSelectionButton = changeDeckFactionTemplateInstantiated.GetComponent<FactionSelectionButtonUI>();

            factionSelectionButton.SetFactionSO(factionSO);
        }

        changeDeckFactionTemplate.gameObject.SetActive(false);
    }

    public void CloseChangeDeckFactionContainer() {
        changeDeckFactionContainer.gameObject.SetActive(false);
        changeDeckFactionTemplate.gameObject.SetActive(false);
    }

    public List<PlayerIconSelectSingleUI> GetPlayerIconsArray() {
        return playerIcons;
    }

    public void ShowPlayerIconsPanel() {
        playerIconsPanel.SetActive(true);
        playerIconsContainerActive = true;
    }

    private void HidePlayerIconsPanel() {
        playerIconsPanel.gameObject.SetActive(false);
        playerIconsContainerActive = false;
    }

    public void TogglePlayerIconsPanel() {
        playerIconsContainerActive = !playerIconsContainerActive;
        playerIconsPanel.SetActive(playerIconsContainerActive);
    }

    public void ShowPanel() {
        gameObject.SetActive(true);
    }

    public void HidePanel() {
        gameObject.SetActive(false);
    }
}
