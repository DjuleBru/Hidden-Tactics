using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditUI : MonoBehaviour
{
    public static DeckEditUI Instance;

    private Animator animator;
    private Deck deckSelected;
    private FactionSO factionSelected;
    private FactionSO previousFactionSelected;
    private DeckSlot deckSlotSelected;

    public event EventHandler OnDeckEditMenuClosed;

    [SerializeField] Button saveDeckButton;
    [SerializeField] TMP_InputField deckNameInputField;

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
    [SerializeField] private Image windowBorder;
    [SerializeField] private Image windowBorderShadow;
    [SerializeField] private Image item_Top;
    [SerializeField] private Image item_Bottom;
    [SerializeField] private Image item_Left;
    [SerializeField] private Image item_Right;
    [SerializeField] private Image heroesBorder;
    [SerializeField] private Image heroesBorderShadow;

    [SerializeField] private Image deckSelectionBorder;
    [SerializeField] private Image deckSelectionBorderShadow;
    [SerializeField] private Image deckFactionSlotBorder;
    [SerializeField] private Image deckFactionSlotShadowBorder;
    [SerializeField] private Image deckFactionSlotBackground;
    [SerializeField] private Image deckFactionSlotImage;
    [SerializeField] private Image deckFactionSlotShadowImage;
    [SerializeField] private Image heroBorder;
    [SerializeField] private Image heroBackground;
    [SerializeField] private Image unitsBorder;
    [SerializeField] private Image unitsBackground;
    [SerializeField] private Image unitsBorderShadow;
    [SerializeField] private Image buildingsBorder;
    [SerializeField] private Image buildingsBackground;
    [SerializeField] private Image buildingsBorderShadow;
    [SerializeField] private Image spellsBorder;
    [SerializeField] private Image spellsBackground;
    [SerializeField] private Image spellsBorderShadow;

    [SerializeField] private GameObject elvesBorderShadow;
    [SerializeField] private GameObject elvesBorder;
    [SerializeField] private GameObject humansBorderShadow;
    [SerializeField] private GameObject humansBorder;
    [SerializeField] private GameObject dwarvesBorderShadow;
    [SerializeField] private GameObject dwarvesBorder;
    [SerializeField] private GameObject greenSkinsBorderShadow;
    [SerializeField] private GameObject greenSkinsBorder;

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material panelSelectingMaterial;
    [SerializeField] private Color panelBackgroundSelectingColor;
    [SerializeField] private Color panelBackgroundCleanColor;

    private void Awake() {
        Instance = this;
        animator = GetComponent<Animator>();
        saveDeckButton.onClick.AddListener(() => {
            CloseEditDeckMenu();
            OnDeckEditMenuClosed?.Invoke(this, EventArgs.Empty);
        });
    }

    private void Start()
    {
        deckNameInputField.onValueChanged.AddListener(delegate { deckNameInputField_OnValueChanged(); });

        factionSelected = SavingManager.Instance.LoadFactionSO();
        previousFactionSelected = SavingManager.Instance.LoadFactionSO();

        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;
        DeckSlot.OnAnyDeckSlotSelected += DeckSlot_OnAnyDeckSlotSelected;
        DeckSlot.OnAnyDeckSlotUnSelected += DeckSlot_OnAnyDeckSlotUnSelected;
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {
        deckSelected = e.selectedDeck;
        RefreshAllSelectionMenus();
    }

    private void RefreshAllSelectionMenus() {
        factionSelected = deckSelected.deckFactionSO;

        // Deck Name
        deckNameInputField.text = DeckManager.LocalInstance.GetDeckSelected().deckName;

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

        windowBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        windowBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;

        item_Top.sprite = deckSelected.deckFactionSO.panelTopItem;
        item_Bottom.sprite = deckSelected.deckFactionSO.panelBottomItem;
        item_Left.sprite = deckSelected.deckFactionSO.panelLeftItem;
        item_Right.sprite = deckSelected.deckFactionSO.panelRightItem;

        deckSelectionBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        heroesBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        unitsBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        buildingsBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        spellsBorder.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;

        deckSelectionBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        heroesBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        unitsBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        buildingsBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;
        spellsBorderShadow.sprite = deckSelected.deckFactionSO.panelBackgroundBorderSimple;

        deckFactionSlotBackground.sprite = deckSelected.deckFactionSO.slotBackgroundSquare;
        deckFactionSlotBorder.sprite = deckSelected.deckFactionSO.slotBorder;
        deckFactionSlotShadowBorder.sprite = deckSelected.deckFactionSO.slotBorder;
        deckFactionSlotImage.sprite = deckSelected.deckFactionSO.factionSprite;
        deckFactionSlotShadowImage.sprite = deckSelected.deckFactionSO.factionSprite;

        if (deckSelected.deckFactionSO.factionName == FactionSO.FactionName.Greenskins) {
            elvesBorderShadow.gameObject.SetActive(false);
            elvesBorder.gameObject.SetActive(false);
            humansBorderShadow.gameObject.SetActive(false);
            humansBorder.gameObject.SetActive(false);
            dwarvesBorderShadow.gameObject.SetActive(false);
            dwarvesBorder.gameObject.SetActive(false);
            greenSkinsBorderShadow.gameObject.SetActive(true);
            greenSkinsBorder.gameObject.SetActive(true);
        }
        if (deckSelected.deckFactionSO.factionName == FactionSO.FactionName.Humans) {
            elvesBorderShadow.gameObject.SetActive(false);
            elvesBorder.gameObject.SetActive(false);
            humansBorderShadow.gameObject.SetActive(true);
            humansBorder.gameObject.SetActive(true);
            dwarvesBorderShadow.gameObject.SetActive(false);
            dwarvesBorder.gameObject.SetActive(false);
            greenSkinsBorderShadow.gameObject.SetActive(false);
            greenSkinsBorder.gameObject.SetActive(false);
        }
        if (deckSelected.deckFactionSO.factionName == FactionSO.FactionName.Elves) {
            elvesBorderShadow.gameObject.SetActive(true);
            elvesBorder.gameObject.SetActive(true);
            humansBorderShadow.gameObject.SetActive(false);
            humansBorder.gameObject.SetActive(false);
            dwarvesBorderShadow.gameObject.SetActive(false);
            dwarvesBorder.gameObject.SetActive(false);
            greenSkinsBorderShadow.gameObject.SetActive(false);
            greenSkinsBorder.gameObject.SetActive(false);
        }
        if (deckSelected.deckFactionSO.factionName == FactionSO.FactionName.Dwarves) {
            elvesBorderShadow.gameObject.SetActive(false);
            elvesBorder.gameObject.SetActive(false);
            humansBorderShadow.gameObject.SetActive(false);
            humansBorder.gameObject.SetActive(false);
            dwarvesBorderShadow.gameObject.SetActive(true);
            dwarvesBorder.gameObject.SetActive(true);
            greenSkinsBorderShadow.gameObject.SetActive(false);
            greenSkinsBorder.gameObject.SetActive(false);
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

            bool troopIsInDeck = false;
            foreach (TroopSO deckTroopSO in deckSelected.troopsInDeck)
            {
                if(deckTroopSO == troopSO)
                {
                    troopIsInDeck = true;
                }
            }
            itemTemplateVisualUI.SetSelected(troopIsInDeck);
        }

        if (deckSlotSelected != null) {
            RefreshSelectedMenus(deckSlotSelected);
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

            bool buildingIsInDeck = false;
            foreach (BuildingSO deckBuildingSOSO in deckSelected.buildingsInDeck)
            {
                if (deckBuildingSOSO == buildingSO)
                {
                    buildingIsInDeck = true;
                }
            }
            itemTemplateVisualUI.SetSelected(buildingIsInDeck);
        }

        if(deckSlotSelected != null) {
            RefreshSelectedMenus(deckSlotSelected);
        }
    }

    private void RefreshSpellsMenu() {
        foreach (Transform child in spellsMenuContainer) {
            if (child == spellSelectionSlotTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    public void SetFactionSelected(FactionSO factionSO) {
        factionSelected = factionSO;
        SavingManager.Instance.SaveFactionSO(factionSO);
        RefreshAllSelectionMenus();
    }

    private void DeckSlot_OnAnyDeckSlotSelected(object sender, System.EventArgs e)
    {
        deckSlotSelected = (DeckSlot)sender;
        RefreshSelectedMenus(deckSlotSelected);
    }

    private void RefreshSelectedMenus(DeckSlot deckSlotSelected) {
        HideSelectingTypes();
        if (deckSlotSelected.GetCanHostTroop()) {
            ShowSelectingTroop(true);
        }

        if (deckSlotSelected.GetCanHostBuilding()) {
            ShowSelectingBuilding(true);
        }

        if (deckSlotSelected.GetCanHostSpell()) {
            ShowSelectingSpell(true);
        }

        if (deckSlotSelected.GetCanHostHero()) {
            ShowSelectingHero(true);
        }
    }

    private void HideSelectingTypes() {
        ShowSelectingTroop(false);
        ShowSelectingBuilding(false);
        ShowSelectingSpell(false);
        ShowSelectingHero(false);
    }

    private void ShowSelectingTroop(bool selecting) {
        if(selecting) {
            unitsBorder.material = panelSelectingMaterial;
            unitsBackground.color = panelBackgroundSelectingColor;

            EnableButtonsInCategory(unitsMenuContainer, true);

        } else {
            unitsBorder.material = cleanMaterial;
            unitsBackground.color = panelBackgroundCleanColor;

            EnableButtonsInCategory(unitsMenuContainer, false);
        }
    }

    private void ShowSelectingBuilding(bool selecting) {
        if (selecting) {
            buildingsBorder.material = panelSelectingMaterial;
            buildingsBackground.color = panelBackgroundSelectingColor;

            EnableButtonsInCategory(buildingsMenuContainer, true);
        }
        else {
            buildingsBorder.material = cleanMaterial;
            buildingsBackground.color = panelBackgroundCleanColor;

            EnableButtonsInCategory(buildingsMenuContainer, false);
        }
    }

    private void ShowSelectingSpell(bool selecting) {
        if (selecting) {
            spellsBorder.material = panelSelectingMaterial;
            spellsBackground.color = panelBackgroundSelectingColor;

            EnableButtonsInCategory(spellsMenuContainer, true);
        }
        else {
            spellsBorder.material = cleanMaterial;
            spellsBackground.color = panelBackgroundCleanColor;

            EnableButtonsInCategory(spellsMenuContainer, false);
        }
    }

    private void ShowSelectingHero(bool selecting) {
        if (selecting) {
            heroBorder.material = panelSelectingMaterial;
            heroBackground.color = panelBackgroundSelectingColor;

            EnableButtonsInCategory(heroMenuContainer, true);
        }
        else {
            heroBorder.material = cleanMaterial;
            heroBackground.color = panelBackgroundCleanColor;

            EnableButtonsInCategory(heroMenuContainer, false);
        }
    }

    private void EnableButtonsInCategory(Transform itemMenuContainer, bool enable) {

        foreach (ItemTemplateUI_DeckCreation itemTemplate in itemMenuContainer.GetComponentsInChildren<ItemTemplateUI_DeckCreation>()) {
            itemTemplate.EnableButton(enable);
        }
    }

    private void DeckSlot_OnAnyDeckSlotUnSelected(object sender, System.EventArgs e) {
        if(deckSlotSelected == (DeckSlot)sender) {
            deckSlotSelected = null;
            HideSelectingTypes();
        }
    }

    public int GetDeckSlotSelectedIndex()
    {
        return deckSlotSelected.GetDeckSlotNumber();
    }

    public DeckSlot GetDeckSlotSelected() {
        return deckSlotSelected;
    }

    public void CloseEditDeckMenu() {
        DeckSlotMouseHoverManager.Instance.SetEditingDeck(false);
        foreach (DeckSlot deckSlot in deckSlotList) {
            deckSlot.SetUIActive(false);
        }

        StartCoroutine(SwitchToMainMenuUI(.6f, .5f, .5f, 1.4f));

    }

    public void EnableEditDeckUI() {
        StartCoroutine(SwitchToEditDeckUI(.5f, 1f, 1f));

        deckSelected = DeckManager.LocalInstance.GetDeckSelected();
        RefreshAllSelectionMenus();
        RefreshPanelVisuals();
    }

    private void deckNameInputField_OnValueChanged() {
        DeckManager.LocalInstance.SetDeckName(deckNameInputField.text);
    }

    public IEnumerator SwitchToEditDeckUI(float delayToActivateCameraTransition, float delayToOpenEditDeckPanel, float delayToActivateEditDeckPanels) {

        DeckVisualWorldUI.Instance.SetDeckVisualFlyUp();
        yield return new WaitForSeconds(delayToActivateCameraTransition);

        MainMenuCameraManager.Instance.SetEditDeckCamera();
        MainMenuUI.Instance.MainMenuUIToEditDeckTransition();

        yield return new WaitForSeconds(delayToOpenEditDeckPanel);

        PlayerDeckVisual.Instance.SetEditDeckMenuVisual();
        animator.SetTrigger("Open");

        yield return new WaitForSeconds(delayToActivateEditDeckPanels);

        ActivateDeckEditingUI();
        DeckVisualWorldUI.Instance.SetDeckVisualFlyDown();
        DeckVisualWorldUI.Instance.DisableEditDeckButton();
        DeckSlotMouseHoverManager.Instance.SetEditingDeck(true);
    }

    public IEnumerator SwitchToMainMenuUI(float delayToCloseEditDeckUI, float delayToActivateCameraTransition, float delayToActivateMainMenuUI, float delayToFLyDownDeckVisual) {
        DeckVisualWorldUI.Instance.SetDeckVisualFlyUp();

        yield return new WaitForSeconds(delayToCloseEditDeckUI);
        animator.SetTrigger("Close");

        yield return new WaitForSeconds(delayToActivateCameraTransition);
        MainMenuCameraManager.Instance.SetBaseCamera();
        PlayerDeckVisual.Instance.SetMainMenuVisual();

        yield return new WaitForSeconds(delayToActivateMainMenuUI);
        MainMenuUI.Instance.MainMenuUIFromEditDeckTransition();

        yield return new WaitForSeconds(delayToFLyDownDeckVisual);
        DeckVisualWorldUI.Instance.SetDeckVisualFlyDown();
        DeckVisualWorldUI.Instance.EnableEditDeckButton();
    }

    public IEnumerator TriggerOpenPanelAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
    }

    public IEnumerator ActivateDeckEditingUIAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
    }

    private void ActivateDeckEditingUI() {

        foreach (DeckSlot deckSlot in deckSlotList) {
            deckSlot.SetUIActive(true);
            deckSlot.SetAnimatorActive(true);
            deckSlot.GetDeckSlotVisual().EnableDeckSlotHover();
        }
    }
}
