using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IPlaceableDescriptionSlotTemplate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public static IPlaceableDescriptionSlotTemplate Instance;

    private bool pointerEntered;

    [SerializeField] private Animator animator;

    [SerializeField] private GameObject descriptionCardGameObject;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image outlineShadowImage;
    [SerializeField] private Image statPanelBackgroundImage;
    [SerializeField] private Image statPanelOutlineImage;
    [SerializeField] private Image attackPanelBackgroundImage;
    [SerializeField] private Image attackPanelOutlineImage;
    [SerializeField] private Image switchAttacksButtonOutlineImage;
    [SerializeField] private Image switchAttacksButtonBackgroundImage;

    [SerializeField] private Image cardIllustrationImage;
    [SerializeField] private Image moveTypeImage;
    [SerializeField] private Image attackTypeImage;
    [SerializeField] private Image attackDamageImage;
    [SerializeField] private Image attackSpeedImage;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI attackTargetsText;
    [SerializeField] private TextMeshProUGUI attackRangeText;
    [SerializeField] private TextMeshProUGUI attackAOEText;

    [SerializeField] private GameObject attackAOEGameObject;
    [SerializeField] private GameObject attackSpeedGameObject;

    [SerializeField] private GameObject statusEffectGameObject;
    [SerializeField] private TextMeshProUGUI statusEffect1Text;
    [SerializeField] private Image statusEffect1Image;
    [SerializeField] private TextMeshProUGUI statusEffect2Text;
    [SerializeField] private Image statusEffect2Image;

    [SerializeField] private Transform keywordContainer;
    [SerializeField] private Transform keywordTemplateTransform;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Transform keywordDescriptionContainer;
    [SerializeField] private Transform keywordDescriptionTemplate;

    [SerializeField] private Transform changeAttackSOButtonContainer;
    [SerializeField] private Transform changeAttackSOButtonTemplate;

    [SerializeField] private Sprite groundMoveType;
    [SerializeField] private Sprite airMoveType;
    [SerializeField] private Sprite meleeTargetType;
    [SerializeField] private Sprite rangedTargetType;

    [SerializeField] private Sprite healSprite;
    [SerializeField] private Sprite damageSprite;
    [SerializeField] private Sprite pierceSprite;
    [SerializeField] private Sprite fireSprite;
    [SerializeField] private Sprite bleedSprite;
    [SerializeField] private Sprite fearSprite;
    [SerializeField] private Sprite shockSprite;
    [SerializeField] private Sprite iceSprite;
    [SerializeField] private Sprite poisonSprite;

    [SerializeField] private float shortRangeMaxRange;
    [SerializeField] private float mediumRangeMaxRange;

    private UnitSO currentUnitSO;
    private TroopSO currentTroopSO;

    private AttackSO currentAttackSO;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SetFactionVisuals();
        descriptionCardGameObject.SetActive(false);
        keywordTemplateTransform.gameObject.SetActive(false);
        keywordDescriptionTemplate.gameObject.SetActive(false);
        keywordDescriptionContainer.gameObject.SetActive(false);
        changeAttackSOButtonTemplate.gameObject.SetActive(false);
    }

    private void SetFactionVisuals() {
        Deck playerDeck = DeckManager.LocalInstance.GetDeckSelected();
        FactionSO deckFactionSO = playerDeck.deckFactionSO;

        backgroundImage.sprite = deckFactionSO.panelBackground;
        outlineImage.sprite = deckFactionSO.panelBackgroundBorderSimple;
        outlineShadowImage.sprite = deckFactionSO.panelBackgroundBorderSimple;
        statPanelBackgroundImage.sprite = deckFactionSO.slotBackground;
        statPanelOutlineImage.sprite = deckFactionSO.slotBorder;
        attackPanelBackgroundImage.sprite = deckFactionSO.slotBackground;
        attackPanelOutlineImage.sprite = deckFactionSO.slotBorder;
        switchAttacksButtonBackgroundImage.sprite = deckFactionSO.slotBackground;
        switchAttacksButtonOutlineImage.sprite = deckFactionSO.slotBorder;

        Color keywordColor = deckFactionSO.color_samePlayerFaction_Opponent_fill;
        keywordColor.a = 1f;
        keywordTemplateTransform.GetComponent<TextMeshProUGUI>().color = keywordColor;
        keywordDescriptionTemplate.Find("KeyWordName").GetComponent<TextMeshProUGUI>().color = keywordColor;
    }

    public void SetDescriptionSlot(TroopSO troopSO, UnitSO unitSO) {
        currentUnitSO = unitSO;
        currentTroopSO = troopSO;

        cardIllustrationImage.sprite = troopSO.troopDescriptionSlotSprite;
        nameText.text = troopSO.troopName;
        costText.text = troopSO.spawnTroopCost.ToString();
        numberText.text = "x" + troopSO.troopPrefab.GetComponent<Troop>().GetBaseUnitPositions().Count.ToString();
        healthText.text = unitSO.HP.ToString();
        armorText.text = unitSO.armor.ToString();
        moveSpeedText.text = unitSO.unitMoveSpeed.ToString();
        descriptionText.text = troopSO.troopDescription;

        currentAttackSO = currentUnitSO.mainAttackSO;
        RefreshAttackTypeButtons();
        RefreshKeywords(unitSO);
        SetAttackStats(unitSO.mainAttackSO);
        SetAttackTargetTypesStats(unitSO.mainAttackSO);
        SetMoveTypeStats(unitSO);
        SetAttackTypeStats(unitSO.mainAttackSO);
        SetStatusEffectStats(unitSO.mainAttackSO);
    }

    private void RefreshAttackTypeButtons() {

        foreach(Transform child in changeAttackSOButtonContainer) {
            if (child == changeAttackSOButtonTemplate) continue;
            Destroy(child.gameObject);
        }

        if (currentUnitSO.mainAttackSO != null) {
            SwitchAttackSOButton switchAttackSOButton = Instantiate(changeAttackSOButtonTemplate, changeAttackSOButtonContainer).GetComponent<SwitchAttackSOButton>();
            switchAttackSOButton.SetAttackSO(currentUnitSO.mainAttackSO);
            switchAttackSOButton.gameObject.SetActive(true);
            switchAttackSOButton.SetSelected(true);
        }

        if (currentUnitSO.sideAttackSO != null) {
            SwitchAttackSOButton switchAttackSOButton = Instantiate(changeAttackSOButtonTemplate, changeAttackSOButtonContainer).GetComponent<SwitchAttackSOButton>();
            switchAttackSOButton.SetAttackSO(currentUnitSO.sideAttackSO);
            switchAttackSOButton.gameObject.SetActive(true);
            switchAttackSOButton.SetSelected(false);
        }

        if (currentUnitSO.jumpAttackSO != null) {
            SwitchAttackSOButton switchAttackSOButton = Instantiate(changeAttackSOButtonTemplate, changeAttackSOButtonContainer).GetComponent<SwitchAttackSOButton>();
            switchAttackSOButton.SetAttackSO(currentUnitSO.jumpAttackSO);
            switchAttackSOButton.gameObject.SetActive(true);
            switchAttackSOButton.SetSelected(false);
        }

        if (currentUnitSO.deathTriggerAttackSO != null) {
            SwitchAttackSOButton switchAttackSOButton = Instantiate(changeAttackSOButtonTemplate, changeAttackSOButtonContainer).GetComponent<SwitchAttackSOButton>();
            switchAttackSOButton.SetAttackSO(currentUnitSO.deathTriggerAttackSO);
            switchAttackSOButton.gameObject.SetActive(true);
            switchAttackSOButton.SetSelected(false);
        }

    }

    private void SetAttackStats(AttackSO attackSO) {
        if(attackSO.attackType == AttackSO.AttackType.melee || attackSO.attackType == AttackSO.AttackType.ranged || attackSO.attackType == AttackSO.AttackType.healAllyMeleeTargeting || attackSO.attackType == AttackSO.AttackType.healAllyRangedTargeting) {
            attackSpeedGameObject.SetActive(true);
            attackDamageText.text = attackSO.attackDamage.ToString();
            attackSpeedText.text = attackSO.attackRate.ToString() + "s";
        }

        if(attackSO.attackType == AttackSO.AttackType.healAllyMeleeTargeting || attackSO.attackType == AttackSO.AttackType.healAllyRangedTargeting) {
            attackDamageImage.sprite = healSprite;
        } else {
            attackDamageImage.sprite = damageSprite;
        }

        if (attackSO.attackType == AttackSO.AttackType.deathTrigger ) {
            attackSpeedGameObject.SetActive(false);
            attackDamageImage.sprite = GetStatusEffectSprite(attackSO.attackSpecialList[0]);
        } else {
            attackDamageImage.sprite = damageSprite;
        }

        if(attackSO.attackType == AttackSO.AttackType.jump) {
            attackSpeedGameObject.SetActive(false);
            attackDamageText.text = attackSO.attackDamage.ToString();
        }

        // AOE elements
        if (attackSO.attackAOE != 0) {
            attackAOEGameObject.SetActive(true);
            attackAOEText.text = attackSO.attackAOE.ToString();
        }
        else {
            attackAOEGameObject.SetActive(false);
        }
    }

    private void SetAttackTargetTypesStats(AttackSO attackSO) {
        attackTargetsText.text = "";
        if (attackSO.attackTargetTypes.Contains(ITargetable.TargetType.groundUnit)) {
            attackTargetsText.text += "Ground";
        }
        if (attackSO.attackTargetTypes.Contains(ITargetable.TargetType.airUnit)) {
            attackTargetsText.text += ", Air";
        }
        if (attackSO.attackTargetTypes.Contains(ITargetable.TargetType.building)) {
            attackTargetsText.text += ", Buildings";
        }
    }

    private void SetMoveTypeStats(UnitSO unitSO) {

        if (unitSO.moveType == UnitSO.MoveType.ground) {
            moveTypeImage.sprite = groundMoveType;
        }
        else {
            moveTypeImage.sprite = airMoveType;
        }
    }

    private void SetAttackTypeStats(AttackSO attackSO) {

        if (attackSO.attackType == AttackSO.AttackType.melee || attackSO.attackType == AttackSO.AttackType.healAllyMeleeTargeting || attackSO.attackType == AttackSO.AttackType.deathTrigger || attackSO.attackType == AttackSO.AttackType.jump) {
            attackTypeImage.sprite = meleeTargetType;

            if (attackSO.meleeAttackRange <= shortRangeMaxRange) {
                attackRangeText.text = "Short";
            }

            if (attackSO.meleeAttackRange > shortRangeMaxRange && attackSO.meleeAttackRange <= mediumRangeMaxRange) {
                attackRangeText.text = "Medium";
            }

            if (attackSO.meleeAttackRange > mediumRangeMaxRange) {
                attackRangeText.text = "Long";
            }
            return;
        }

        if(attackSO.attackType == AttackSO.AttackType.ranged) {

            attackTypeImage.sprite = rangedTargetType;

            int minXTile = 0;
            int maxXTile = 0;

            foreach (Vector2 gridPosition in attackSO.attackTargetTiles) {
                if (gridPosition.x < minXTile) {
                    minXTile = (int)gridPosition.x;
                }
                if (gridPosition.x > maxXTile) {
                    maxXTile = (int)gridPosition.x;
                }

                attackRangeText.text = minXTile.ToString() + "-" + maxXTile.ToString();
            }
        }
    }

    private void SetStatusEffectStats(AttackSO attackSO) {
        if (attackSO.attackSpecialList.Count > 0) {
            statusEffect1Text.gameObject.SetActive(true);
            statusEffect1Image.gameObject.SetActive(true);
            statusEffectGameObject.SetActive(true);
            statusEffect1Text.text = attackSO.attackSpecialList[0].ToString();
            statusEffect1Image.sprite = GetStatusEffectSprite(attackSO.attackSpecialList[0]);
        } else {
            statusEffect1Text.gameObject.SetActive(false);
            statusEffect1Image.gameObject.SetActive(false);
            statusEffectGameObject.SetActive(false);
        }

        if (attackSO.attackSpecialList.Count > 1) {
            statusEffect2Text.gameObject.SetActive(true);
            statusEffect2Image.gameObject.SetActive(true);
            statusEffect2Text.text = attackSO.attackSpecialList[1].ToString();
            statusEffect2Image.sprite = GetStatusEffectSprite(attackSO.attackSpecialList[1]);
        } else {
            statusEffect2Text.gameObject.SetActive(false);
            statusEffect2Image.gameObject.SetActive(false);
        }

    }

    private void RefreshKeywords(UnitSO unitSO) {
        keywordDescriptionContainer.gameObject.SetActive(false);

            // Refresh card keywords
            foreach (Transform child in keywordContainer) {
            if (child == keywordTemplateTransform) continue;
            Destroy(child.gameObject);
            }

            foreach(UnitSO.UnitKeyword unitKeyword in unitSO.unitKeywordsList) {
                Transform instantiatedKeywordTemplate = Instantiate(keywordTemplateTransform, keywordContainer);

                string unitKeywordString = SetKeywordName(unitKeyword);
                if (unitKeyword != unitSO.unitKeywordsList[unitSO.unitKeywordsList.Count -1]) {
                        unitKeywordString += ", ";
                    }

                instantiatedKeywordTemplate.GetComponent<TextMeshProUGUI>().text = unitKeywordString;

                instantiatedKeywordTemplate.gameObject.SetActive(true);
            }

        // Refresh description keywords
        if (unitSO.unitKeywordsList.Count > 0) {
            keywordDescriptionContainer.gameObject.SetActive(true);

            foreach (Transform child in keywordDescriptionContainer) {
                if (child == keywordDescriptionTemplate) continue;
                Destroy(child.gameObject);
            }

            foreach (UnitSO.UnitKeyword unitKeyword in unitSO.unitKeywordsList) {
                Transform instantiatedKeywordTemplate = Instantiate(keywordDescriptionTemplate, keywordDescriptionContainer);

                instantiatedKeywordTemplate.Find("KeyWordName").GetComponent<TextMeshProUGUI>().text = SetKeywordName(unitKeyword);
                instantiatedKeywordTemplate.Find("KeyWordDescription").GetComponent<TextMeshProUGUI>().text = SetKeywordDescription(unitKeyword);
                instantiatedKeywordTemplate.gameObject.SetActive(true);
            }
        }
    } 

    private Sprite GetStatusEffectSprite(AttackSO.UnitAttackSpecial attackSpecial) {
        if(attackSpecial == AttackSO.UnitAttackSpecial.pierce) {
            return pierceSprite;
        }

        if (attackSpecial == AttackSO.UnitAttackSpecial.fear) {
            return fearSprite;
        }

        if (attackSpecial == AttackSO.UnitAttackSpecial.bleed) {
            return bleedSprite;
        }

        if (attackSpecial == AttackSO.UnitAttackSpecial.fire) {
            return fireSprite;
        }

        if (attackSpecial == AttackSO.UnitAttackSpecial.shock) {
            return shockSprite;
        }

        if (attackSpecial == AttackSO.UnitAttackSpecial.ice) {
            return iceSprite;
        }

        if (attackSpecial == AttackSO.UnitAttackSpecial.poison) {
            return poisonSprite;
        }

        if (attackSpecial == AttackSO.UnitAttackSpecial.heal) {
            return healSprite;
        }

        return null;
    }

    private string SetKeywordDescription(UnitSO.UnitKeyword unitKeyword) {
        if(unitKeyword == UnitSO.UnitKeyword.Flying) {
            return "Can fly over buildings, is not targeted by melee attacks.";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Spawner) {
            return "Spawns more units !";
        }

        if (unitKeyword == UnitSO.UnitKeyword.AntiLarge) {
            return "Deals double damage to large units";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Large) {
            return "Is huge";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Beast) {
            return "Is a beast";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Shielded) {
            return "Tanks a few arrow volleys between each encounter";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Charge) {
            return "Charges after a dealay, increasing it's movement speed and damage";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Jumper) {
            return "Jumps when entering melee combat, dealing damage";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Siege) {
            return "Deals double damage to buildings";
        }

        if (unitKeyword == UnitSO.UnitKeyword.RoyalAura) {
            return "Increases nearby units attack speed";
        }

        if (unitKeyword == UnitSO.UnitKeyword.BushyBeard) {
            return "Increases nearby units movement speed";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Support) {
            return "Buffs nearby units at the beginning of a battle, and until this unit dies";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Hybrid) {
            return "Fires projectiles before each melee encounter";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Restricted) {
            return "Movement is limited to the highlighted tiles during placement";
        }

        if (unitKeyword == UnitSO.UnitKeyword.DeathTrigger) {
            return "Triggers an effect when dying";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Healer) {
            return "Heals ally units";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Unstoppable) {
            return "Does not stop walking, attacks while moving";
        }

        if (unitKeyword == UnitSO.UnitKeyword.BloodFlag) {
            return "Increases nearby units damage";
        }


        return "keyword description not set";
    }

    private string SetKeywordName(UnitSO.UnitKeyword unitKeyword) {
       
        if (unitKeyword == UnitSO.UnitKeyword.AntiLarge) {
            return "Anti-Large";
        }

        if (unitKeyword == UnitSO.UnitKeyword.RoyalAura) {
            return "Royal Aura";
        }

        if (unitKeyword == UnitSO.UnitKeyword.BushyBeard) {
            return "Bushy Beard";
        }

        if (unitKeyword == UnitSO.UnitKeyword.DeathTrigger) {
            return "Death Trigger";
        }

        if (unitKeyword == UnitSO.UnitKeyword.BloodFlag) {
            return "Blood Flag";
        }

        return unitKeyword.ToString();
    }

    public void SetAttackPanel(AttackSO attackSO, SwitchAttackSOButton selectedButton) {

        currentAttackSO = attackSO;
        SetAttackTargetTypesStats(attackSO);
        SetAttackTypeStats(attackSO);
        SetStatusEffectStats(attackSO);
        SetAttackStats(attackSO);

        foreach(SwitchAttackSOButton button in changeAttackSOButtonContainer.GetComponentsInChildren<SwitchAttackSOButton>()) {
            button.SetSelected(false);
        }

        selectedButton.SetSelected(true);
    }

    public void Show() {
        descriptionCardGameObject.SetActive(true);
    }

    public void Hide() {
        if (pointerEntered) return;
        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine() {

        if (animator != null) {
            animator.SetTrigger("Close");
        }

        yield return new WaitForSeconds(.3f);
        descriptionCardGameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        pointerEntered = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        pointerEntered = false;
    }
}
