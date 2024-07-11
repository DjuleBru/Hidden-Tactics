using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditUI : MonoBehaviour
{
    public static DeckEditUI Instance;

    private Deck deckSelected;
    private FactionSO factionSelected;
    private FactionSO previousFactionSelected;
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

    [SerializeField] private Image background;
    [SerializeField] private Image backgroundInnerShadow;
    [SerializeField] private Image borderShadow;
    [SerializeField] private Image border;
    [SerializeField] private Image item_Top;
    [SerializeField] private Image item_Bottom;
    [SerializeField] private Image item_Left;
    [SerializeField] private Image item_Right;
    [SerializeField] private Image heroesBorder;
    [SerializeField] private Image heroesBorderShadow;

    [SerializeField] private Image unitsBorder;
    [SerializeField] private Image unitsBorderShadow;
    [SerializeField] private Image buildingsBorder;
    [SerializeField] private Image buildingsBorderShadow;
    [SerializeField] private Image spellsBorder;
    [SerializeField] private Image spellsBorderShadow;

    private void Awake() {
        Instance = this;
    }

    private void Start()
    {
        factionSelected = SavingManager.Instance.LoadFactionSO();
        previousFactionSelected = SavingManager.Instance.LoadFactionSO();

        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;
        DeckSlot.OnAnyDeckSlotSelected += DeckSlot_OnAnyDeckSlotSelected;
        DeckSlot.OnAnyDeckSlotUnSelected += DeckSlot_OnAnyDeckSlotUnSelected;
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

        if(factionSelected != previousFactionSelected) {
            previousFactionSelected = factionSelected;
            RefreshPanelVisuals();
        }
    }

    private void RefreshPanelVisuals() {
        background.sprite = deckSelected.deckFactionSO.panelBackground;
        backgroundInnerShadow.sprite = deckSelected.deckFactionSO.panelBackgroundInnerShadow;
        borderShadow.sprite = deckSelected.deckFactionSO.panelBorder;
        border.sprite = deckSelected.deckFactionSO.panelBorder;

        item_Top.sprite = deckSelected.deckFactionSO.panelTopItem;
        item_Bottom.sprite = deckSelected.deckFactionSO.panelBottomItem;
        item_Left.sprite = deckSelected.deckFactionSO.panelLeftItem;
        item_Right.sprite = deckSelected.deckFactionSO.panelRightItem;

        heroesBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;
        unitsBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;
        buildingsBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;
        spellsBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;

        heroesBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;
        unitsBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;
        buildingsBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;
        spellsBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorder;

        //Change border size function of faction
        RectTransform borderRT = border.GetComponent(typeof(RectTransform)) as RectTransform;
        RectTransform borderShadowRT = borderShadow.GetComponent(typeof(RectTransform)) as RectTransform;
        RectTransform item_BottomRT = item_Bottom.GetComponent(typeof(RectTransform)) as RectTransform;
        RectTransform item_LeftRT = item_Left.GetComponent(typeof(RectTransform)) as RectTransform;
        RectTransform item_RightRT = item_Right.GetComponent(typeof(RectTransform)) as RectTransform;
        RectTransform item_TopRT = item_Top.GetComponent(typeof(RectTransform)) as RectTransform;

        float shadowOffset = 20;
        if (deckSelected.deckFactionSO.factionName == FactionSO.FactionName.Greenskins) {
            /*Right - Top*/
            borderRT.offsetMax = new Vector2(77, 165);
            borderShadowRT.offsetMax = borderRT.offsetMax - new Vector2(shadowOffset, shadowOffset);
            /*Left - Bottom*/
            borderRT.offsetMin = new Vector2(-40, -121);
            borderShadowRT.offsetMin = borderRT.offsetMin - new Vector2(shadowOffset, shadowOffset);

            item_BottomRT.anchoredPosition = new Vector2(0, -36);
            item_LeftRT.anchoredPosition = new Vector2(0, 0);
            item_RightRT.anchoredPosition = new Vector2(0, 0);
            item_TopRT.anchoredPosition = new Vector2(0, 0);
        }

        if (deckSelected.deckFactionSO.factionName == FactionSO.FactionName.Dwarves) {
            /*Right - Top*/
            borderRT.offsetMax = new Vector2(43, 77);
            borderShadowRT.offsetMax = borderRT.offsetMax - new Vector2(shadowOffset, shadowOffset);
            /*Left - Bottom*/
            borderRT.offsetMin = new Vector2(-40, -121);
            borderShadowRT.offsetMin = borderRT.offsetMin - new Vector2(shadowOffset, shadowOffset);

            item_BottomRT.anchoredPosition = new Vector2(0, -36);
            item_LeftRT.anchoredPosition = new Vector2(0, 0);
            item_RightRT.anchoredPosition = new Vector2(0, 0);
            item_TopRT.anchoredPosition = new Vector2(0, 0);
        }

        if (deckSelected.deckFactionSO.factionName == FactionSO.FactionName.Humans) {
            /*Right - Top*/
            borderRT.offsetMax = new Vector2(94, 87);
            borderShadowRT.offsetMax = borderRT.offsetMax - new Vector2(shadowOffset, shadowOffset);
            /*Left - Bottom*/
            borderRT.offsetMin = new Vector2(-96, -84);
            borderShadowRT.offsetMin = borderRT.offsetMin - new Vector2(shadowOffset, shadowOffset);

            item_BottomRT.anchoredPosition = new Vector2(0, -36);
            item_LeftRT.anchoredPosition = new Vector2(0, 0);
            item_RightRT.anchoredPosition = new Vector2(0, 0);
            item_TopRT.anchoredPosition = new Vector2(0, 20);
        }

        if (deckSelected.deckFactionSO.factionName == FactionSO.FactionName.Elves) {
            /*Right - Top*/
            borderRT.offsetMax = new Vector2(102, 160);
            borderShadowRT.offsetMax = borderRT.offsetMax - new Vector2(shadowOffset, shadowOffset);
            /*Left - Bottom*/
            borderRT.offsetMin = new Vector2(-110, -104);
            borderShadowRT.offsetMin = borderRT.offsetMin - new Vector2(shadowOffset, shadowOffset);

            item_BottomRT.anchoredPosition = new Vector2(0, 5);
            item_LeftRT.anchoredPosition = new Vector2(0, 0);
            item_RightRT.anchoredPosition = new Vector2(0, 0);
            item_TopRT.anchoredPosition = new Vector2(0, 0);
        }

    }

    private void RefreshHeroMenu() {
        foreach (Transform child in heroMenuContainer) {
            if (child == heroSelectionSlotTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void RefreshUnitsMenu() {

        foreach(Transform child in unitsMenuContainer) {
            if (child == unitSelectionSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(TroopSO troopSO in factionSelected.troopsInFaction) {
            Transform troopTemplateInstantiated = Instantiate(unitSelectionSlotTemplate, unitsMenuContainer);
            ItemTemplateUI_DeckCreation itemTemplateVisualUI = troopTemplateInstantiated.GetComponent<ItemTemplateUI_DeckCreation>();

            itemTemplateVisualUI.SetTroopSO(troopSO);
            itemTemplateVisualUI.SetDeckVisuals(deckSelected);

            foreach (TroopSO deckTroopSO in deckSelected.troopsInDeck)
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
            ItemTemplateUI_DeckCreation itemTemplateVisualUI = troopTemplateInstantiated.GetComponent<ItemTemplateUI_DeckCreation>();

            itemTemplateVisualUI.SetBuildingSO(buildingSO);
            itemTemplateVisualUI.SetDeckVisuals(deckSelected);

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
        foreach (Transform child in spellsMenuContainer) {
            if (child == spellSelectionSlotTemplate) continue;
            Destroy(child.gameObject);
        }
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

    private void DeckSlot_OnAnyDeckSlotUnSelected(object sender, System.EventArgs e) {
        if(deckSlotSelected == (DeckSlot)sender) {
            deckSlotSelected = null;
        }
    }

    public int GetDeckSlotSelectedIndex()
    {
        return deckSlotSelected.GetDeckSlotNumber();
    }

    public DeckSlot GetDeckSlotSelected() {
        return deckSlotSelected;
    }

    public void EnableEditDeckUI() {

        deckSelected = DeckManager.LocalInstance.GetDeckSelected();
        RefreshAllSelectionMenus();
        RefreshPanelVisuals();
        foreach (DeckSlot deckSlot in deckSlotList)
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
