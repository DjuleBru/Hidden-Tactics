using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditUI : MonoBehaviour
{
    public static DeckEditUI Instance;

    private Deck deckSelected;
    private FactionSO factionSelected;

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

        factionSelected = SavingManager.Instance.LoadFactionSO();

        backToMenuButton.onClick.AddListener(() => {
            CloseEditDeckMenu();
        });

        gameObject.SetActive(false);
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {
        deckSelected = e.selectedDeck;
        RefreshAllSelectionMenus();
    }

    private void RefreshAllSelectionMenus() {
        factionSelected = deckSelected.deckFactionSO;

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
        SavingManager.Instance.SaveFactionSO(factionSO);
        RefreshAllSelectionMenus();
    }

    public void OnEnable() {
        deckSelected = DeckManager.LocalInstance.GetDeckSelected();
        RefreshAllSelectionMenus();
    }
}
