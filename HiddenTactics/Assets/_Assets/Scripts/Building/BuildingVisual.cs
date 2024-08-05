using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingVisual : MonoBehaviour
{
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material placingMaterial;
    [SerializeField] private Material invisibleMaterial;
    [SerializeField] private Material shadowMaterial;
    [SerializeField] private Animator buildingGeneralAnimator;

    [SerializeField] List<SpriteRenderer> buildingSpriteRendererList;
    [SerializeField] List<SpriteRenderer> shadowSpriteRendererList;

    private Building building;

    private void Awake() {
        building = GetComponentInParent<Building>();
    }

    private void Start() {
        building.OnBuildingPlaced += Building_OnBuildingPlaced;
        building.OnBuildingDestroyed += Building_OnBuildingDestroyed;
        building.OnBuildingSelected += Building_OnBuildingSelected;

        if (!building.GetBuildingIsOnlyVisual())
        {
            if(SettingsManager.Instance.GetTacticalViewSetting()) {
                ChangeSpriteRendererListMaterial(buildingSpriteRendererList, invisibleMaterial);
            } else {
                ChangeSpriteRendererListMaterial(buildingSpriteRendererList, placingMaterial);
            }

            SettingsManager.Instance.OnTacticalViewEnabled += SettingsManager_OnTacticalViewEnabled;
            SettingsManager.Instance.OnTacticalViewDisabled += SettingsManager_OnTacticalViewDisabled;
            SettingsManager.Instance.OnShowTacticalIconsDisabled += SettingsManager_OnShowTacticalIconsDisabled;
            SettingsManager.Instance.OnShowTacticalIconsEnabled += SettingsManager_OnShowTacticalIconsEnabled;
        }
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
    }

    private void Building_OnBuildingDestroyed(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }

    private void Building_OnBuildingPlaced(object sender, System.EventArgs e) {
        if(!SettingsManager.Instance.GetTacticalViewSetting()) {
            ChangeSpriteRendererListMaterial(buildingSpriteRendererList, cleanMaterial);
        }

        buildingGeneralAnimator.SetTrigger("BuildingPlaced");
    }

    protected virtual void ChangeSpriteRendererListMaterial(List<SpriteRenderer> spriteRendererList, Material material) {
        foreach (SpriteRenderer spriteRenderer in spriteRendererList) {

            spriteRenderer.material = material;
        }
    }

    public void OnDestroy() {
        SettingsManager.Instance.OnTacticalViewEnabled -= SettingsManager_OnTacticalViewEnabled;
        SettingsManager.Instance.OnTacticalViewDisabled -= SettingsManager_OnTacticalViewDisabled;
        SettingsManager.Instance.OnShowTacticalIconsDisabled -= SettingsManager_OnShowTacticalIconsDisabled;
        SettingsManager.Instance.OnShowTacticalIconsEnabled -= SettingsManager_OnShowTacticalIconsEnabled;
    }
}
