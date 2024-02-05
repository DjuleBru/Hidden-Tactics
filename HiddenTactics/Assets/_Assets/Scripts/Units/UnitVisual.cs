using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitVisual : MonoBehaviour
{
    [SerializeField] protected Unit unit;
    [SerializeField] protected SpriteRenderer bodySpriteRenderer;


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
    }

    protected virtual void Start() {
        unit.OnUnitUpgraded += Unit_OnUnitUpgraded;
       
    }

    protected virtual void Unit_OnUnitUpgraded(object sender, System.EventArgs e) {
        bodySpriteRenderer.material = upgradedBodyMaterial;

        if (upgradeReplacesBody)
        {
            activeBodyAnimator = upgradedBodyAnimator;
            bodyAnimator.runtimeAnimatorController = upgradedBodyAnimator;
        }
    }

   
}
