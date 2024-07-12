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
    [SerializeField] private Transform playerIconsContainer;
    private PlayerIconSelectSingleUI[] playerIcons;
    private bool playerIconsContainerActive;
    [SerializeField] private Image playerIcon;


    [SerializeField] private Image panelBackground;
    [SerializeField] private Image panelBorder;
    [SerializeField] private Image panelShadowBorder;

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
        playerIcons = playerIconsContainer.GetComponentsInChildren<PlayerIconSelectSingleUI>();
        playerIconsContainer.gameObject.SetActive(false);

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
        panelBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;
        panelShadowBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;
    }

    private void deckNameInputField_OnValueChanged() {
        DeckManager.LocalInstance.SetDeckName(deckNameInputField.text);
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshDeckVisual(e.selectedDeck);
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

    public PlayerIconSelectSingleUI[] GetPlayerIconsArray() {
        return playerIcons;
    }

    public void ShowPlayerIconsPanel() {
        playerIconsContainer.gameObject.SetActive(true);
        playerIconsContainerActive = true;
    }

    private void HidePlayerIconsPanel() {
        playerIconsContainer.gameObject.SetActive(false);
        playerIconsContainerActive = false;
    }

    public void TogglePlayerIconsPanel() {
        playerIconsContainerActive = !playerIconsContainerActive;
        playerIconsContainer.gameObject.SetActive(playerIconsContainerActive);
    }

    public void ShowPanel() {
        gameObject.SetActive(true);
    }

    public void HidePanel() {
        gameObject.SetActive(false);
    }
}
