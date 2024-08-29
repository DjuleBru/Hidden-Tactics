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

    private Animator editBattlefieldAnimator;

    [SerializeField] private Button editBattlefieldGridTiles;
    [SerializeField] private Button editBattlefieldBase;
    [SerializeField] private Button editBattlefieldVillages;

    [SerializeField] private Image tileSlotOutlineImage;
    [SerializeField] private Image villageSlotOutlineImage;
    [SerializeField] private Image battlefieldSlotOutlineImage;
    [SerializeField] private Image tileSlotOutlineShadowImage;
    [SerializeField] private Image villageSlotOutlineShadowImage;
    [SerializeField] private Image battlefieldSlotOutlineShadowImage;

    private bool editingVillages;
    private bool editingGridTiles;

    private void Awake() {
        Instance = this;

        editBattlefieldAnimator = GetComponent<Animator>();

        battlefieldCustomizationUI.SetActive(false);
        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualGridTemplate.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVisualBaseTemplate.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(false);
        battlefieldVillageSpriteTemplate.gameObject.SetActive(false);

        editBattlefieldGridTiles.onClick.AddListener(() => {
            StartEditBattlefieldGridTiles();
        });
        editBattlefieldBase.onClick.AddListener(() => {
            StartEditBattlefieldBase();
        });
        editBattlefieldVillages.onClick.AddListener(() => {
            StartEditBattlefieldVillages();
        });
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;

        editingGridTiles = true;
        battlefieldVisualGridContainer.gameObject.SetActive(true);
        RefreshBattlefieldVisualGridContainer(DeckManager.LocalInstance.GetDeckSelected().deckFactionSO);

        RefreshSlotVisuals(DeckManager.LocalInstance.GetDeckSelected());
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshSlotVisuals(e.selectedDeck);

        if (editingVillages) {
            RefreshBattlefieldVillageSpritesContainer(e.selectedDeck);
        }

        if(editingGridTiles) {
            RefreshBattlefieldVisualGridContainer(e.selectedDeck.deckFactionSO);
        }
    }
    private void RefreshSlotVisuals(Deck deck) {

        tileSlotOutlineImage.sprite = deck.deckFactionSO.slotBorder;
        villageSlotOutlineImage.sprite = deck.deckFactionSO.slotBorder;
        battlefieldSlotOutlineImage.sprite = deck.deckFactionSO.slotBorder;
        tileSlotOutlineShadowImage.sprite = deck.deckFactionSO.slotBorder;
        villageSlotOutlineShadowImage.sprite = deck.deckFactionSO.slotBorder;
        battlefieldSlotOutlineShadowImage.sprite = deck.deckFactionSO.slotBorder;
    }

    public void StartEditBattlefieldGridTiles() {
        editingGridTiles = true;
        editingVillages = false;

        battlefieldVisualGridContainer.gameObject.SetActive(true);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(false);

        RefreshBattlefieldVisualGridContainer(DeckManager.LocalInstance.GetDeckSelected().deckFactionSO);

        //MainMenuCameraManager.Instance.SetEditBattlefieldGridTilesCamera();
    }

    public void StartEditBattlefieldVillages() {
        editingVillages = true;
        editingGridTiles = false;

        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(true);

        RefreshBattlefieldVillageSpritesContainer(DeckManager.LocalInstance.GetDeckSelected());

        //MainMenuCameraManager.Instance.SetEditBattlefieldVillagesCamera();
    }

    public void StartEditBattlefieldBase() {

        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(true);
        battlefieldVillageSpritesContainer.gameObject.SetActive(false);

        RefreshBattlefieldVisualBaseContainer();

        //MainMenuCameraManager.Instance.SetEditBattlefieldBaseCamera();
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

    public void SwitchToEditBattlefield() {
        StartCoroutine(SwitchToEditBattlefieldCoroutine(.5f, .5f));

    }

    private IEnumerator SwitchToEditBattlefieldCoroutine(float delayToMoveBattlefield, float delayToOpenUI) {
        MainMenuCameraManager.Instance.SetEditBattlefieldCamera();
        MainMenuUI.Instance.MainMenuUIToEditBattlefieldTransition();
        DeckVisualWorldUI.Instance.SetDeckVisualFlyDownDown();
        DeckVisualWorldUI.Instance.DisableEditDeckButton();

        yield return new WaitForSeconds(delayToMoveBattlefield);

        BattlefieldVisual.Instance.MoveBattlefieldToEdit();

        yield return new WaitForSeconds(delayToOpenUI);

        editBattlefieldAnimator.SetTrigger("Open");

        battlefieldCustomizationUI.SetActive(true);
    }

    public void SwitchToMainMenu() {
        editingVillages = false;
        editingGridTiles = false;
        battlefieldCustomizationUI.SetActive(false);
        PlayerCustomizationUI.Instance.gameObject.SetActive(true);
        MainMenuCameraManager.Instance.SetBaseCamera();

        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(false);
    }
}
