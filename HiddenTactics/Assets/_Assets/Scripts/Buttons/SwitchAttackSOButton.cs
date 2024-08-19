using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchAttackSOButton : MonoBehaviour
{
    private Button button;
    private AttackSO attackSO;

    [SerializeField] private Image attackTypeImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image backgroundImage;

    [SerializeField] private Sprite meleeAttackTypeSprite;
    [SerializeField] private Sprite rangedAttackTypeSprite;
    [SerializeField] private Sprite healTypeSprite;
    [SerializeField] private Sprite jumpTypeSprite;
    [SerializeField] private Sprite deathTriggerTypeSprite;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material cleanMaterial;


    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            IPlaceableDescriptionSlotTemplate.Instance.SetAttackPanel(attackSO, this);
        });
    }

    public void SetAttackSO(AttackSO attackSO) {
        this.attackSO = attackSO;

        if(attackSO.attackType == AttackSO.AttackType.melee) {
            attackTypeImage.sprite = meleeAttackTypeSprite;
        }

        if(attackSO.attackType == AttackSO.AttackType.ranged) {
            attackTypeImage.sprite = rangedAttackTypeSprite;
        }

        if (attackSO.attackType == AttackSO.AttackType.jump) {
            attackTypeImage.sprite = jumpTypeSprite;
        }

        if (attackSO.attackType == AttackSO.AttackType.deathTrigger) {
            attackTypeImage.sprite = deathTriggerTypeSprite;
        }

        if (attackSO.attackType == AttackSO.AttackType.healAllyMeleeTargeting || attackSO.attackType == AttackSO.AttackType.healAllyRangedTargeting) {
            attackTypeImage.sprite = healTypeSprite;
        }
    }

    public void SetSelected(bool selected) {
        if(selected) {
            outlineImage.material = selectedMaterial;
            backgroundImage.material = selectedMaterial;
            attackTypeImage.material = selectedMaterial;
        } else {
            outlineImage.material = cleanMaterial;
            backgroundImage.material = cleanMaterial;
            attackTypeImage.material = cleanMaterial;
        }
    }
}
