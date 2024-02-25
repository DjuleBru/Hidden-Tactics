using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;

public class WeaponVisual : NetworkBehaviour
{
    [FoldoutGroup("Base attributes")]
    [SerializeField] private Transform weaponHoldPoint;
    [FoldoutGroup("Base attributes")]
    [SerializeField] private Transform weaponSortingLayerTransform;
    [FoldoutGroup("Base attributes")]
    [SerializeField] private WeaponSO mainWeaponSO;
    [FoldoutGroup("Base attributes")]
    [SerializeField] private WeaponSO sideWeaponSO;
    [FoldoutGroup("Base attributes")]
    [SerializeField] private SpriteRenderer weaponMountedVisualSpriteRenderer;
    [FoldoutGroup("Base attributes")]
    [SerializeField] private Vector3 weaponScale = Vector3.one;
    [FoldoutGroup("Base attributes")]
    [SerializeField] private bool orthogonalAnimations;

    [FoldoutGroup("Legendary Weapon")]
    [SerializeField] private Animator legendaryWeaponAnimator;
    [FoldoutGroup("Legendary Weapon")]
    [SerializeField] private SpriteRenderer legendaryweaponVisualSpriteRenderer;
    [FoldoutGroup("Legendary Weapon")]
    [SerializeField] private Sprite IcebergBlade;
    [FoldoutGroup("Legendary Weapon")]
    [SerializeField] private Sprite ThunderboltSword;
    [FoldoutGroup("Legendary Weapon")]
    [SerializeField] private Sprite ViperScimitar;
    [FoldoutGroup("Legendary Weapon")]
    [SerializeField] private Sprite VolcanoMace;

    private WeaponSO activeWeaponSO;
    private Vector3 upgradedWeaponScale;
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material placingUnitMaterial;
    [SerializeField] private Material invisibleMaterial;
    private Material weaponMaterial;

    private UCW ucw;
    private UCWAnimatorManager ucwAnimatorManager;
    private SpriteRenderer weaponVisualSpriteRenderer;
    private Animator mainWeaponAnimator;
    private Animator activeWeaponAnimator;

    private float X;
    private float Y;

    private UCW.MagicState magicState;
    private UCW.LegendaryState legendaryState;
    private string magicStateTrigger;
    private string legendaryStateTrigger;

    private void Awake() {
        mainWeaponAnimator = GetComponent<Animator>();
        weaponVisualSpriteRenderer = GetComponent<SpriteRenderer>();
        ucw = GetComponentInParent<UCW>();
        ucwAnimatorManager = GetComponentInParent<UCWAnimatorManager>();

        weaponVisualSpriteRenderer.material = placingUnitMaterial;

        activeWeaponSO = mainWeaponSO;
        activeWeaponAnimator = mainWeaponAnimator;
        magicState = UCW.MagicState.Base;
        magicStateTrigger = "Base";
        weaponMaterial = cleanMaterial;
        upgradedWeaponScale = weaponScale;
    }

    public override void OnNetworkSpawn() {
        ucw.OnLegendaryStateChanged += Ucw_OnLegendaryStateChanged;
        ucw.OnMagicStateChanged += Ucw_OnMagicStateChanged;
        ucw.GetComponent<UnitAI>().OnSideAttackActivated += WeaponVisual_OnSideAttackActivated;
        ucw.GetComponent<UnitAI>().OnMainAttackActivated += WeaponVisual_OnMainAttackActivated;

        ucw.OnUnitPlaced += Ucw_OnUnitPlaced;
        ucw.OnUnitSetAsAdditionalUnit += Ucw_OnUnitSetAsAdditionalUnit;

        ucw.GetComponent<UnitAI>().OnStateChanged += UnitAI_OnStateChanged;
        ucwAnimatorManager.OnUcwAttack += UcwAnimatorManager_OnUcwAttack;
        ucwAnimatorManager.OnUcwAttackStart += UcwAnimatorManager_OnUcwAttackStart;
        ucwAnimatorManager.OnUcwAttackEnd += UcwAnimatorManager_OnUcwAttackEnd;
    }

