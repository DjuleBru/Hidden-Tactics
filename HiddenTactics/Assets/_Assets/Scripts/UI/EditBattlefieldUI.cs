using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditBattlefieldUI : MonoBehaviour
{
    [SerializeField] private GameObject battlefieldCustomizationUI;

    [SerializeField] private Transform battlefieldVisualGridContainer;
    [SerializeField] private Transform battlefieldVisualGridTemplate;

    [SerializeField] private Transform battlefieldVisualBaseContainer;
    [SerializeField] private Transform battlefieldVisualBaseTemplate;

    [SerializeField] private Transform battlefieldVillageSpritesContainer;
    [SerializeField] private Transform battlefieldVillageSpriteTemplate;

    private bool editingVillages;
    private bool editingGridTiles;

    private void Awake() {
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

    }

    public void StartEditBattlefieldGridTiles() {
        editingGridTiles = true;
        editingVillages = false;
        StartEditBattlefield();

        battlefieldVisualGridContainer.gameObject.SetActive(true);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(false);

        RefreshBattlefieldVisualGridContainer(DeckManager.LocalInstance.GetDeckSelected().deckFactionSO);

        MainMenuCameraManager.Instance.SetEditBattlefieldGridTilesCamera();
    }

    public void StartEditBattlefieldVillages() {
        editingVillages = true;
        editingGridTiles = false;
        StartEditBattlefield();

        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVillageSpritesContainer.gameObject.SetActive(true);

        RefreshBattlefieldVillageSpritesContainer(DeckManager.LocalInstance.GetDeckSelected());

        MainMenuCameraManager.Instance.SetEditBattlefieldVillagesCamera();
    }

    public void StartEditBattlefieldBase() {
        StartEditBattlefield();

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

    private void StartEditBattlefield() {
        battlefieldCustomizationUI.SetActive(true);
        DeckVisualUI.Instance.gameObject.SetActive(false);
    }

    public void StopEditBattlefield() {
        editingVillages = false;
        editingGridTiles = false;
        battlefieldCustomizationUI.SetActive(false);
        DeckVisualUI.Instance.gameObject.SetActive(true);
        MainMenuCameraManager.Instance.SetBaseCamera();
    }
}
