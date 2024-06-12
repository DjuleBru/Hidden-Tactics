using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitVisual : NetworkBehaviour
{
    [SerializeField] protected Unit unit;
    [SerializeField] protected List<SpriteRenderer> allVisualsSpriteRendererList;
    [SerializeField] protected SpriteRenderer selectedUnitSpriteRenderer;
    [SerializeField] protected List<GameObject> shadowGameObjectList;

    [SerializeField] protected Material cleanMaterial;
    [SerializeField] protected Material invisibleMaterial;
    [SerializeField] protected Material placingUnitMaterial;

    [SerializeField] protected ParticleSystem unitGoldBurstPS;

    [FoldoutGroup("Visual Components")]
    protected Animator bodyAnimator;
    protected RuntimeAnimatorController activeBodyAnimator;

    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] protected Material upgradedBodyMaterial;
    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] protected bool upgradeReplacesBody;

    [FoldoutGroup("Upgrade visual attributes"), ShowIf("upgradeReplacesBody"), ShowIf("upgradeReplacesBody")]
    [SerializeField] protected AnimatorOverrideController upgradedBodyAnimator;

    public event EventHandler OnUnitVisualPlacingMaterialSet;

    protected virtual void Awake() {
        unit = GetComponentInParent<Unit>();
        bodyAnimator = GetComponent<Animator>();
        activeBodyAnimator = bodyAnimator.runtimeAnimatorController;
    
        selectedUnitSpriteRenderer.enabled = false;
    }

    public override void OnNetworkSpawn() {
        unit.OnUnitUpgraded += Unit_OnUnitUpgraded;
        unit.OnAdditionalUnitActivated += Unit_OnAdditionalUnitBought;
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        unit.OnUnitSetAsAdditionalUnit += Unit_OnUnitSetAsAdditionalUnit;
        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitFell += Unit_OnUnitFell;
        unit.OnUnitReset += Unit_OnUnitReset;
    }

    private void Unit_OnUnitFell(object sender, System.EventArgs e) {
        unitGoldBurstPS.Play();
        unitGoldBurstPS.Stop();
        ParticleSystem.Burst burst = new ParticleSystem.Burst(0, unit.GetUnitSO().damageToVillages * PlayerGoldManager.Instance.GetPlayerUnitJumpedBonusGold());
        unitGoldBurstPS.emission.SetBurst(0, burst);
        unitGoldBurstPS.Play();

    }

    protected virtual void Start() {
        if (!unit.GetUnitIsOnlyVisual())
        {
            ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, placingUnitMaterial);
            OnUnitVisualPlacingMaterialSet?.Invoke(this, EventArgs.Empty);
        } else
        {
            ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
        }
    }

    private void Unit_OnUnitReset(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
        ChangeSpriteRendererListSortingOrder(allVisualsSpriteRendererList, 0);
    }

    private void Unit_OnUnitDied(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
        ChangeSpriteRendererListSortingOrder(allVisualsSpriteRendererList, -10);
    }

    protected virtual void Unit_OnUnitUpgraded(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, upgradedBodyMaterial);

        if (upgradeReplacesBody)
        {
            activeBodyAnimator = upgradedBodyAnimator;
            bodyAnimator.runtimeAnimatorController = upgradedBodyAnimator;
        }
    }

    protected void Unit_OnAdditionalUnitBought(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);

        //Activate shadows
        foreach (GameObject shadowGameObject in shadowGameObjectList) {
            shadowGameObject.SetActive(true);
        }
    }

    protected virtual void Unit_OnUnitPlaced(object sender, System.EventArgs e) {
        Debug.Log("received placed event");
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, cleanMaterial);
    }

    protected virtual void Unit_OnUnitSetAsAdditionalUnit(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(allVisualsSpriteRendererList, invisibleMaterial);
        foreach(GameObject shadowGameObject in shadowGameObjectList) {
            shadowGameObject.SetActive(false);
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

    public void SetUnitHovered(bool hovered) {
        if (hovered) {
            selectedUnitSpriteRenderer.enabled = true;
            selectedUnitSpriteRenderer.material = placingUnitMaterial;
        } else {
            selectedUnitSpriteRenderer.enabled = false;
        }
    }

    public void SetUnitSelected(bool selected) {
        if (selected) {
            selectedUnitSpriteRenderer.enabled = true;
            selectedUnitSpriteRenderer.material = cleanMaterial;
        } else {
            selectedUnitSpriteRenderer.enabled = false;
        }
    }

}