    private void LateUpdate() {
        UpdateWeaponVisual();
    }

    private void UpdateWeaponVisual() {
        if(activeWeaponAnimator.runtimeAnimatorController == null) {
            return;
        }

        SetXY(ucwAnimatorManager.GetX(), ucwAnimatorManager.GetY());
        if (activeWeaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
            HandleWeaponIdleVisual();
        }
        else {
            // Weapon is attacking 
            HandleWeaponAttackingVisual();
        };
    }
    private void UcwAnimatorManager_OnUcwAttack(object sender, System.EventArgs e) {
        SetAttackTrigger();
    }

    private void UcwAnimatorManager_OnUcwAttackEnd(object sender, System.EventArgs e) {
        SetAttackEndTrigger();
    }

    private void UcwAnimatorManager_OnUcwAttackStart(object sender, System.EventArgs e) {
        SetAttackStartTrigger();
    }

    private void HandleWeaponIdleVisual() {
        weaponVisualSpriteRenderer.enabled = true;
        weaponSortingLayerTransform.localPosition = new Vector3(0,-0.1f,0);

        // Weapon is idle : Change weapon idle sprite
        if (legendaryState == UCW.LegendaryState.Base)
        {
            UpdateWeaponMagicVisualSprite(magicState);
        }
        else
        {
            UpdateWeaponLegendaryVisualSprite(legendaryState);
        }

        // keep weapon position on weapon hold point
        transform.position = weaponHoldPoint.position;

        // Check if X movement is negative : reverse sprite X to keep symetry
        if (X < 0)
        {
            transform.localScale = new Vector3(-weaponScale.x, weaponScale.y, weaponScale.z);
        }
        else
        {
            transform.localScale = new Vector3(weaponScale.x, weaponScale.y, weaponScale.z);
        }
    }

    private void HandleWeaponAttackingVisual() {
        // If legendary : disable base weapon sprite
        if (legendaryState != UCW.LegendaryState.Base)
        {
            weaponVisualSpriteRenderer.enabled = false;
        }

        // keep transform on unit pivot (except for cavalry)
        if (!ucw.GetIsMountedUnit()) {
            transform.localScale = new Vector3(weaponScale.x, weaponScale.y, weaponScale.z);
            transform.position = transform.parent.position;
        } else
        {
            transform.localScale = Vector3.one;
            weaponVisualSpriteRenderer.enabled = false;
        }

        // change sorting layer if Y > 0 (only works with legendary weapons)
        HandleWeaponSpriteSortingLayer();
    }

    private void HandleWeaponSpriteSortingLayer(){

        if(orthogonalAnimations)
        {
            if(Y > 0.5)
            {
                weaponSortingLayerTransform.localPosition = new Vector3(0, 0.1f, 0);
            }
        } else
        {
            if (Y > 0)
            {

                weaponSortingLayerTransform.localPosition = new Vector3(0, 0.1f, 0);
            }
        }

    }

    private void UpdateWeaponMagicVisual(UCW.MagicState magicState) {

        switch (magicState) {
            case UCW.MagicState.Base:
                magicStateTrigger = "Base";
            break;
            case UCW.MagicState.Fire:
                magicStateTrigger = "Fire";
                break;
            case UCW.MagicState.Ice:
                magicStateTrigger = "Ice";
                break;
            case UCW.MagicState.Poison:
                magicStateTrigger = "Poison";
                break;
            case UCW.MagicState.Shock:
                magicStateTrigger = "Shock";
                break;
            case UCW.MagicState.Fear:
                magicStateTrigger = "Fear";
                break;
            case UCW.MagicState.Bleed:
                magicStateTrigger = "Bleed";
                break;
        }
    }

