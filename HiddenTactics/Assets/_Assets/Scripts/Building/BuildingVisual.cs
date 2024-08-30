using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildingVisual : NetworkBehaviour
{
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material placingMaterial;
    [SerializeField] private Material invisibleMaterial;
    [SerializeField] private Material shadowMaterial;
    [SerializeField] private Animator buildingGeneralAnimator;

    [SerializeField] List<SpriteRenderer> buildingSpriteRendererList;
    [SerializeField] List<SpriteRenderer> shadowSpriteRendererList;

    [SerializeField] GameObject level1VisualGameObject;
    [SerializeField] GameObject level2VisualGameObject;

    [SerializeField] GameObject deckSlotVisualsGameObject;
    [SerializeField] List<GameObject> NPCGameObjectsToDeactivateForDeckSlot;
    [SerializeField] List<SpriteRenderer> deckSlotBackSpriteRendererList;
    [SerializeField] List<SpriteRenderer> deckSlotFrontSpriteRendererList;
    [SerializeField] List<SpriteRenderer> deckSlotShadowSpriteRendererList;
    [SerializeField] List<SpriteRenderer> deckSlotUnitsSpriteRendererList;
    [SerializeField] List<SpriteRenderer> deckSlotUnitsShadowSpriteRendererList;
    [SerializeField] List<SpriteRenderer> deckSlotUnitsWeaponsSpriteRendererList;
    [SerializeField] List<Animator> deckSlotUnitsAnimatorList;

    private Color hoveredColor;
    private Color selectedColor;
    private Color shadowColor;

    private Building building;

    private void Awake() {
        building = GetComponentInParent<Building>();
        shadowColor = shadowSpriteRendererList[0].color;
    }

    private void Start() {
        // Building is only visual
        if(building.GetBuildingIsOnlyVisual()) {
            SetDeckSlotVisualsActive(true);
            level1VisualGameObject.SetActive(false);
            level2VisualGameObject.SetActive(false);
        }
    }

    public override void OnNetworkSpawn() {
        building.OnBuildingPlaced += Building_OnBuildingPlaced;
        building.OnBuildingDestroyed += Building_OnBuildingDestroyed;
        building.OnBuildingSelected += Building_OnBuildingSelected;
        building.OnBuildingUnselected += Building_OnBuildingUnselected;
        building.OnBuildingHovered += Building_OnBuildingHovered;
        building.OnBuildingUnhovered += Building_OnBuildingUnhovered;
        building.OnBuildingSelled += Building_OnBuildingSelled;

        if (!building.GetBuildingIsOnlyVisual()) {

            SetDeckSlotVisualsActive(false);
            level1VisualGameObject.SetActive(true);
            level2VisualGameObject.SetActive(false);

            if (SettingsManager.Instance.GetTacticalViewSetting()) {
                ChangeSpriteRendererListMaterial(buildingSpriteRendererList, invisibleMaterial);
                ChangeSpriteRendererListMaterial(shadowSpriteRendererList, invisibleMaterial);
            }
            else {
                ChangeSpriteRendererListMaterial(buildingSpriteRendererList, placingMaterial);
                ChangeSpriteRendererListMaterial(shadowSpriteRendererList, shadowMaterial);
            }

            SettingsManager.Instance.OnTacticalViewEnabled += SettingsManager_OnTacticalViewEnabled;
            SettingsManager.Instance.OnTacticalViewDisabled += SettingsManager_OnTacticalViewDisabled;
            SettingsManager.Instance.OnShowTacticalIconsDisabled += SettingsManager_OnShowTacticalIconsDisabled;
            SettingsManager.Instance.OnShowTacticalIconsEnabled += SettingsManager_OnShowTacticalIconsEnabled;

        } 
    }

    private void Building_OnBuildingSelled(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }

    private void SettingsManager_OnShowTacticalIconsEnabled(object sender, System.EventArgs e) {
        if (SettingsManager.Instance.GetTacticalViewSetting()) {
            ChangeSpriteRendererListMaterial(buildingSpriteRendererList, invisibleMaterial);
            foreach (SpriteRenderer shadowSpriteRenderer in shadowSpriteRendererList) {
                shadowSpriteRenderer.gameObject.SetActive(false);
            }
        };
    }

    private void SettingsManager_OnShowTacticalIconsDisabled(object sender, System.EventArgs e) {
        if (SettingsManager.Instance.GetTacticalViewSetting()) {
            if (building.GetBuildingIsPlaced()) {

                ChangeSpriteRendererListMaterial(buildingSpriteRendererList, cleanMaterial);
                ChangeSpriteRendererListMaterial(shadowSpriteRendererList, shadowMaterial);

            }
            else {

                if (!building.IsOwnedByPlayer()) return;

                ChangeSpriteRendererListMaterial(buildingSpriteRendererList, placingMaterial);
            }
            foreach (SpriteRenderer shadowSpriteRenderer in shadowSpriteRendererList) {
                shadowSpriteRenderer.gameObject.SetActive(true);
            }
        };
    }

    private void SettingsManager_OnTacticalViewDisabled(object sender, System.EventArgs e) {
        if (building.GetBuildingIsPlaced()) {

            ChangeSpriteRendererListMaterial(buildingSpriteRendererList, cleanMaterial);
            ChangeSpriteRendererListMaterial(shadowSpriteRendererList, shadowMaterial);

        }
        else {

            if (!building.IsOwnedByPlayer()) return;

            ChangeSpriteRendererListMaterial(buildingSpriteRendererList, placingMaterial);
        }
        foreach (SpriteRenderer shadowSpriteRenderer in shadowSpriteRendererList) {
            shadowSpriteRenderer.gameObject.SetActive(true);
        }
    }

    private void SettingsManager_OnTacticalViewEnabled(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(buildingSpriteRendererList, invisibleMaterial);
        foreach (SpriteRenderer shadowSpriteRenderer in shadowSpriteRendererList) {
            shadowSpriteRenderer.gameObject.SetActive(false);
        }
    }

    private void Building_OnBuildingSelected(object sender, System.EventArgs e) {
        buildingGeneralAnimator.SetTrigger("BuildingPlaced");
        SetBuildingSelected(true);
    }

    private void Building_OnBuildingUnselected(object sender, System.EventArgs e) {
        SetBuildingSelected(false);
    }

    private void Building_OnBuildingUnhovered(object sender, System.EventArgs e) {
        if (SettingsManager.Instance.GetTacticalViewSetting()) return;
        SetBuildingHovered(false);
    }

    private void Building_OnBuildingHovered(object sender, System.EventArgs e) {
        if (SettingsManager.Instance.GetTacticalViewSetting()) return;
        SetBuildingHovered(true);
    }


    private void Building_OnBuildingDestroyed(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }

    private void Building_OnBuildingPlaced(object sender, System.EventArgs e) {
        if(!SettingsManager.Instance.GetTacticalViewSetting()) {
            ChangeSpriteRendererListMaterial(buildingSpriteRendererList, cleanMaterial);
        }

        SetFactionVisualColor();
        buildingGeneralAnimator.SetTrigger("BuildingPlaced");
    }

    protected virtual void ChangeSpriteRendererListMaterial(List<SpriteRenderer> spriteRendererList, Material material) {
        foreach (SpriteRenderer spriteRenderer in spriteRendererList) {

            spriteRenderer.material = material;
        }
    }

    public void SetBuildingHovered(bool hovered) {
        if(hovered) {
            foreach (SpriteRenderer shadowSpriteRenderer in shadowSpriteRendererList) {
                shadowSpriteRenderer.color = hoveredColor;
            }
        } else {
            foreach (SpriteRenderer shadowSpriteRenderer in shadowSpriteRendererList) {
                shadowSpriteRenderer.color = shadowColor;
            }
        }
        }

    public void SetBuildingSelected(bool selected) {
        if (selected) {
            foreach (SpriteRenderer shadowSpriteRenderer in shadowSpriteRendererList) {
                shadowSpriteRenderer.color = selectedColor;
            }
        }
        else {
            foreach (SpriteRenderer shadowSpriteRenderer in shadowSpriteRendererList) {
                shadowSpriteRenderer.color = shadowColor;
            }
        }
    }

    public void SetDeckSlotVisualsActive(bool active) {
        deckSlotVisualsGameObject.SetActive(active);

        foreach (SpriteRenderer spriteRenderer in buildingSpriteRendererList) {
            spriteRenderer.gameObject.SetActive(!active);
        }
        foreach (SpriteRenderer spriteRenderer in shadowSpriteRendererList) {
            spriteRenderer.gameObject.SetActive(!active);
        }
        foreach (GameObject gameObject in NPCGameObjectsToDeactivateForDeckSlot) {
            gameObject.SetActive(!active);
        }
        foreach (Animator unitAnimator in deckSlotUnitsAnimatorList) {
            unitAnimator.SetFloat("Y", -1); 
            float randomOffset = UnityEngine.Random.Range(0f, 1f);
            unitAnimator.Play("Idle", 0, randomOffset);
        }
    }

    public void SetBuildingDeckSlotSpriteSortingOrder(int sortOrder) {

        foreach (SpriteRenderer spriteRenderer in deckSlotBackSpriteRendererList) {
            spriteRenderer.sortingOrder = sortOrder +1;
        }
        foreach (SpriteRenderer spriteRenderer in deckSlotFrontSpriteRendererList) {
            spriteRenderer.sortingOrder = sortOrder +7;
        }
        foreach (SpriteRenderer spriteRenderer in deckSlotShadowSpriteRendererList) {
            spriteRenderer.sortingOrder = sortOrder +2 ;
        }
        foreach (SpriteRenderer spriteRenderer in deckSlotUnitsSpriteRendererList) {
            spriteRenderer.sortingOrder = sortOrder + 5;
        }
        foreach (SpriteRenderer spriteRenderer in deckSlotUnitsShadowSpriteRendererList) {
            spriteRenderer.sortingOrder = sortOrder + 4;
        }
        foreach (SpriteRenderer spriteRenderer in deckSlotUnitsWeaponsSpriteRendererList) {
            spriteRenderer.sortingOrder = sortOrder + 6;
        }
    }

    private void SetFactionVisualColor() {
        FactionSO deckFactionSO = DeckManager.LocalInstance.GetDeckSelected().deckFactionSO;

        if (!building.IsOwnedByPlayer()) {
            PlayerCustomizationData opponentCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData();
            deckFactionSO = PlayerCustomizationDataManager.Instance.GetFactionSOFromId(opponentCustomizationData.factionID);
        }

        if (!HiddenTacticsMultiplayer.Instance.GetPlayerAndOpponentSameFaction()) {
            hoveredColor = deckFactionSO.color_differentPlayerFaction_fill;
            hoveredColor.a = 115f;
            selectedColor = deckFactionSO.color_differentPlayerFaction_outline;
            selectedColor.a = 150f;
        }

        else {
            if (building.IsOwnedByPlayer()) {
                hoveredColor = deckFactionSO.color_differentPlayerFaction_fill;
                hoveredColor.a = 115f;
                selectedColor = deckFactionSO.color_differentPlayerFaction_outline;
                selectedColor.a = 150f;
            }
            else {
                hoveredColor = deckFactionSO.color_samePlayerFaction_Opponent_fill;
                hoveredColor.a = 115f;
                selectedColor = deckFactionSO.color_samePlayerFaction_Opponent_outline;
                selectedColor.a = 150f;
            }
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();
        SettingsManager.Instance.OnTacticalViewEnabled -= SettingsManager_OnTacticalViewEnabled;
        SettingsManager.Instance.OnTacticalViewDisabled -= SettingsManager_OnTacticalViewDisabled;
        SettingsManager.Instance.OnShowTacticalIconsDisabled -= SettingsManager_OnShowTacticalIconsDisabled;
        SettingsManager.Instance.OnShowTacticalIconsEnabled -= SettingsManager_OnShowTacticalIconsEnabled;
    }
}
