using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UnitVisual : NetworkBehaviour
{
    [SerializeField] protected Unit unit;
    [SerializeField] protected List<SpriteRenderer> allVisualsSpriteRendererList;
    [SerializeField] protected SpriteRenderer baseOutlineSpriteRenderer;
    [SerializeField] protected SpriteRenderer baseCircleSpriteRenderer;
    [SerializeField] protected SpriteRenderer selectedUnitSpriteRenderer;
    [SerializeField] protected List<GameObject> shadowGameObjectList;
    [SerializeField] protected SpriteRenderer shadowBaseSpriteRenderer;

    [SerializeField] protected Material cleanMaterial;
    [SerializeField] protected Material invisibleMaterial;
    [SerializeField] protected Material placingUnitMaterial;

    [SerializeField] protected ParticleSystem unitGoldBurstPS;
    [SerializeField] protected ParticleSystem unitPlacedPS;
    [SerializeField] protected GameObject unitSelectedGameObject;
    [SerializeField] protected Image unitSelectedImage;

    [FoldoutGroup("Visual Components")]
    protected Animator bodyAnimator;
    protected RuntimeAnimatorController activeBodyAnimator;

    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] protected Material upgradedBodyMaterial;
    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] protected bool upgradeReplacesBody;

    [FoldoutGroup("Upgrade visual attributes"), ShowIf("upgradeReplacesBody"), ShowIf("upgradeReplacesBody")]
    [SerializeField] protected AnimatorOverrideController upgradedBodyAnimator;

    [SerializeField] protected TrailRenderer trailRenderer;

    public event EventHandler OnUnitVisualPlacingMaterialSet;

    [SerializeField] protected GameObject selectedVisual;
    [SerializeField] protected GameObject statusEffectVisuals;
    protected UnitAnimatorManager unitAnimatorManager;
    protected float baseUnitScale;

    protected virtual void Awake() {

        baseOutlineSpriteRenderer.enabled = false;
        baseCircleSpriteRenderer.enabled = false;
        selectedUnitSpriteRenderer.enabled = false;
        unitSelectedGameObject.SetActive(false);
        baseUnitScale = shadowBaseSpriteRenderer.transform.localScale.x;

        if (unit.GetUnitSO().isInvisibleGarrisonedUnit) return;

        unitAnimatorManager = GetComponent<UnitAnimatorManager>();
        bodyAnimator = GetComponent<Animator>();
        activeBodyAnimator = bodyAnimator.runtimeAnimatorController;
    
        if(trailRenderer != null ) {
            DisableTrailRenderer();
        }

        if (BattleManager.Instance == null) return;
        if (SettingsManager.Instance.GetTacticalViewSetting()) {
            SetUnitCircleGameObjectsActive(false);
        }
    }

    public override void OnNetworkSpawn() {
        if (unit.GetUnitSO().isInvisibleGarrisonedUnit) return;

        unit.OnUnitUpgraded += Unit_OnUnitUpgraded;
        unit.OnAdditionalUnitActivated += Unit_OnAdditionalUnitActivated;
        unit.OnUnitDynamicallySpawned += Unit_OnUnitDynamicallySpawned;
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        unit.OnUnitSetAsAdditionalUnit += Unit_OnUnitSetAsAdditionalUnit;
        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitFell += Unit_OnUnitFell;
        unit.OnUnitReset += Unit_OnUnitReset;
        unit.OnUnitHovered += Unit_OnUnitHovered;
        unit.OnUnitUnhovered += Unit_OnUnitUnhovered;
        unit.OnUnitSelectedFromTroop += Unit_OnUnitSelected;
        unit.OnSingleUnitSelected += Unit_OnSingleUnitSelected;
        unit.OnUnitUnselected += Unit_OnUnitUnselected;
        unit.OnUnitSold += Unit_OnUnitSold;

        unitAnimatorManager.OnUnitXChanged += UnitAnimatorManager_OnUnitXChanged;

        if(!unit.GetUnitIsOnlyVisual()) {
            SettingsManager.Instance.OnTacticalViewEnabled += SettingsManager_OnTacticalViewEnabled;
            SettingsManager.Instance.OnTacticalViewDisabled += SettingsManager_OnTacticalViewDisabled;
            SettingsManager.Instance.OnShowTacticalIconsDisabled += SettingsManager_OnShowTacticalIconsDisabled;
            SettingsManager.Instance.OnShowTacticalIconsEnabled += SettingsManager_OnShowTacticalIconsEnabled;
        } else {
            return;
        }

    }

    protected virtual void Start() {
        if (unit.GetUnitSO().isInvisibleGarrisonedUnit) {
            selectedVisual.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        if (!unit.GetUnitIsOnlyVisual()) {
            if (!unit.IsOwnedByPlayer()) return;

            if(!unit.GetUnitIsDynamicallySpawnedUnit()) {
                // Unit is a basic unit

                if(SettingsManager.Instance.GetTacticalViewSetting()) {

                    ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, invisibleMaterial); 
                    foreach (GameObject shadowGameObject in shadowGameObjectList) {
                        shadowGameObject.SetActive(false);
                    }

                } else {
                    ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, placingUnitMaterial);
                }

                OnUnitVisualPlacingMaterialSet?.Invoke(this, EventArgs.Empty);
            }
        } else
        // Unit is only visual : main menu, etc.
        {
            ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
        }
    }
    
    private void SettingsManager_OnTacticalViewDisabled(object sender, EventArgs e) {
        SetUnitCircleGameObjectsActive(true);

        if (unit.GetUnitIsPlaced()) {

            ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);

        } else {

            if (!unit.IsOwnedByPlayer() || unit.GetUnitIsBought()) return;
            if (unit.GetUnitIsDynamicallySpawnedUnit()) return;

            ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, placingUnitMaterial);
        }

        foreach (GameObject shadowGameObject in shadowGameObjectList) {
            shadowGameObject.SetActive(true);
        }
    }

    private void SettingsManager_OnTacticalViewEnabled(object sender, EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, invisibleMaterial);
        SetUnitCircleGameObjectsActive(false);
        foreach (GameObject shadowGameObject in shadowGameObjectList) {
            shadowGameObject.SetActive(false);
        }
    }

    private void SettingsManager_OnShowTacticalIconsEnabled(object sender, EventArgs e) {

        if (SettingsManager.Instance.GetTacticalViewSetting()) {
            ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, invisibleMaterial);
            SetUnitCircleGameObjectsActive(false);
            foreach (GameObject shadowGameObject in shadowGameObjectList) {
                shadowGameObject.SetActive(false);
            }
        };

    }

    private void SettingsManager_OnShowTacticalIconsDisabled(object sender, EventArgs e) {
        if (SettingsManager.Instance.GetTacticalViewSetting()) {
            SetUnitCircleGameObjectsActive(true);
            if (unit.GetUnitIsPlaced()) {

                ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
            }
            else {

                if (!unit.IsOwnedByPlayer() || unit.GetUnitIsBought()) return;
                if (unit.GetUnitIsDynamicallySpawnedUnit()) return;

                ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, placingUnitMaterial);
            }
            foreach (GameObject shadowGameObject in shadowGameObjectList) {
                shadowGameObject.SetActive(true);
            }
        };
        
    }

    private void UnitAnimatorManager_OnUnitXChanged(object sender, EventArgs e) {
        if (unitAnimatorManager.GetX() < 0) {
            selectedVisual.transform.localScale = new Vector3(-1, 1, 1) * baseUnitScale;
            statusEffectVisuals.transform.localScale = new Vector3(-1, 1, 1) * baseUnitScale;
            shadowBaseSpriteRenderer.transform.localScale = new Vector3(-1, 1, 1) * baseUnitScale;
        }
        else {
            selectedVisual.transform.localScale = new Vector3(1, 1, 1) * baseUnitScale;
            statusEffectVisuals.transform.localScale = new Vector3(1, 1, 1) * baseUnitScale;
            shadowBaseSpriteRenderer.transform.localScale = new Vector3(1, 1, 1) * baseUnitScale;
        }
    }

    private void Unit_OnUnitReset(object sender, System.EventArgs e) {
        ResetUnitVisuals();
        
    }

    private void Unit_OnUnitDied(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
        ChangeSpriteRendererListSortingOrder(allVisualsSpriteRendererList, -10);
        shadowBaseSpriteRenderer.gameObject.SetActive(false);
    }

    protected virtual void Unit_OnUnitUpgraded(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, upgradedBodyMaterial);

        if (upgradeReplacesBody)
        {
            activeBodyAnimator = upgradedBodyAnimator;
            bodyAnimator.runtimeAnimatorController = upgradedBodyAnimator;
        }
    }

    protected void Unit_OnAdditionalUnitActivated(object sender, System.EventArgs e) {
        if (SettingsManager.Instance.GetTacticalViewSetting()) return;
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
        SetFactionVisualColor();

        //Activate shadows
        foreach (GameObject shadowGameObject in shadowGameObjectList) {
            shadowGameObject.SetActive(true);
        }
    }

    private void Unit_OnUnitDynamicallySpawned(object sender, EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
        SetFactionVisualColor();

        //Activate shadows
        foreach (GameObject shadowGameObject in shadowGameObjectList) {
            shadowGameObject.SetActive(true);
        }
    }

    protected virtual void Unit_OnUnitPlaced(object sender, System.EventArgs e) {
        SetFactionVisualColor();

        if (SettingsManager.Instance.GetTacticalViewSetting()) return;
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
    }

    protected virtual void Unit_OnUnitSetAsAdditionalUnit(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, invisibleMaterial);
        foreach(GameObject shadowGameObject in shadowGameObjectList) {
            shadowGameObject.SetActive(false);
        }
    }

    private void Unit_OnUnitFell(object sender, System.EventArgs e) {
        unitGoldBurstPS.Play();
        unitGoldBurstPS.Stop();
        ParticleSystem.Burst burst = new ParticleSystem.Burst(0, unit.GetUnitSO().damageToVillages * PlayerGoldManager.Instance.GetPlayerUnitJumpedBonusGold());
        unitGoldBurstPS.emission.SetBurst(0, burst);
        unitGoldBurstPS.Play();

    }

    private void Unit_OnUnitSold(object sender, EventArgs e) {
        SetUnitCircleGameObjectsActive(false);
        gameObject.SetActive(false);
    }

    private void SetFactionVisualColor() {
        FactionSO deckFactionSO = DeckManager.LocalInstance.GetDeckSelected().deckFactionSO;
        ParticleSystem.MainModule ma = unitPlacedPS.main;
        ma.startColor = deckFactionSO.color_differentPlayerFaction_fill;

        if (!unit.IsOwnedByPlayer()) {
            PlayerCustomizationData opponentCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData();
            deckFactionSO = PlayerCustomizationDataManager.Instance.GetFactionSOFromId(opponentCustomizationData.factionID);
        }

        if (!HiddenTacticsMultiplayer.Instance.GetPlayerAndOpponentSameFaction()) {
            baseOutlineSpriteRenderer.color = deckFactionSO.color_differentPlayerFaction_outline;
            baseCircleSpriteRenderer.color = deckFactionSO.color_differentPlayerFaction_fill;
            selectedUnitSpriteRenderer.color = deckFactionSO.color_differentPlayerFaction_outline;
        }

        else {
            if (unit.IsOwnedByPlayer()) {
                baseOutlineSpriteRenderer.color = deckFactionSO.color_differentPlayerFaction_outline;
                selectedUnitSpriteRenderer.color = deckFactionSO.color_differentPlayerFaction_outline;
                baseCircleSpriteRenderer.color = deckFactionSO.color_differentPlayerFaction_fill;
            }
            else {
                baseOutlineSpriteRenderer.color = deckFactionSO.color_samePlayerFaction_Opponent_outline;
                selectedUnitSpriteRenderer.color = deckFactionSO.color_samePlayerFaction_Opponent_outline;
                baseCircleSpriteRenderer.color = deckFactionSO.color_samePlayerFaction_Opponent_fill;
            }
        }
    }

    protected virtual void ChangeSpriteRendererListMaterial(List<SpriteRenderer> spriteRendererList, Material material) {
        foreach (SpriteRenderer spriteRenderer in spriteRendererList) {

            spriteRenderer.material = material;
        }
    }

    protected virtual void ChangeSpriteRendererListSortingOrder(List<SpriteRenderer> spriteRendererList, int sortingOrder) {
        foreach (SpriteRenderer spriteRenderer in spriteRendererList) {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    public virtual void ChangeAllSpriteRendererListSortingOrder(int sortingLayerId, int sortingOrder)
    {
        foreach (SpriteRenderer spriteRenderer in allVisualsSpriteRendererList)
        {
            spriteRenderer.sortingOrder = sortingOrder;
            spriteRenderer.sortingLayerID = sortingLayerId;
        }

        foreach (GameObject gameObject in shadowGameObjectList)
        {
            SpriteRenderer shadowSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if(shadowSpriteRenderer != null)
            {
                gameObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder - 1;
                gameObject.GetComponent<SpriteRenderer>().sortingLayerID = sortingLayerId;
            }
        }
    }

    public void DeActivateStylizedShadows()
    {
        foreach(GameObject shadowGameObject in shadowGameObjectList)
        {
            if(shadowGameObject.name != "ShadowBase")
            {
                shadowGameObject.SetActive(false);
            }
        }
    }

    private void Unit_OnUnitUnselected(object sender, EventArgs e) {
        if (unit.GetUnitSO().isInvisibleGarrisonedUnit) return;
        SetUnitSelected(false, false);
    }

    private void Unit_OnUnitSelected(object sender, EventArgs e) {
        if (unit.GetUnitSO().isInvisibleGarrisonedUnit) return;
        SetUnitSelected(true, false);
    }

    private void Unit_OnSingleUnitSelected(object sender, EventArgs e) {
        if (unit.GetUnitSO().isInvisibleGarrisonedUnit) return;
        SetUnitSelected(true, true);
    }

    private void Unit_OnUnitUnhovered(object sender, EventArgs e) {
        if (unit.GetUnitSO().isInvisibleGarrisonedUnit) return;
        SetUnitHovered(false);
    }

    private void Unit_OnUnitHovered(object sender, EventArgs e) {
        if (unit.GetUnitSO().isInvisibleGarrisonedUnit) return;
        if (SettingsManager.Instance.GetTacticalViewSetting()) return;
        SetUnitHovered(true);
    }

    public void SetUnitHovered(bool hovered) {
        if (!unit.GetUnitIsBought()) return;

        if (hovered) {
            baseOutlineSpriteRenderer.enabled = true;
            baseCircleSpriteRenderer.enabled = true;
            selectedUnitSpriteRenderer.enabled = false;
            shadowBaseSpriteRenderer.enabled = false;
        } else {
            baseOutlineSpriteRenderer.enabled = false;
            baseCircleSpriteRenderer.enabled = false;
            selectedUnitSpriteRenderer.enabled = false;
            if (unit.GetIsDead()) return;
            shadowBaseSpriteRenderer.enabled = true;
        }
    }

    public void SetUnitSelected(bool selected, bool showSingleUnit) {
        if (!unit.GetUnitIsBought()) return;

        if (selected) {
            baseOutlineSpriteRenderer.enabled = true;
            baseCircleSpriteRenderer.enabled = true;
            selectedUnitSpriteRenderer.enabled = true;
            shadowBaseSpriteRenderer.enabled = false;

            if(showSingleUnit) {
                unitSelectedGameObject.SetActive(true);
            }
        } else {
            baseOutlineSpriteRenderer.enabled = false;
            baseCircleSpriteRenderer.enabled = false;
            selectedUnitSpriteRenderer.enabled = false;
            shadowBaseSpriteRenderer.enabled = true;

            unitSelectedGameObject.SetActive(false);
        }
    }

    public void ShowAsAdditionalUnitToBuy() {
        SetFactionVisualColor();
        gameObject.SetActive(true);
        baseOutlineSpriteRenderer.enabled = true;
        baseCircleSpriteRenderer.enabled = true;
        selectedUnitSpriteRenderer.enabled = false;
        GetComponent<UnitAnimatorManager>().SetUnitWatchDirectionBasedOnPlayerOwnance();
    }

    public void HideAsAdditionalUnitToBuy() {
        gameObject.SetActive(false);
        baseOutlineSpriteRenderer.enabled = false;
        baseCircleSpriteRenderer.enabled = false;
        selectedUnitSpriteRenderer.enabled = false;
    }

    public void ResetUnitVisuals() {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
        ChangeSpriteRendererListSortingOrder(allVisualsSpriteRendererList, 0);

        foreach (GameObject shadowGameObject in shadowGameObjectList) {
            shadowGameObject.SetActive(true);
        }
    }

    public void EnableTrailRenderer() {
        trailRenderer.enabled = true;
    }

    public void DisableTrailRenderer() {
        trailRenderer.enabled = false;
    }

    private void SetUnitCircleGameObjectsActive(bool active) {
        baseOutlineSpriteRenderer.gameObject.SetActive(active);
        baseCircleSpriteRenderer.gameObject.SetActive(active);
        selectedUnitSpriteRenderer.gameObject.SetActive(active);
        shadowBaseSpriteRenderer.gameObject.SetActive(active);
    }

    public override void OnDestroy() {
        if (unit.GetUnitIsOnlyVisual()) return;
        SettingsManager.Instance.OnTacticalViewEnabled -= SettingsManager_OnTacticalViewEnabled;
        SettingsManager.Instance.OnTacticalViewDisabled -= SettingsManager_OnTacticalViewDisabled;
        SettingsManager.Instance.OnShowTacticalIconsDisabled -= SettingsManager_OnShowTacticalIconsDisabled;
        SettingsManager.Instance.OnShowTacticalIconsEnabled -= SettingsManager_OnShowTacticalIconsEnabled;
    }

}
