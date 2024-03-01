using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitVisual : NetworkBehaviour
{
    [SerializeField] protected Unit unit;
    [SerializeField] protected List<SpriteRenderer> bodySpriteRendererList;
    [SerializeField] protected List<GameObject> shadowGameObjectList;

    [SerializeField] protected Material cleanMaterial;
    [SerializeField] protected Material invisibleMaterial;
    [SerializeField] protected Material placingUnitMaterial;

    [FoldoutGroup("Visual Components")]
    protected Animator bodyAnimator;
    protected RuntimeAnimatorController activeBodyAnimator;

    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] protected Material upgradedBodyMaterial;
    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] protected bool upgradeReplacesBody;

    [FoldoutGroup("Upgrade visual attributes"), ShowIf("upgradeReplacesBody"), ShowIf("upgradeReplacesBody")]
    [SerializeField] protected AnimatorOverrideController upgradedBodyAnimator;

    protected virtual void Awake() {
        unit = GetComponentInParent<Unit>();
        bodyAnimator = GetComponent<Animator>();
        activeBodyAnimator = bodyAnimator.runtimeAnimatorController;

        ChangeSpriteRendererListMaterial(bodySpriteRendererList, placingUnitMaterial);
    }

    public override void OnNetworkSpawn() {
        unit.OnUnitUpgraded += Unit_OnUnitUpgraded;
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        unit.OnUnitSetAsAdditionalUnit += Unit_OnUnitSetAsAdditionalUnit;
    }

    protected virtual void Unit_OnUnitUpgraded(object sender, System.EventArgs e) {
        if (!unit.GetUnitIsBought()) return;
        ChangeSpriteRendererListMaterial(bodySpriteRendererList, upgradedBodyMaterial);

        if (upgradeReplacesBody)
        {
            activeBodyAnimator = upgradedBodyAnimator;
            bodyAnimator.runtimeAnimatorController = upgradedBodyAnimator;
        }
    }

    protected virtual void Unit_OnUnitPlaced(object sender, System.EventArgs e) {
        if (!unit.GetUnitIsBought()) return;

        ChangeSpriteRendererListMaterial(bodySpriteRendererList, cleanMaterial);
    }

    protected virtual void Unit_OnUnitSetAsAdditionalUnit(object sender, System.EventArgs e) {
        ChangeSpriteRendererListMaterial(bodySpriteRendererList, invisibleMaterial);
        foreach(GameObject shadowGameObject in shadowGameObjectList) {
            shadowGameObject.SetActive(false);
        }
    }

    protected virtual void ChangeSpriteRendererListMaterial(List<SpriteRenderer> spriteRendererList, Material material) {
        foreach(SpriteRenderer spriteRenderer in spriteRendererList) {
            spriteRenderer.material = material;
        }
    }

}
