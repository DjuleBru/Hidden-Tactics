using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitVisual : NetworkBehaviour
{
    [SerializeField] protected Unit unit;
    [SerializeField] protected SpriteRenderer bodySpriteRenderer;
    [SerializeField] protected GameObject shadowGameObject;

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

        bodySpriteRenderer.material = placingUnitMaterial;
    }

    public override void OnNetworkSpawn() {
        unit.OnUnitUpgraded += Unit_OnUnitUpgraded;
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        unit.OnUnitSetAsAdditionalUnit += Unit_OnUnitSetAsAdditionalUnit;
    }

    protected virtual void Unit_OnUnitUpgraded(object sender, System.EventArgs e) {
        if (!unit.UnitIsBought()) return;
        bodySpriteRenderer.material = upgradedBodyMaterial;

        if (upgradeReplacesBody)
        {
            activeBodyAnimator = upgradedBodyAnimator;
            bodyAnimator.runtimeAnimatorController = upgradedBodyAnimator;
        }
    }

    protected void Unit_OnUnitPlaced(object sender, System.EventArgs e) {
        if (!unit.UnitIsBought()) return;

        bodySpriteRenderer.material = cleanMaterial;
    }

    protected void Unit_OnUnitSetAsAdditionalUnit(object sender, System.EventArgs e) {
        bodySpriteRenderer.material = invisibleMaterial;
        shadowGameObject.SetActive(false);
    }

}