    private void UpdateWeaponMagicVisualSprite(UCW.MagicState magicState) {
        UpdateWeaponMagicStateTrigger(magicState);
        switch (magicState) {
            case UCW.MagicState.Base:
                weaponVisualSpriteRenderer.sprite = activeWeaponSO.idleSprite;
                break;
            case UCW.MagicState.Fire:
                weaponVisualSpriteRenderer.sprite = activeWeaponSO.fireSprite;
                break;
            case UCW.MagicState.Ice:
                weaponVisualSpriteRenderer.sprite = activeWeaponSO.iceSprite;
                break;
            case UCW.MagicState.Poison:
                weaponVisualSpriteRenderer.sprite = activeWeaponSO.poisonSprite;
                break;
            case UCW.MagicState.Shock:
                weaponVisualSpriteRenderer.sprite = activeWeaponSO.shockSprite;
                break;
            case UCW.MagicState.Fear:
                weaponVisualSpriteRenderer.sprite = activeWeaponSO.fearSprite;
                break;
            case UCW.MagicState.Bleed:
                weaponVisualSpriteRenderer.sprite = activeWeaponSO.bleedSprite;
                break;
        }
    }

    private void UpdateWeaponMagicStateTrigger(UCW.MagicState magicState)
    {
        switch (magicState)
        {
            case UCW.MagicState.Base:
                magicStateTrigger = "Base";
                break;
            case UCW.MagicState.Fire:
                magicStateTrigger = "Fire";
                break;
            case UCW.MagicState.Ice:
                magicStateTrigger = "Ice";
                break;
            case UCW.MagicState.Poison:
                magicStateTrigger = "Poison";
                break;
            case UCW.MagicState.Shock:
                magicStateTrigger = "Shock";
                break;
            case UCW.MagicState.Fear:
                magicStateTrigger = "Fear";
                break;
            case UCW.MagicState.Bleed:
                magicStateTrigger = "Bleed";
                break;
        }
    }

    private void UpdateWeaponLegendaryVisualSprite(UCW.LegendaryState legendaryState) {
        switch (legendaryState) {
            case UCW.LegendaryState.Base:
                weaponVisualSpriteRenderer.sprite = mainWeaponSO.idleSprite;
                break;
            case UCW.LegendaryState.IcebergBlade:
                weaponVisualSpriteRenderer.sprite = IcebergBlade;
                break;
            case UCW.LegendaryState.ThunderboltSword:
                weaponVisualSpriteRenderer.sprite = ThunderboltSword;
                break;
            case UCW.LegendaryState.ViperScimitar:
                weaponVisualSpriteRenderer.sprite = ViperScimitar;
                break;
            case UCW.LegendaryState.VolcanoMace:
                weaponVisualSpriteRenderer.sprite = VolcanoMace;
                break;
        }
    }

    private void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        activeWeaponAnimator.ResetTrigger("InterruptAttack");

