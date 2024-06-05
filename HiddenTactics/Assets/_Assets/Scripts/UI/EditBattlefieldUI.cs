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

    private void Awake() {
        battlefieldCustomizationUI.SetActive(false);
        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualGridTemplate.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);
        battlefieldVisualBaseTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckChanged += DeckManager_OnDeckChanged;
    }

    private void DeckManager_OnDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshBattlefieldVisualGridContainer(e.selectedDeck.deckFactionSO);
    }

    public void StartEditBattlefieldGridTiles() {
        StartEditBattlefield();

        battlefieldVisualGridContainer.gameObject.SetActive(true);
        battlefieldVisualBaseContainer.gameObject.SetActive(false);

        RefreshBattlefieldVisualGridContainer(DeckManager.LocalInstance.GetDeckSelected().deckFactionSO);

        MainMenuCameraManager.Instance.SetEditBattlefieldGridTilesCamera();
    }

    public void StartEditBattlefieldVillages() {
        StartEditBattlefield();
        MainMenuCameraManager.Instance.SetEditBattlefieldVillagesCamera();
    }

    public void StartEditBattlefieldBase() {
        StartEditBattlefield();

        battlefieldVisualGridContainer.gameObject.SetActive(false);
        battlefieldVisualBaseContainer.gameObject.SetActive(true);

        RefreshBattlefieldVisualBaseContainer();

        MainMenuCameraManager.Instance.SetBaseCamera();
    }

    private void RefreshBattlefieldVisualGridContainer(FactionSO factionSO) {
        battlefieldVisualGridTemplate.gameObject.SetActive(true);

        foreach (Transform child in battlefieldVisualGridContainer) {
            if (child == battlefieldVisualGridTemplate) continue;
            Destroy(child.gameObject);
        }

        int id = 0;
        foreach(GridTileVisualSO tileSO in PlayerCustomizationData.Instance.GetGridTileVisualSOList()) {
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

        foreach (Sprite battlefieldBaseSprite in PlayerCustomizationData.Instance.GetBattlefieldBaseSpriteList()) {
            Transform battlefieldBaseVisualInstantiated = Instantiate(battlefieldVisualBaseTemplate, battlefieldVisualBaseContainer);
            battlefieldBaseVisualInstantiated.GetComponent<BattlefieldVisualBaseTemplate>().SetBattlefieldBaseImage(battlefieldBaseSprite);
        }

        battlefieldVisualBaseTemplate.gameObject.SetActive(false);
    }

    private void StartEditBattlefield() {
        battlefieldCustomizationUI.SetActive(true);
        DeckVisualUI.Instance.gameObject.SetActive(false);
    }

    public void StopEditBattlefield() {
        battlefieldCustomizationUI.SetActive(false);
        DeckVisualUI.Instance.gameObject.SetActive(true);
        MainMenuCameraManager.Instance.SetBaseCamera();
    }
}
