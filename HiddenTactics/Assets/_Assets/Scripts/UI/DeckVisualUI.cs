using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckVisualUI : MonoBehaviour
{

    public static DeckVisualUI Instance;

    [SerializeField] Button saveDeckButton;
    [SerializeField] GameObject editDeckMenu;

    [SerializeField] TMP_InputField deckNameInputField;

    [SerializeField] private ChangeDeckFactionButtonUI deckFactionChangeButton;
    [SerializeField] private Transform changeDeckFactionContainer;
    [SerializeField] private Transform changeDeckFactionTemplate;

    public event EventHandler OnDeckEditMenuClosed;

    private void Awake() {
        Instance = this;

        saveDeckButton.onClick.AddListener(() => {
            saveDeckButton.gameObject.SetActive(false);
            DeckEditUI.Instance.CloseEditDeckMenu();
            DeckVisualWorldUI.Instance.EnableEditDeckButton();
            OnDeckEditMenuClosed?.Invoke(this, EventArgs.Empty);
        });

        saveDeckButton.gameObject.SetActive(false);
        changeDeckFactionTemplate.gameObject.SetActive(false);
        changeDeckFactionContainer.gameObject.SetActive(false);
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;
        deckNameInputField.onValueChanged.AddListener(delegate { deckNameInputField_OnValueChanged(); });
    }


    private void RefreshDeckVisual(Deck deckSelected) {
        // Deck faction
        deckFactionChangeButton.SetFactionSO(deckSelected.deckFactionSO);

        // Deck Name
        deckNameInputField.text = DeckManager.LocalInstance.GetDeckSelected().deckName;
    }
    private void deckNameInputField_OnValueChanged() {
        DeckManager.LocalInstance.SetDeckName(deckNameInputField.text);
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshDeckVisual(e.selectedDeck);
    }

    public void StartEditingDeck() {
        MainMenuCameraManager.Instance.SetEditDeckCamera();
        editDeckMenu.SetActive(true);
        saveDeckButton.gameObject.SetActive(true);
        DeckEditUI.Instance.EnableEditDeckUI();
        DeckSlotMouseHoverManager.Instance.SetEditingDeck(true);
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

}
