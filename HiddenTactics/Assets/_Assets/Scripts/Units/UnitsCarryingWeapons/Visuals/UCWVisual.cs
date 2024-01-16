using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UCWVisual : UnitVisual
{
    [FoldoutGroup("Visual Components")]
    [SerializeField] private Animator weaponAnimator;
    [FoldoutGroup("Visual Components")]
    [SerializeField] private WeaponVisual weaponVisual;

    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] private bool upgradeReplacesWeapon;
    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] private bool upgradeReplacesSideWeapon;
    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] private bool upgradeChangesWeaponScale;
    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] private bool upgradeChangesWeaponShader;
    [FoldoutGroup("Upgrade visual attributes")]
    [SerializeField] private bool upgradeChangesMountShader;

    [FoldoutGroup("Upgrade visual attributes"), ShowIf("upgradeReplacesWeapon")]
    [SerializeField] private WeaponSO ugradedWeaponSO;

    [FoldoutGroup("Upgrade visual attributes"), ShowIf("upgradeReplacesSideWeapon")]
    [SerializeField] private WeaponSO ugradedSideWeaponSO;

    [FoldoutGroup("Upgrade visual attributes"), ShowIf("upgradeChangesWeaponScale")]
    [SerializeField] private Vector3 upgradedWeaponScale;

    [FoldoutGroup("Upgrade visual attributes"), ShowIf("upgradeChangesWeaponShader")]
    [SerializeField] private Material upgradedWeaponMaterial;

    [FoldoutGroup("Upgrade visual attributes"), ShowIf("upgradeChangesMountShader")]
    [SerializeField] private SpriteRenderer mountSpriteRenderer;

    [FoldoutGroup("Upgrade visual attributes"), ShowIf("upgradeChangesMountShader")]
    [SerializeField] private Material ugradedMountShader;

    [FoldoutGroup("Weapon visual attributes")]
    [SerializeField] private bool hasSideWeapon;
    [FoldoutGroup("Weapon visual attributes"), ShowIf("hasSideWeapon")]
    [SerializeField] private AnimatorOverrideController bodySideWeaponAnimator;

    private UCW ucw;

    protected override void Awake() {
        ucw = GetComponentInParent<UCW>();
    }

    protected override void Start() {
        ucw.OnUnitUpgraded += Ucw_OnUnitUpgraded;
        ucw.OnMainWeaponActivated += Ucw_OnMainWeaponActivated;
        ucw.OnSideWeaponActivated += Ucw_OnSideWeaponActivated;
    }

    private void Ucw_OnSideWeaponActivated(object sender, EventArgs e) {
        bodyAnimator.runtimeAnimatorController = bodySideWeaponAnimator;
    }

    private void Ucw_OnMainWeaponActivated(object sender, EventArgs e) {
        bodyAnimator.runtimeAnimatorController = activeBodyAnimator;
    }

    private void Ucw_OnUnitUpgraded(object sender, EventArgs e) {
        bodySpriteRenderer.material = upgradedBodyMaterial;

        if(upgradeReplacesBody) {
            activeBodyAnimator = upgradedBodyAnimator;
            bodyAnimator.runtimeAnimatorController = upgradedBodyAnimator;
        }

        if(upgradeReplacesWeapon) {
            weaponVisual.ReplaceWeaponSO(ugradedWeaponSO);
            bodyAnimator.runtimeAnimatorController = upgradedBodyAnimator;
            weaponAnimator.runtimeAnimatorController = ugradedWeaponSO.weaponAnimator;
        }

        if (upgradeReplacesSideWeapon)
        {
            weaponVisual.ReplaceSideWeaponSO(ugradedSideWeaponSO);
        }

        if (upgradeChangesWeaponScale) {
            weaponVisual.SetWeaponScale(upgradedWeaponScale);
        }

        if(upgradeChangesWeaponShader) {
            weaponVisual.SetWeaponMaterial(upgradedWeaponMaterial);
        }

        if (upgradeChangesMountShader)
        {
            mountSpriteRenderer.material = ugradedMountShader;
        }

    }
}
