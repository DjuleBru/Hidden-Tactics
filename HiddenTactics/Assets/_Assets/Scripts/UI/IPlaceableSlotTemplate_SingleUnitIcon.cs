using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IPlaceableSlotTemplate_SingleUnitIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private BattlePhaseIPlaceableSlotTemplateUI parentSlotTemplate;

    [SerializeField] private Image slotBackgroundImage;
    [SerializeField] private Image unitIconBackground;
    [SerializeField] private Image unitIconFill;

    [SerializeField] private Color baseColor;
    [SerializeField] private Color hoveredColor;
    [SerializeField] private Color showInteractableColor;

    [SerializeField] private Transform statusEffectsContainer;
    [SerializeField] private Transform statusEffectsTemplate;

    [SerializeField] private Gradient healthGradient = new Gradient();

    private Unit unit;
    private UnitHP unitHP;
    private UnitBuffManager unitBuffManager;

    private IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect moveSpeedBuffTemplate;
    private IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect attackSpeedBuffTemplate;
    private IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect attackDamageBuffTemplate;
    private IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect fireTemplate;
    private IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect fearTemplate;
    private IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect poisonTemplate;

    private float statusEffectsUpdateTimer;
    private float statusEffectsUpdateRate = .2f;

    private bool burning;
    private bool poisoned;
    private bool scared;

    private bool interactable;
    private bool pointerEntered;

    private void Awake() {
        statusEffectsTemplate.gameObject.SetActive(false);
    }

    public void SetUnit(Unit unit, BattlePhaseIPlaceableSlotTemplateUI parentSlotTemplate) {
        this.parentSlotTemplate = parentSlotTemplate;

        this.unit = unit;
        unitHP = unit.GetComponent<UnitHP>();
        unitBuffManager = unit.GetComponent<UnitBuffManager>();

        unitIconBackground.sprite = unit.GetUnitSO().singleUnitSprite;
        unitIconFill.sprite = unit.GetUnitSO().singleUnitSprite;

        unitHP.OnHealthChanged += Unit_OnHealthChanged;
        unit.OnUnitReset += Unit_OnUnitReset;

        unit.OnUnitFlamed += Unit_OnUnitFlamed;
        unit.OnUnitFlamedEnded += Unit_OnUnitFlamedEnded;
        unit.OnUnitPoisoned += Unit_OnUnitPoisoned;
        unit.OnUnitPoisonedEnded += Unit_OnUnitPoisonedEnded;
        unit.OnUnitScared += Unit_OnUnitScared;
        unit.OnUnitScaredEnded += Unit_OnUnitScaredEnded;

        unitBuffManager.OnAttackRateBuffed += UnitBuffManager_OnAttackRateBuffed;
        unitBuffManager.OnAttackRateDebuffed += UnitBuffManager_OnAttackRateDebuffed;
        unitBuffManager.OnAttackDamageBuffed += UnitBuffManager_OnAttackDamageBuffed;
        unitBuffManager.OnAttackDamageDebuffed += UnitBuffManager_OnAttackDamageDebuffed;
        unitBuffManager.OnMoveSpeedBuffed += UnitBuffManager_OnMoveSpeedBuffed;
        unitBuffManager.OnMoveSpeedDebuffed += UnitBuffManager_OnMoveSpeedDebuffed;
    }

    private void Update() {
        statusEffectsUpdateTimer -= Time.deltaTime;
        if(statusEffectsUpdateTimer < 0) {
            statusEffectsUpdateTimer = statusEffectsUpdateRate;
            UpdateStatusEffects();
        }
    }

    private void UpdateStatusEffects() {
        if(burning) {
            fireTemplate.SetStatusEffectText(((int)unit.GetBurningRemainingTime()).ToString());
            fireTemplate.SetStatusEffectFill(unit.GetBurningDurationNormalized());
        }
        if (scared) {
            fireTemplate.SetStatusEffectText(((int)unit.GetScaredRemainingTime()).ToString());
            fireTemplate.SetStatusEffectFill(unit.GetScaredDurationNormalized());
        }
        if (poisoned) {
            fireTemplate.SetStatusEffectText(((int)unit.GetPoisonedRemainingTime()).ToString());
            fireTemplate.SetStatusEffectFill(unit.GetPoisonedDurationNormalized());
        }
    }

    private void Unit_OnHealthChanged(object sender, UnitHP.OnHealthChangedEventArgs e) {
        float hpNormalized = e.newHealth / (float)unit.GetComponent<UnitHP>().GetMaxHP();
        Color fillColor = healthGradient.Evaluate(hpNormalized);

        unitIconFill.fillAmount = hpNormalized;
        unitIconFill.color = fillColor;

        Color alphaFillColor = fillColor;
        alphaFillColor.a = .25f;
        unitIconBackground.color = alphaFillColor;
    }

    private void Unit_OnUnitScaredEnded(object sender, System.EventArgs e) {
        if (fearTemplate != null) {
            Destroy(fearTemplate.gameObject);
        }
        scared = false;
    }

    private void Unit_OnUnitScared(object sender, Unit.OnUnitSpecialEventArgs e) {
        if (fearTemplate == null) {
            fearTemplate = SetBuffTemplate(IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType.Fear);
        }
        scared = true;
    }

    private void Unit_OnUnitPoisonedEnded(object sender, System.EventArgs e) {
        if (poisonTemplate != null) {
            Destroy(poisonTemplate.gameObject);
        }
        poisoned = false;
    }

    private void Unit_OnUnitPoisoned(object sender, Unit.OnUnitSpecialEventArgs e) {
        if (poisonTemplate == null) {
            poisonTemplate = SetBuffTemplate(IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType.Poison);
        }
        poisoned = true;
    }

    private void Unit_OnUnitFlamedEnded(object sender, System.EventArgs e) {
        if (fireTemplate != null) {
            Destroy(fireTemplate.gameObject);
        }
        burning = false;
    }

    private void Unit_OnUnitFlamed(object sender, Unit.OnUnitSpecialEventArgs e) {
        if (fireTemplate == null) {
            fireTemplate = SetBuffTemplate(IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType.Fire);
        }
        burning = true;
    }

    private void UnitBuffManager_OnMoveSpeedDebuffed(object sender, System.EventArgs e) {
        if(moveSpeedBuffTemplate != null) {
            if(unitBuffManager.GetAttackDamageMultiplier() == 1) {
                Destroy(moveSpeedBuffTemplate.gameObject);
            }
            else {
                moveSpeedBuffTemplate.SetStatusEffectText(SetBuffText(unitBuffManager.GetMoveSpeedMultiplier()));
            }
        }
    }

    private void UnitBuffManager_OnMoveSpeedBuffed(object sender, System.EventArgs e) {
        if(moveSpeedBuffTemplate == null) {
            moveSpeedBuffTemplate = SetBuffTemplate(IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType.MoveSpeed);
        }
        else {
            moveSpeedBuffTemplate.SetStatusEffectText(SetBuffText(unitBuffManager.GetMoveSpeedMultiplier()));
        }
    }

    private void UnitBuffManager_OnAttackDamageDebuffed(object sender, System.EventArgs e) {
        if (attackDamageBuffTemplate != null) {
            if (unitBuffManager.GetAttackDamageMultiplier() == 1) {
                Destroy(attackDamageBuffTemplate.gameObject);
            } else {
                attackDamageBuffTemplate.SetStatusEffectText(SetBuffText(unitBuffManager.GetAttackDamageMultiplier()));
            }
        }
    }

    private void UnitBuffManager_OnAttackDamageBuffed(object sender, System.EventArgs e) {
        if (attackDamageBuffTemplate == null) {
            attackDamageBuffTemplate = SetBuffTemplate(IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType.Damage);
        }
        else {
            attackDamageBuffTemplate.SetStatusEffectText(SetBuffText(unitBuffManager.GetAttackDamageMultiplier()));
        }
    }

    private void UnitBuffManager_OnAttackRateDebuffed(object sender, System.EventArgs e) {
        if (attackSpeedBuffTemplate != null) {
            if (unitBuffManager.GetAttackRateMultiplier() == 1) {
                Destroy(attackSpeedBuffTemplate.gameObject);
            }
            else {
                attackSpeedBuffTemplate.SetStatusEffectText(SetBuffText(unitBuffManager.GetAttackRateMultiplier()));
            }
        }
    }

    private void UnitBuffManager_OnAttackRateBuffed(object sender, System.EventArgs e) {
        if (attackSpeedBuffTemplate == null) {
            attackSpeedBuffTemplate = SetBuffTemplate(IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType.AttackSpeed);
        }
        else {
            attackSpeedBuffTemplate.SetStatusEffectText(SetBuffText(unitBuffManager.GetAttackRateMultiplier()));
        }
    }

    private IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect SetBuffTemplate(IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType buffType) {
        IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect template = Instantiate(statusEffectsTemplate, statusEffectsContainer).GetComponent<IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect>();
        template.gameObject.SetActive(true);

        template.SetStatusEffectType(buffType);

        if(buffType == IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType.MoveSpeed) {
            template.SetStatusEffectText(SetBuffText(unitBuffManager.GetMoveSpeedMultiplier()));
        }

        if(buffType == IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType.AttackSpeed) {
            template.SetStatusEffectText(SetBuffText(unitBuffManager.GetAttackRateMultiplier()));
        }

        if(buffType == IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect.StatusEffectType.Damage) {
            template.SetStatusEffectText(SetBuffText(unitBuffManager.GetAttackDamageMultiplier()));
        }

        return template;
    }

    private string SetBuffText(float statMultiplier) {
        if(statMultiplier > 1) {
            return "+" + ((statMultiplier-1)*100).ToString() + "%";
        } else {
            return ((statMultiplier-1)*100).ToString() + "%";
        }
    }

    private void Unit_OnUnitReset(object sender, System.EventArgs e) {
        Destroy(moveSpeedBuffTemplate);
    }

    public void SetInteractable(bool interactable) {
        this.interactable = interactable;

        if(pointerEntered) {
            unit.SetUnitSelected(true);
            slotBackgroundImage.color = hoveredColor;
            parentSlotTemplate.DeselectOtherSlots(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        pointerEntered = false;

        if (!interactable) return;
        unit.SetUnitSelected(false);
        slotBackgroundImage.color = baseColor;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        pointerEntered = true;
        if (!interactable) return;

        parentSlotTemplate.DeselectOtherSlots(this);
        unit.SetUnitSelected(true);
        slotBackgroundImage.color = hoveredColor;
    }

    public void SetUnitUnSelected() {
        unit.SetUnitSelected(false);
        slotBackgroundImage.color = baseColor;
    }
}
