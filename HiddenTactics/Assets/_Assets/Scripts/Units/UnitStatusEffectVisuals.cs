using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitStatusEffectVisuals : NetworkBehaviour
{
    private Unit unit;
    private UnitBuffManager unitBuffManager;
    [SerializeField] protected Material transparentBuffMaterial;
    [SerializeField] protected Material cleanMaterial;
    [SerializeField] protected List<SpriteRenderer> buffSpriteRendererList;

    [SerializeField] protected SpriteRenderer attackSpeedBuffEffectSpriteRenderer;
    [SerializeField] protected SpriteRenderer attackSpeedBuffBaseSpriteRenderer;
    [SerializeField] protected SpriteRenderer attackDamageBuffEffectSpriteRenderer;
    [SerializeField] protected SpriteRenderer attackDamageBuffBaseSpriteRenderer;
    [SerializeField] protected SpriteRenderer healthRegenBuffEffectSpriteRenderer;
    [SerializeField] protected SpriteRenderer healthRegenBuffBaseSpriteRenderer;
    [SerializeField] protected SpriteRenderer moveSpeedBuffEffectSpriteRenderer;
    [SerializeField] protected SpriteRenderer moveSpeedBuffBaseSpriteRenderer;

    [SerializeField] protected Animator fireFXAnimator;
    [SerializeField] protected Animator fearFXAnimator;
    [SerializeField] protected Animator iceFXAnimator;
    [SerializeField] protected Animator poisonFXAnimator;
    [SerializeField] protected Animator bleedFXAnimator;
    [SerializeField] protected Animator stunFXAnimator;

    [SerializeField] protected Animator attackSpeedBuffAnimator;
    [SerializeField] protected Animator attackDamageBuffAnimator;
    [SerializeField] protected Animator healthRegenBuffAnimator;
    [SerializeField] protected Animator moveSpeedBuffAnimator;

    private void Awake() {
        unit = GetComponentInParent<Unit>();
        unitBuffManager = GetComponentInParent<UnitBuffManager>();
    }

    public override void OnNetworkSpawn() {
        if(!unit.GetUnitIsOnlyVisual()) {
            BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        }

        if (unit.GetUnitSO().isInvisibleGarrisonedUnit) return;

        unit.OnUnitFlamed += Unit_OnUnitFlamed;
        unit.OnUnitFlamedEnded += Unit_OnUnitFlameEnded;


        unit.OnUnitPoisoned += Unit_OnUnitPoisoned;
        unit.OnUnitPoisonedEnded += Unit_OnUnitPoisonedEnded;

        unit.OnUnitScared += Unit_OnUnitScared;
        unit.OnUnitScaredEnded += Unit_OnUnitScaredEnded;

        unitBuffManager.OnAttackRateBuffed += unitBuffManager_OnAttackRateBuffed;
        unitBuffManager.OnAttackRateDebuffed += unitBuffManager_OnAttackRateDebuffed;
        unitBuffManager.OnAttackDamageBuffed += unitBuffManager_OnAttackDamageBuffed;
        unitBuffManager.OnAttackDamageDebuffed += unitBuffManager_OnAttackDamageDebuffed;
        unitBuffManager.OnMoveSpeedBuffed += UnitBuffManager_OnMoveSpeedBuffed;
        unitBuffManager.OnMoveSpeedDebuffed += UnitBuffManager_OnMoveSpeedDebuffed;

        attackSpeedBuffAnimator.SetTrigger("Effect_Start");
        attackDamageBuffAnimator.SetTrigger("Effect_Start");
        moveSpeedBuffAnimator.SetTrigger("Effect_Start");

        ChangeBuffVisualsMaterials(transparentBuffMaterial);
        HideBuffEffects();
        HideBuffBase();
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {

        if(BattleManager.Instance.IsBattlePhase()) {
            ChangeBuffVisualsMaterials(cleanMaterial);
        }

        if(BattleManager.Instance.IsPreparationPhase()) {
            ChangeBuffVisualsMaterials(transparentBuffMaterial);
            HideBuffEffects();
            HideBuffBase();
            attackSpeedBuffAnimator.ResetTrigger("Effect_End");
            attackSpeedBuffAnimator.SetTrigger("Effect_Start");
            attackDamageBuffAnimator.ResetTrigger("Effect_End");
            attackDamageBuffAnimator.SetTrigger("Effect_Start");
            moveSpeedBuffAnimator.ResetTrigger("Effect_End");
            moveSpeedBuffAnimator.SetTrigger("Effect_Start");
        }
    }

    public void HideBuffEffects() {
        attackSpeedBuffEffectSpriteRenderer.enabled = false;
        attackDamageBuffEffectSpriteRenderer.enabled = false;
        moveSpeedBuffEffectSpriteRenderer.enabled = false;
    }

    public void ShowBuffEffects(SupportUnit.SupportType supportType) {
        if(supportType == SupportUnit.SupportType.attackSpeed) {
            attackSpeedBuffEffectSpriteRenderer.enabled = true;
        }
        if(supportType == SupportUnit.SupportType.attackDamage) {
            attackDamageBuffEffectSpriteRenderer.enabled = true;
        }
    }

    public void HideBuffBase() {
        attackSpeedBuffBaseSpriteRenderer.enabled = false;
        attackDamageBuffBaseSpriteRenderer.enabled = false;
        moveSpeedBuffBaseSpriteRenderer.enabled = false;
    }

    public void ShowBuffBase(SupportUnit.SupportType supportType) {
        if (supportType == SupportUnit.SupportType.attackSpeed) {
            attackSpeedBuffBaseSpriteRenderer.enabled = true;
        }
        if (supportType == SupportUnit.SupportType.attackDamage) {
            attackDamageBuffBaseSpriteRenderer.enabled = true;
        }
        if (supportType == SupportUnit.SupportType.moveSpeed) {
            moveSpeedBuffBaseSpriteRenderer.enabled = true;
        }
    }

    public void ActivateBuffVisuals(SupportUnit.SupportType supportType) {

        if (supportType == SupportUnit.SupportType.attackSpeed) {
            attackSpeedBuffAnimator.Play("Effect_Start");
            attackSpeedBuffAnimator.ResetTrigger("Effect_End");
            attackSpeedBuffBaseSpriteRenderer.enabled = true;
            attackSpeedBuffEffectSpriteRenderer.enabled = true;
        }

        if(supportType == SupportUnit.SupportType.attackDamage) {
            attackDamageBuffAnimator.Play("Effect_Start");
            attackDamageBuffAnimator.ResetTrigger("Effect_End");
            attackDamageBuffBaseSpriteRenderer.enabled = true;
            attackDamageBuffEffectSpriteRenderer.enabled = true;
        }

        if (supportType == SupportUnit.SupportType.moveSpeed) {
            moveSpeedBuffAnimator.Play("Effect_Start");
            moveSpeedBuffAnimator.ResetTrigger("Effect_End");
            moveSpeedBuffBaseSpriteRenderer.enabled = true;
            moveSpeedBuffEffectSpriteRenderer.enabled = true;
        }
    }

    private void ChangeBuffVisualsMaterials(Material material) {
        foreach(SpriteRenderer spriteRenderer in buffSpriteRendererList) {
            spriteRenderer.material = material;
        }
    }

    private void unitBuffManager_OnAttackRateDebuffed(object sender, EventArgs e) {
        attackSpeedBuffBaseSpriteRenderer.enabled = false;
        attackSpeedBuffEffectSpriteRenderer.enabled = false;
        attackSpeedBuffAnimator.ResetTrigger("Effect_Start");
        attackSpeedBuffAnimator.SetTrigger("Effect_End");
    }

    private void unitBuffManager_OnAttackRateBuffed(object sender, EventArgs e) {
        attackSpeedBuffBaseSpriteRenderer.enabled = true;
        attackSpeedBuffEffectSpriteRenderer.enabled = true;
        attackSpeedBuffAnimator.ResetTrigger("Effect_End");
        attackSpeedBuffAnimator.SetTrigger("Effect_Start");
    }

    private void unitBuffManager_OnAttackDamageDebuffed(object sender, EventArgs e) {
        attackDamageBuffBaseSpriteRenderer.enabled = false;
        attackDamageBuffEffectSpriteRenderer.enabled = false;
        attackDamageBuffAnimator.ResetTrigger("Effect_Start");
        attackDamageBuffAnimator.SetTrigger("Effect_End");
    }

    private void unitBuffManager_OnAttackDamageBuffed(object sender, EventArgs e) {
        attackDamageBuffBaseSpriteRenderer.enabled = true;
        attackDamageBuffEffectSpriteRenderer.enabled = true;
        attackDamageBuffAnimator.ResetTrigger("Effect_End");
        attackDamageBuffAnimator.SetTrigger("Effect_Start");
    }

    private void UnitBuffManager_OnMoveSpeedDebuffed(object sender, EventArgs e) {
        moveSpeedBuffBaseSpriteRenderer.enabled = false;
        moveSpeedBuffEffectSpriteRenderer.enabled = false;
        moveSpeedBuffAnimator.ResetTrigger("Effect_End");
        moveSpeedBuffAnimator.SetTrigger("Effect_Start");
    }

    private void UnitBuffManager_OnMoveSpeedBuffed(object sender, EventArgs e) {
        moveSpeedBuffBaseSpriteRenderer.enabled = true;
        moveSpeedBuffEffectSpriteRenderer.enabled = true;
        moveSpeedBuffAnimator.ResetTrigger("Effect_End");
        moveSpeedBuffAnimator.SetTrigger("Effect_Start");
    }

    private void Unit_OnUnitFlameEnded(object sender, EventArgs e) {
        fireFXAnimator.ResetTrigger("Effect_Start");
        fireFXAnimator.SetTrigger("Effect_End");
    }

    private void Unit_OnUnitFlamed(object sender, Unit.OnUnitSpecialEventArgs e) {

        fireFXAnimator.ResetTrigger("Effect_End");
        fireFXAnimator.SetTrigger("Effect_Start");
    }

    private void Unit_OnUnitPoisonedEnded(object sender, EventArgs e) {
        poisonFXAnimator.ResetTrigger("Effect_Start");
        poisonFXAnimator.SetTrigger("Effect_End");
    }

    private void Unit_OnUnitPoisoned(object sender, Unit.OnUnitSpecialEventArgs e) {

        poisonFXAnimator.ResetTrigger("Effect_End");
        poisonFXAnimator.SetTrigger("Effect_Start");
    }

    private void Unit_OnUnitScaredEnded(object sender, EventArgs e) {
        fearFXAnimator.ResetTrigger("Effect_Start");
        fearFXAnimator.SetTrigger("Effect_End");
    }

    private void Unit_OnUnitScared(object sender, Unit.OnUnitSpecialEventArgs e) {
        fearFXAnimator.ResetTrigger("Effect_End");
        fearFXAnimator.SetTrigger("Effect_Start");
    }

    public override void OnDestroy() {
        if (!unit.GetUnitIsOnlyVisual()) {
            BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
        }
    }

}
