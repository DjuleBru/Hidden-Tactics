using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EditBattlefieldUI : MonoBehaviour
{
    public static EditBattlefieldUI Instance;

    [SerializeField] private GameObject battlefieldCustomizationUI;

    [SerializeField] private Transform battlefieldVisualGridContainer;
    [SerializeField] private Transform battlefieldVisualGridTemplate;

    [SerializeField] private Transform battlefieldVisualBaseContainer;
    [SerializeField] private Transform battlefieldVisualBaseTemplate;

    [SerializeField] private Transform battlefieldVillageSpritesContainer;
    [SerializeField] private Transform battlefieldVillageSpriteTemplate;


    [SerializeField] private GameObject editGridTilesButton;
    [SerializeField] private GameObject editBaseButton;
    [SerializeField] private GameObject editVillagesButton;
    [SerializeField] private GameObject editBattlefieldGeneralButton;

    [SerializeField] private Image background;
    [SerializeField] private Image border;
    [SerializeField] private Image borderShadow;
    [SerializeField] private Image innerShadow;

    private bool editingVillages;
    private bool editingGridTiles;

    private void Awake() {
        Instance = this;

        battlefieldCustomizationUI.SetActive(false);
        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualGridTemplate.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVisualBaseTemplate.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(false);
        battlefieldVillageSpriteTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {
        if(editingVillages) {
            RefreshBattlefieldVillageSpritesContainer(e.selectedDeck);
        }

        if(editingGridTiles) {
            RefreshBattlefieldVisualGridContainer(e.selectedDeck.deckFactionSO);
        }
        RefreshPanelVisuals(e.selectedDeck);
    }

    public void StartEditBattlefieldGridTiles() {
        editingGridTiles = true;
        editingVillages = false;

        battlefieldVisualGridContainer.gameObject.SetActive(true);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(false);

        RefreshBattlefieldVisualGridContainer(DeckManager.LocalInstance.GetDeckSelected().deckFactionSO);

        MainMenuCameraManager.Instance.SetEditBattlefieldGridTilesCamera();
    }

    public void StartEditBattlefieldVillages() {
        editingVillages = true;
        editingGridTiles = false;

        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(true);

        RefreshBattlefieldVillageSpritesContainer(DeckManager.LocalInstance.GetDeckSelected());

        MainMenuCameraManager.Instance.SetEditBattlefieldVillagesCamera();
    }

    public void StartEditBattlefieldBase() {

        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(true);
        battlefieldVillageSpritesContainer.gameObject.SetActive(false);

        RefreshBattlefieldVisualBaseContainer();

        MainMenuCameraManager.Instance.SetEditBattlefieldBaseCamera();
    }

    private void RefreshBattlefieldVisualGridContainer(FactionSO factionSO) {
        battlefieldVisualGridTemplate.gameObject.SetActive(true);

        foreach (Transform child in battlefieldVisualGridContainer) {
            if (child == battlefieldVisualGridTemplate) continue;
            Destroy(child.gameObject);
        }

        int id = 0;
        foreach(GridTileVisualSO tileSO in PlayerCustomizationDataManager.Instance.GetGridTileVisualSOList()) {
            if(tileSO.gridFactionSO != factionSO) continue;
            Transform visualGridTemplateInstantiated = Instantiate(battlefieldVisualGridTemplate, battlefieldVisualGridContainer);
            visualGridTemplateInstantiated.GetComponent<BattlefieldVisualGridTemplate>().SetGridTileVisualSO(tileSO);
            id++;
        }

        battlefieldVisualGridTemplate.gameObject.SetActive(false);
    }

    private void RefreshBattlefieldVisualBaseContainer() {
        battlefieldVisualBaseTemplate.gameObject.SetActive(true);

        foreach (Transform child in battlefieldVisualBaseContainer) {
            if (child == battlefieldVisualBaseTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Sprite battlefieldBaseSprite in PlayerCustomizationDataManager.Instance.GetBattlefieldBaseSpriteList()) {
            Transform battlefieldBaseVisualInstantiated = Instantiate(battlefieldVisualBaseTemplate, battlefieldVisualBaseContainer);
            battlefieldBaseVisualInstantiated.GetComponent<BattlefieldVisualBaseTemplate>().SetBattlefieldBaseImage(battlefieldBaseSprite);
        }

        battlefieldVisualBaseTemplate.gameObject.SetActive(false);
    }

    private void RefreshBattlefieldVillageSpritesContainer(Deck deck) {
        battlefieldVillageSpriteTemplate.gameObject.SetActive(true);

        foreach (Transform child in battlefieldVillageSpritesContainer) {
            if (child == battlefieldVillageSpriteTemplate) continue;
            Destroy(child.gameObject);
        }

        int id = 0;
        foreach (Sprite villageSprite in PlayerCustomizationDataManager.Instance.GetVillageSpriteList()) {

            if (!deck.deckFactionSO.villageSpritesInFaction.Contains(villageSprite)) continue;

            Transform villageSpriteInstantiated = Instantiate(battlefieldVillageSpriteTemplate, battlefieldVillageSpritesContainer);
            villageSpriteInstantiated.GetComponent<BattlefieldVillateTemplateUI>().SetVillageSpriteVisual(villageSprite);

            if(SavingManager.Instance.LoadVillageSpriteList(deck).Contains(villageSprite)) {
                villageSpriteInstantiated.GetComponent<BattlefieldVillateTemplateUI>().SetVillageSelected(true);
            }

            id++;
        }

        battlefieldVillageSpriteTemplate.gameObject.SetActive(false);
    }

    private void RefreshPanelVisuals(Deck deck) {
        background.sprite = deck.deckFactionSO.panelBackground;
        border.sprite = deck.deckFactionSO.panelBackgroundBorder;
        borderShadow.sprite = deck.deckFactionSO.panelBackgroundBorder;
        innerShadow.sprite = deck.deckFactionSO.panelBackgroundInnerShadow;
    }

    public void StartEditBattlefield() {
        battlefieldCustomizationUI.SetActive(true);
        PlayerCustomizationUI.Instance.gameObject.SetActive(false);

        editGridTilesButton.SetActive(true);
        editBaseButton.SetActive(true);
        editVillagesButton.SetActive(true);
        editBattlefieldGeneralButton.SetActive(false);

        MainMenuCameraManager.Instance.SetEditBattlefieldCamera();

        RefreshPanelVisuals(DeckManager.LocalInstance.GetDeckSelected());
    }

    public void StopEditBattlefield() {
        editingVillages = false;
        editingGridTiles = false;
        battlefieldCustomizationUI.SetActive(false);
        PlayerCustomizationUI.Instance.gameObject.SetActive(true);
        MainMenuCameraManager.Instance.SetBaseCamera();

        editGridTilesButton.SetActive(false);
        editBaseButton.SetActive(false);
        editVillagesButton.SetActive(false);
        editBattlefieldGeneralButton.SetActive(true);

        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(false);
    }
}
