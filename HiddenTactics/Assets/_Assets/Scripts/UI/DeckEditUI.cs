using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditUI : MonoBehaviour
{
    public static DeckEditUI Instance;

    private Deck deckSelected;
    private FactionSO factionSelected;
    private DeckSlot deckSlotSelected;

    [SerializeField] Transform heroMenuContainer;
    [SerializeField] Transform heroSelectionSlotTemplate;
    [SerializeField] Transform unitsMenuContainer;
    [SerializeField] Transform unitSelectionSlotTemplate;
    [SerializeField] Transform buildingsMenuContainer;
    [SerializeField] Transform buildingSelectionSlotTemplate;
    [SerializeField] Transform spellsMenuContainer;
    [SerializeField] Transform spellSelectionSlotTemplate;

    [SerializeField] List<DeckSlot> deckSlotList;

    private void Awake() {
        Instance = this;
    }

    private void Start()
    {
        factionSelected = SavingManager.Instance.LoadFactionSO();

        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;
        DeckSlot.OnAnyDeckSlotSelected += DeckSlot_OnAnyDeckSlotSelected;
        gameObject.SetActive(false);
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

            foreach(TroopSO deckTroopSO in deckSelected.troopsInDeck)
            {
                if(deckTroopSO == troopSO)
                {
                    itemTemplateVisualUI.SetSelected(true);

                }
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

            foreach (BuildingSO deckBuildingSOSO in deckSelected.buildingsInDeck)
            {
                if (deckBuildingSOSO == buildingSO)
                {
                    itemTemplateVisualUI.SetSelected(true);

                }
            }
        }
    }

    private void RefreshSpellsMenu() {

    }

    public void CloseEditDeckMenu() {
        MainMenuCameraManager.Instance.SetBaseCamera();
        gameObject.SetActive(false);
        DeckSlotMouseHoverManager.Instance.SetEditingDeck(false);
    }

    public void SetFactionSelected(FactionSO factionSO) {
        factionSelected = factionSO;
        SavingManager.Instance.SaveFactionSO(factionSO);
        RefreshAllSelectionMenus();
    }

    private void DeckSlot_OnAnyDeckSlotSelected(object sender, System.EventArgs e)
    {
        deckSlotSelected = (DeckSlot)sender;
    }

    public int GetDeckSlotSelectedIndex()
    {
        return deckSlotSelected.GetDeckSlotNumber();
    }

    public void EnableEditDeckUI() {

        deckSelected = DeckManager.LocalInstance.GetDeckSelected();
        RefreshAllSelectionMenus();

        foreach(DeckSlot deckSlot in deckSlotList)
        {
            deckSlot.SetUIActive(true);
        }
    }

    public void OnDisable()
    {
        foreach (DeckSlot deckSlot in deckSlotList)
        {
            deckSlot.SetUIActive(false);
        }
    }
}