        if (!ucw.GetComponent<UnitAI>().IsAttacking()) {
            if(!activeWeaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                activeWeaponAnimator.SetTrigger("InterruptAttack");
            }
        }
    }

    protected void Ucw_OnUnitSetAsAdditionalUnit(object sender, System.EventArgs e) {
        weaponVisualSpriteRenderer.material = invisibleMaterial;
    }

    private void Ucw_OnUnitPlaced(object sender, System.EventArgs e) {
        if (!ucw.GetUnitIsBought()) return;

        weaponVisualSpriteRenderer.material = cleanMaterial;
    }

    private void Ucw_OnMagicStateChanged(object sender, System.EventArgs e) {
        magicState = ucw.GetMagicState();
        UpdateWeaponMagicVisual(magicState);
    }

    private void Ucw_OnLegendaryStateChanged(object sender, System.EventArgs e) {
        legendaryState = ucw.GetLegendaryState();
        UpdateLegendaryState();
    }

    private void WeaponVisual_OnMainAttackActivated(object sender, System.EventArgs e) {
        SetMainWeaponActive();
    }

    private void WeaponVisual_OnSideAttackActivated(object sender, System.EventArgs e) {
        SetSideWeaponActive();
    }

    public void SetXY(float xValue, float yValue) {
        X = xValue; 
        Y = yValue;

        if(activeWeaponAnimator.runtimeAnimatorController == null) {
            return;
        }

        activeWeaponAnimator.SetFloat("X", xValue);
        activeWeaponAnimator.SetFloat("Y", yValue);
    }

    public void SetSideWeaponActive() {
        if(sideWeaponSO != null && activeWeaponSO != sideWeaponSO) {
            activeWeaponSO = sideWeaponSO;
            mainWeaponAnimator.runtimeAnimatorController = sideWeaponSO.weaponAnimator;
        }
    }
    
    public void SetMainWeaponActive() {
        if (sideWeaponSO != null && activeWeaponSO != mainWeaponSO) {
            activeWeaponSO = mainWeaponSO;
            mainWeaponAnimator.runtimeAnimatorController = mainWeaponSO.weaponAnimator;
        }
    }

    public void SetAttackTrigger() {
        if (legendaryState == UCW.LegendaryState.Base) {
            activeWeaponAnimator.SetTrigger(magicStateTrigger);
        } else {
            activeWeaponAnimator.SetTrigger(legendaryStateTrigger);
        }
    }

    public void SetAttackStartTrigger()
    {

        if (!ucw.GetHasAttackStart_End()) {
            // Unit has no attack start and end animation
            return;
        }
        if(ucw.GetStartIsWeaponSprite())
        {
            // Weapon start animation is only one sprite
            UpdateWeaponMagicStateTrigger(magicState);
            SetAttackTrigger();
            activeWeaponAnimator.speed = 0;
            return;
        }
        UpdateWeaponMagicStateTrigger(magicState);

        activeWeaponAnimator.ResetTrigger(magicStateTrigger + "_End");

        magicStateTrigger = magicStateTrigger + "_Start";

        SetAttackTrigger();
    }

    public void SetAttackEndTrigger()
    {
        if(!ucw.GetHasAttackStart_End())
        {
            // Unit has no attack start and end animation
            return;
        }

        if (ucw.GetStartIsWeaponSprite())
        {
            // Weapon start animation is only one sprite
            activeWeaponAnimator.speed = 1;
            return;
        }

        UpdateWeaponMagicStateTrigger(magicState);
        magicStateTrigger = magicStateTrigger + "_End";
        SetAttackTrigger();
    }

    public void UpdateLegendaryState() {

        if (legendaryState != UCW.LegendaryState.Base) {
            weaponVisualSpriteRenderer.material = weaponMaterial;
            activeWeaponAnimator = legendaryWeaponAnimator;
            mainWeaponAnimator.enabled = false;
            weaponScale = Vector3.one;
        } else {
            weaponVisualSpriteRenderer.material = cleanMaterial;
            activeWeaponAnimator = mainWeaponAnimator;
            mainWeaponAnimator.enabled = true;
            weaponScale = upgradedWeaponScale;
        }

        switch (legendaryState) {
            case UCW.LegendaryState.Base:
                legendaryStateTrigger = "Base";
                break;
            case UCW.LegendaryState.IcebergBlade:
                legendaryStateTrigger = "IcebergBlade";
                break;
            case UCW.LegendaryState.ThunderboltSword:
                legendaryStateTrigger = "ThunderboltSword";
                break;
            case UCW.LegendaryState.ViperScimitar:
                legendaryStateTrigger = "ViperScimitar";
                break;
            case UCW.LegendaryState.VolcanoMace:
                legendaryStateTrigger = "VolcanoMace";
                break;
        }
    }

    public void ReplaceWeaponSO(WeaponSO newWeaponSO) {
        mainWeaponSO = newWeaponSO;
        activeWeaponSO = newWeaponSO;
    }

    public void ReplaceSideWeaponSO(WeaponSO newWeaponSO)
    {
        sideWeaponSO = newWeaponSO;
    }

    public void SetWeaponScale(Vector3 newWeaponScale) {

        if (legendaryState == UCW.LegendaryState.Base) {
            transform.localScale = newWeaponScale;
            weaponScale = newWeaponScale;
        }

        upgradedWeaponScale = newWeaponScale;
    }

    public void SetWeaponMaterial(Material newWeaponMaterial) {
        weaponMaterial = newWeaponMaterial;
        weaponVisualSpriteRenderer.material = weaponMaterial;
    }

}
