using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitStatusEffectVisuals : NetworkBehaviour
{
    private Unit unit;
    private UnitAttack unitAttack;
    [SerializeField] protected Material transparentBuffMaterial;
    [SerializeField] protected Material cleanMaterial;
    [SerializeField] protected List<SpriteRenderer> buffSpriteRendererList;
    [SerializeField] protected SpriteRenderer attackSpeedBuffEffectSpriteRenderer;
    [SerializeField] protected SpriteRenderer attackSpeedBuffBaseSpriteRenderer;

    [SerializeField] protected Animator fireFXAnimator;
    [SerializeField] protected Animator fearFXAnimator;
    [SerializeField] protected Animator iceFXAnimator;
    [SerializeField] protected Animator poisonFXAnimator;
    [SerializeField] protected Animator bleedFXAnimator;
    [SerializeField] protected Animator stunFXAnimator;

    [SerializeField] protected Animator attackSpeedBuffAnimator;

    private void Awake() {
        unit = GetComponentInParent<Unit>();
        unitAttack = GetComponentInParent<UnitAttack>();
    }

    public override void OnNetworkSpawn() {
        if(!unit.GetUnitIsOnlyVisual()) {
            BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        }

        unit.OnUnitFlamed += Unit_OnUnitFlamed;
        unit.OnUnitFlamedEnded += Unit_OnUnitFlameEnded;

        unit.OnUnitScared += Unit_OnUnitScared;
        unit.OnUnitScaredEnded += Unit_OnUnitScaredEnded;

        unitAttack.OnAttackRateBuffed += UnitAttack_OnAttackRateBuffed;
        unitAttack.OnAttackRateDebuffed += UnitAttack_OnAttackRateDebuffed;

        attackSpeedBuffAnimator.SetTrigger("Effect_Start");

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
        }
    }

    public void HideBuffEffects() {
        attackSpeedBuffEffectSpriteRenderer.enabled = false;
    }

    public void ShowBuffEffects() {
        attackSpeedBuffEffectSpriteRenderer.enabled = true;
    }

    public void HideBuffBase() {
        attackSpeedBuffBaseSpriteRenderer.enabled = false;
    }

    public void ShowBuffBase() {
        attackSpeedBuffBaseSpriteRenderer.enabled = true;
    }

    public void ActivateBuffVisuals() {
        attackSpeedBuffAnimator.Play("Effect_Start");
        attackSpeedBuffAnimator.ResetTrigger("Effect_End");
        attackSpeedBuffBaseSpriteRenderer.enabled = true;
        attackSpeedBuffEffectSpriteRenderer.enabled = true;
    }

    private void ChangeBuffVisualsMaterials(Material material) {
        foreach(SpriteRenderer spriteRenderer in buffSpriteRendererList) {
            spriteRenderer.material = material;
        }
    }

    private void UnitAttack_OnAttackRateDebuffed(object sender, EventArgs e) {
        Debug.Log("attack rate reset received");
        attackSpeedBuffBaseSpriteRenderer.enabled = false;
        attackSpeedBuffEffectSpriteRenderer.enabled = false;
        attackSpeedBuffAnimator.ResetTrigger("Effect_Start");
        attackSpeedBuffAnimator.SetTrigger("Effect_End");
    }

    private void UnitAttack_OnAttackRateBuffed(object sender, EventArgs e) {
        Debug.Log("attack rate buffed received");
        attackSpeedBuffBaseSpriteRenderer.enabled = true;
        attackSpeedBuffEffectSpriteRenderer.enabled = true;
        attackSpeedBuffAnimator.ResetTrigger("Effect_End");
        attackSpeedBuffAnimator.SetTrigger("Effect_Start");
    }

    private void Unit_OnUnitFlameEnded(object sender, EventArgs e) {
        fireFXAnimator.ResetTrigger("Effect_Start");
        fireFXAnimator.SetTrigger("Effect_End");
    }

    private void Unit_OnUnitFlamed(object sender, Unit.OnUnitSpecialEventArgs e) {

        fireFXAnimator.ResetTrigger("Effect_End");
        fireFXAnimator.SetTrigger("Effect_Start");
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
