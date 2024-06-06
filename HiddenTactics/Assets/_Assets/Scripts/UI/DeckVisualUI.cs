using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckVisualUI : MonoBehaviour
{

    public static DeckVisualUI Instance;

    [SerializeField] Button editDeckButton;
    [SerializeField] GameObject editDeckMenu;

    [SerializeField] TMP_InputField deckNameInputField;

    [SerializeField] private ChangeDeckFactionButtonUI deckFactionChangeButton;
    [SerializeField] private Transform changeDeckFactionContainer;
    [SerializeField] private Transform changeDeckFactionTemplate;

    [SerializeField] private List<Image> troopsInDeckImageList;
    [SerializeField] private List<Image> buildingsInDeckImageList;
    [SerializeField] private Image heroImage;
    [SerializeField] private List<Image> spellsInDeckImageList;

    private void Awake() {
        Instance = this;

        editDeckButton.onClick.AddListener(() => {
            OpenEditDeckMenu();
        });

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

        // Set all images transparent
        Color invisibleColor = new Color(0, 0, 0, 0);
        heroImage.sprite = null;
        heroImage.color = invisibleColor;

        foreach (Image image in troopsInDeckImageList) {
            image.sprite = null;
            image.color = invisibleColor;
        }

        foreach (Image image in buildingsInDeckImageList) {
            image.sprite = null;
            image.color = invisibleColor;
        }

        foreach (Image image in spellsInDeckImageList) {
            image.sprite = null;
            image.color = invisibleColor;
        }

        int i = 0;
        // Set deck troops in visual images
        foreach (TroopSO troopSO in deckSelected.troopsInDeck) {
            troopsInDeckImageList[i].sprite = troopSO.troopIllustrationSlotSprite;
            troopsInDeckImageList[i].color = Color.white;
            i++;
        }

        i = 0;
        // Set deck building in visual image
        foreach (BuildingSO buildingSO in deckSelected.buildingsInDeck) {
            buildingsInDeckImageList[i].sprite = buildingSO.buildingIllustrationSlotSprite;
            buildingsInDeckImageList[i].color = Color.white;
            i++;
        }
    }
    private void deckNameInputField_OnValueChanged() {
        DeckManager.LocalInstance.SetDeckName(deckNameInputField.text);
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshDeckVisual(e.selectedDeck);
    }

    private void OpenEditDeckMenu() {
        editDeckMenu.SetActive(true);
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
