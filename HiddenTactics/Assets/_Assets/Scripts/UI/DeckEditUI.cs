using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditUI : MonoBehaviour
{
    public static DeckEditUI Instance;

    private Deck deckSelected;
    private FactionSO factionSelected;
    [SerializeField] private FactionSO defaultFactionSO;

    [SerializeField] Button backToMenuButton;
    [SerializeField] Button saveDeckButton;

    [SerializeField] Transform heroMenuContainer;
    [SerializeField] Transform heroSelectionSlotTemplate;
    [SerializeField] Transform unitsMenuContainer;
    [SerializeField] Transform unitSelectionSlotTemplate;
    [SerializeField] Transform buildingsMenuContainer;
    [SerializeField] Transform buildingSelectionSlotTemplate;
    [SerializeField] Transform spellsMenuContainer;
    [SerializeField] Transform spellSelectionSlotTemplate;

    private void Awake() {
        Instance = this;

        factionSelected = ES3.Load("DeckFactionSelected", defaultValue: defaultFactionSO);

        backToMenuButton.onClick.AddListener(() => {
            CloseEditDeckMenu();
        });

        gameObject.SetActive(false);
    }

    private void Start() {
        DeckManager.Instance.OnDeckChanged += DeckManager_OnDeckChanged;
    }

    private void DeckManager_OnDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        deckSelected = e.selectedDeck;
        factionSelected = e.selectedDeck.deckFactionSO;
        RefreshAllSelectionMenus();
    }

    private void RefreshAllSelectionMenus() {
        heroSelectionSlotTemplate.gameObject.SetActive(true);
        unitSelectionSlotTemplate.gameObject.SetActive(true);
        buildingSelectionSlotTemplate.gameObject.SetActive(true);
        spellSelectionSlotTemplate.gameObject.SetActive(true);

        RefreshHeroMenu();
        RefreshUnitsMenu();
        RefreshBuildingsMenu();
        RefreshSpellsMenu();


        heroSelectionSlotTemplate.gameObject.SetActive(false);
        unitSelectionSlotTemplate.gameObject.SetActive(false);
        buildingSelectionSlotTemplate.gameObject.SetActive(false);
        spellSelectionSlotTemplate.gameObject.SetActive(false);
    }

    private void RefreshHeroMenu() {
        
    }

    private void RefreshUnitsMenu() {

        foreach(Transform child in unitsMenuContainer) {
            if (child == unitSelectionSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(TroopSO troopSO in factionSelected.troopsInFaction) {
            Transform troopTemplateInstantiated = Instantiate(unitSelectionSlotTemplate, unitsMenuContainer);
            SelectionSlotTemplateUI itemTemplateVisualUI = troopTemplateInstantiated.GetComponent<SelectionSlotTemplateUI>();

            itemTemplateVisualUI.SetTroopSO(troopSO);

            if(deckSelected.troopsInDeck.Contains(troopSO)) {
                itemTemplateVisualUI.SetSelected(true);
            }
        }
    }

    private void RefreshBuildingsMenu() {
        foreach (Transform child in buildingsMenuContainer) {
            if (child == buildingSelectionSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (BuildingSO buildingSO in factionSelected.buildingsInFaction) {
            Transform troopTemplateInstantiated = Instantiate(buildingSelectionSlotTemplate, buildingsMenuContainer);
            SelectionSlotTemplateUI itemTemplateVisualUI = troopTemplateInstantiated.GetComponent<SelectionSlotTemplateUI>();

            itemTemplateVisualUI.SetBuildingSO(buildingSO);

            if (deckSelected.buildingsInDeck.Contains(buildingSO)) {
                itemTemplateVisualUI.SetSelected(true);
            }
        }
    }

    private void RefreshSpellsMenu() {

    }

    private void CloseEditDeckMenu() {
        gameObject.SetActive(false);
    }

    public void SetFactionSelected(FactionSO factionSO) {
        factionSelected = factionSO;
        ES3.Save("DeckFactionSelected", factionSO);
        RefreshAllSelectionMenus();
    }

    public void OnEnable() {
        deckSelected = DeckManager.Instance.GetDeckSelected();
        RefreshAllSelectionMenus();
    }
}
