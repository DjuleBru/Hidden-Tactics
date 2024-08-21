using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IPlaceableSlotTemplate_SingleUnitIcon_StatusEffect : MonoBehaviour
{
    [SerializeField] private Image statusEffectImageBackground;
    [SerializeField] private Image statusEffectImageFill;
    [SerializeField] private TextMeshProUGUI statusEffectText;

    [SerializeField] private Sprite moveSpeedSprite;
    [SerializeField] private Sprite damageSprite;
    [SerializeField] private Sprite attackSpeedSprite;
    [SerializeField] private Sprite fireSprite;
    [SerializeField] private Sprite fearSprite;
    [SerializeField] private Sprite poisonSprite;

    public enum StatusEffectType {
        MoveSpeed,
        AttackSpeed,
        Damage,
        Fire,
        Poison,
        Fear,
    }

    public StatusEffectType type;

    public void SetStatusEffectType(StatusEffectType type) {
        this.type = type;

        if(type == StatusEffectType.MoveSpeed) {
            statusEffectImageBackground.sprite = moveSpeedSprite;
            statusEffectImageFill.sprite = moveSpeedSprite;
        }

        if (type == StatusEffectType.Damage) {
            statusEffectImageBackground.sprite = damageSprite;
            statusEffectImageFill.sprite = damageSprite;
        }

        if (type == StatusEffectType.AttackSpeed) {
            statusEffectImageBackground.sprite = attackSpeedSprite;
            statusEffectImageFill.sprite = attackSpeedSprite;
        }

        if (type == StatusEffectType.Fire) {
            statusEffectImageBackground.sprite = fireSprite;
            statusEffectImageFill.sprite = fireSprite;
        }

        if (type == StatusEffectType.Fear) {
            statusEffectImageBackground.sprite = fearSprite;
            statusEffectImageFill.sprite = fearSprite;
        }

        if (type == StatusEffectType.Poison) {
            statusEffectImageBackground.sprite = poisonSprite;
            statusEffectImageFill.sprite = poisonSprite;
        }

    }

    public void SetStatusEffectText(string text) {
        statusEffectText.text = text;
    }

    public void SetStatusEffectFill(float fillAmount) {
        statusEffectImageFill.fillAmount = fillAmount;
    }

}
