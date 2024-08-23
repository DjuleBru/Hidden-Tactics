using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IPlaceableDescriptionSlotTemplate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public static IPlaceableDescriptionSlotTemplate Instance;

    private bool pointerEntered;
    private bool cardOpened;

    [SerializeField] private Animator animator;

    [SerializeField] private GameObject descriptionCardGameObject;
    [SerializeField] private GameObject attackDescriptionPanel;
    [SerializeField] private GameObject buildingDescriptionPanel;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image outlineShadowImage;
    [SerializeField] private Image statPanelBackgroundImage;
    [SerializeField] private Image statPanelOutlineImage;
    [SerializeField] private Image attackPanelBackgroundImage;
    [SerializeField] private Image attackPanelOutlineImage;
    [SerializeField] private Image buildingPanelBackgroundImage;
    [SerializeField] private Image buildingPanelOutlineImage;
    [SerializeField] private Image switchAttacksButtonOutlineImage;
    [SerializeField] private Image switchAttacksButtonBackgroundImage;
    [SerializeField] private Image showOtherTroop1ButtonOutlineImage;
    [SerializeField] private Image showOtherTroop1ButtonBackgroundImage;
    [SerializeField] private Image showOtherTroop2ButtonOutlineImage;
    [SerializeField] private Image showOtherTroop2ButtonBackgroundImage;
    [SerializeField] private Image keywordDescriptionBackgroundImage;

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
    [SerializeField] private TextMeshProUGUI OtherTroop1NumberText;
    [SerializeField] private TextMeshProUGUI OtherTroop2NumberText;

    [SerializeField] private ShowGarrisonedTroopButton showOtherTroop1Button;
    [SerializeField] private ShowGarrisonedTroopButton showOtherTroop2Button;

    [SerializeField] private GameObject unitCostGameObject;
    [SerializeField] private GameObject unitArmorGameObject;
    [SerializeField] private GameObject unitNumberGameObject;
    [SerializeField] private GameObject unitMoveSpeedGameObject;
    [SerializeField] private GameObject otherTroop1GameObject;
    [SerializeField] private GameObject otherTroop2GameObject;

    [SerializeField] private GameObject attackAOEGameObject;
    [SerializeField] private GameObject attackSpeedGameObject;
    [SerializeField] private GameObject attackDamageGameObject;

    [SerializeField] private GameObject economicalBuildingGameObject;
    [SerializeField] private GameObject treeWallGameObject;
    [SerializeField] private GameObject reflectDamageGameObject;
    [SerializeField] private GameObject trappedWallGameObject;
    [SerializeField] private TextMeshProUGUI economicalBuildingRevenueText;
    [SerializeField] private TextMeshProUGUI economicalBuildingDescriptionText;
    [SerializeField] private Image economicalBuildingImage;
    [SerializeField] private TextMeshProUGUI reflectDamageAmountText;

    [SerializeField] private GameObject statusEffectGameObject;
    [SerializeField] private TextMeshProUGUI statusEffect1Text;
    [SerializeField] private Image statusEffect1Image;
    [SerializeField] private TextMeshProUGUI statusEffect2Text;
    [SerializeField] private Image statusEffect2Image;

    [SerializeField] private TextMeshProUGUI keywordText;
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
    [SerializeField] private Sprite slowedSprite;
    [SerializeField] private Sprite knockbackSprite;
    [SerializeField] private Sprite coinSprite;
    [SerializeField] private Sprite elfWolfDenWolfSprite;

    [SerializeField] private float shortRangeMaxRange;
    [SerializeField] private float mediumRangeMaxRange;

    private UnitSO currentUnitSO;
    private TroopSO currentTroopSO;
    private TroopSO parentTroopSO;
    private BuildingSO currentBuildingSO;

    private AttackSO currentAttackSO;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SetFactionVisuals();
        descriptionCardGameObject.SetActive(false);
        keywordDescriptionTemplate.gameObject.SetActive(false);
        keywordDescriptionContainer.gameObject.SetActive(false);
        changeAttackSOButtonTemplate.gameObject.SetActive(false);

        DeckManager.LocalInstance.OnSelectedDeckChanged += DeckManager_OnSelectedDeckChanged;
        GameInput.Instance.OnLeftClickPerformed += GameInput_OnLeftClickPerformed;
        GameInput.Instance.OnRightClickPerformed += GameInput_OnRightClickPerformed;
    }

    private void GameInput_OnRightClickPerformed(object sender, System.EventArgs e) {
        if (BattleManager.Instance == null) return;
        if (MousePositionManager.Instance.IsPointerOverUIElement()) return;

        GridPosition mouseGridPosition = MousePositionManager.Instance.GetMouseGridPosition();
        Troop troop = BattleGrid.Instance.GetTroopAtGridPosition(mouseGridPosition);
        Building building = BattleGrid.Instance.GetBuildingAtGridPosition(mouseGridPosition);

        if (troop != null || building != null) {
            Show();

            if(troop != null) {
                SetDescriptionSlot(troop.GetTroopSO(), troop.GetUnitInTroopList()[0].GetUnitSO(), false, false);
            }
            if(building != null) {
                SetDescriptionSlot(building.GetBuildingSO());
            }
        } else {
            Hide();
        }

    }

    private void GameInput_OnLeftClickPerformed(object sender, System.EventArgs e) {

    }

    private void SetFactionVisuals(TroopSO troopSO = null, BuildingSO buildingSO = null) {
        Deck playerDeck = DeckManager.LocalInstance.GetDeckSelected();
        FactionSO deckFactionSO = playerDeck.deckFactionSO;

        if (troopSO != null && troopSO.troopFactionSO.name != "Mercenaries") {
            deckFactionSO = troopSO.troopFactionSO;
        }

        if (buildingSO != null) {
            deckFactionSO = buildingSO.buildingFactionSO;
        }

        backgroundImage.sprite = deckFactionSO.panelBackground;
        outlineImage.sprite = deckFactionSO.panelBackgroundBorderSimple;
        outlineShadowImage.sprite = deckFactionSO.panelBackgroundBorderSimple;
        statPanelBackgroundImage.sprite = deckFactionSO.slotBackground;
        statPanelOutlineImage.sprite = deckFactionSO.slotBorder;
        attackPanelBackgroundImage.sprite = deckFactionSO.slotBackground;
        attackPanelOutlineImage.sprite = deckFactionSO.slotBorder;
        buildingPanelBackgroundImage.sprite = deckFactionSO.slotBackground;
        buildingPanelOutlineImage.sprite = deckFactionSO.slotBorder;
        switchAttacksButtonBackgroundImage.sprite = deckFactionSO.slotBackground;
        switchAttacksButtonOutlineImage.sprite = deckFactionSO.slotBorder;
        showOtherTroop1ButtonOutlineImage.sprite = deckFactionSO.slotBorder;
        showOtherTroop1ButtonBackgroundImage.sprite = deckFactionSO.slotBackground;
        showOtherTroop2ButtonOutlineImage.sprite = deckFactionSO.slotBorder;
        showOtherTroop2ButtonBackgroundImage.sprite = deckFactionSO.slotBackground;

        if(keywordDescriptionBackgroundImage != null) {
            keywordDescriptionBackgroundImage.sprite = deckFactionSO.slotBackground;
        }

        Color keywordColor = deckFactionSO.color_samePlayerFaction_Opponent_fill;
        keywordColor.a = 1f;
        keywordText.color = keywordColor;
        keywordDescriptionTemplate.Find("KeyWordName").GetComponent<TextMeshProUGUI>().color = keywordColor;
    }

    public void SetDescriptionSlot(TroopSO troopSO, UnitSO unitSO, bool shownFromBuilding = false, bool shownFromTroop = false) {

        unitCostGameObject.gameObject.SetActive(true);
        unitArmorGameObject.SetActive(true);
        unitMoveSpeedGameObject.SetActive(true);
        unitNumberGameObject.SetActive(true);
        otherTroop1GameObject.SetActive(false);
        otherTroop2GameObject.SetActive(false);
        attackDescriptionPanel.gameObject.SetActive(true);
        buildingDescriptionPanel.gameObject.SetActive(false);

        cardIllustrationImage.sprite = troopSO.troopDescriptionSlotSprite;
        nameText.text = troopSO.troopName;
        costText.text = troopSO.spawnTroopCost.ToString();
        numberText.text = "x" + troopSO.troopPrefab.GetComponent<Troop>().GetBaseUnitPositions().Count.ToString();
        healthText.text = unitSO.HP.ToString();
        armorText.text = unitSO.armor.ToString();
        moveSpeedText.text = unitSO.unitMoveSpeed.ToString();
        descriptionText.text = troopSO.troopDescription;

        RefreshAttackTypeButtons(unitSO);
        RefreshKeywords(unitSO);
        SetAttackStats(unitSO.mainAttackSO);
        SetAttackTargetTypesStats(unitSO.mainAttackSO);
        SetMoveTypeStats(unitSO);
        SetAttackTypeStats(unitSO.mainAttackSO);
        SetStatusEffectStats(unitSO.mainAttackSO);
        SetChildTroopSO(troopSO);
        SetFactionVisuals(troopSO);

        currentAttackSO = unitSO.mainAttackSO;
        currentUnitSO = unitSO;
        currentTroopSO = troopSO;

        if (shownFromBuilding) {
            unitCostGameObject.gameObject.SetActive(false);
            unitNumberGameObject.gameObject.SetActive(false);
            otherTroop1GameObject.SetActive(true);
            showOtherTroop1Button.SetBuildingSO(currentBuildingSO);
            OtherTroop1NumberText.text = "";
        }

        if (!shownFromTroop) {
            parentTroopSO = troopSO;
        }

        if (shownFromTroop && parentTroopSO == troopSO) {
            shownFromTroop = false;
        }

        if (shownFromTroop) {
            nameText.text = troopSO.troopName + " (" + parentTroopSO.troopName + ")";
            unitCostGameObject.gameObject.SetActive(false);
            unitNumberGameObject.gameObject.SetActive(false);
            otherTroop1GameObject.SetActive(true);
            showOtherTroop1Button.SetChildTroopSO(parentTroopSO);
            OtherTroop1NumberText.text = "";
        }
    }

    public void SetDescriptionSlot(BuildingSO buildingSO) {
        currentBuildingSO = buildingSO;

        unitCostGameObject.gameObject.SetActive(true);
        unitArmorGameObject.SetActive(false);
        unitMoveSpeedGameObject.SetActive(false);
        unitNumberGameObject.SetActive(false);
        attackDescriptionPanel.gameObject.SetActive(false);
        buildingDescriptionPanel.gameObject.SetActive(true);
        otherTroop2GameObject.SetActive(false);

        cardIllustrationImage.sprite = buildingSO.buildingDescriptionSlotSprite;
        nameText.text = buildingSO.buildingName;
        costText.text = buildingSO.spawnBuildingCost.ToString();
        healthText.text = buildingSO.buildingHP.ToString();
        descriptionText.text = buildingSO.buildingDescription;

        if (buildingSO.garrisonedTroopSO != null) {

            //Unit has a garrisoned troop
            otherTroop1GameObject.SetActive(true);
            OtherTroop1NumberText.text = "x" + buildingSO.garrisonedTroopSO.troopPrefab.GetComponent<Troop>().GetBaseUnitPositions().Count.ToString();
            currentTroopSO = buildingSO.garrisonedTroopSO;
            showOtherTroop1Button.SetGarrisonedTroopSO(currentTroopSO);

        } else {
            otherTroop1GameObject.SetActive(false);
        }

        RefreshKeywords(buildingSO);
        SetBuildingDescriptionPanel(buildingSO);
        SetFactionVisuals(null, buildingSO);
    }

    private void RefreshAttackTypeButtons(UnitSO unitSO) {

        foreach(Transform child in changeAttackSOButtonContainer) {
            if (child == changeAttackSOButtonTemplate) continue;
            Destroy(child.gameObject);
        }

        if (unitSO.mainAttackSO != null) {
            SwitchAttackSOButton switchAttackSOButton = Instantiate(changeAttackSOButtonTemplate, changeAttackSOButtonContainer).GetComponent<SwitchAttackSOButton>();
            switchAttackSOButton.SetAttackSO(unitSO.mainAttackSO);
            switchAttackSOButton.gameObject.SetActive(true);
            switchAttackSOButton.SetSelected(true);
        }

        if (unitSO.sideAttackSO != null) {
            if (unitSO.sideAttackSO.name == "DwarfAxeSideAttack") return;
            SwitchAttackSOButton switchAttackSOButton = Instantiate(changeAttackSOButtonTemplate, changeAttackSOButtonContainer).GetComponent<SwitchAttackSOButton>();
            switchAttackSOButton.SetAttackSO(unitSO.sideAttackSO);
            switchAttackSOButton.gameObject.SetActive(true);
            switchAttackSOButton.SetSelected(false);
        }

        if (unitSO.jumpAttackSO != null) {
            SwitchAttackSOButton switchAttackSOButton = Instantiate(changeAttackSOButtonTemplate, changeAttackSOButtonContainer).GetComponent<SwitchAttackSOButton>();
            switchAttackSOButton.SetAttackSO(unitSO.jumpAttackSO);
            switchAttackSOButton.gameObject.SetActive(true);
            switchAttackSOButton.SetSelected(false);
        }

        if (unitSO.deathTriggerAttackSO != null) {
            SwitchAttackSOButton switchAttackSOButton = Instantiate(changeAttackSOButtonTemplate, changeAttackSOButtonContainer).GetComponent<SwitchAttackSOButton>();
            switchAttackSOButton.SetAttackSO(unitSO.deathTriggerAttackSO);
            switchAttackSOButton.gameObject.SetActive(true);
            switchAttackSOButton.SetSelected(false);
        }

    }

    private void SetAttackStats(AttackSO attackSO) {
        if(attackSO.attackType == AttackSO.AttackType.melee || attackSO.attackType == AttackSO.AttackType.ranged || attackSO.attackType == AttackSO.AttackType.healAllyMeleeTargeting || attackSO.attackType == AttackSO.AttackType.healAllyRangedTargeting) {
            attackSpeedGameObject.SetActive(true);
            attackDamageGameObject.SetActive(true);
            attackDamageText.text = attackSO.attackDamage.ToString();
            attackSpeedText.text = attackSO.attackRate.ToString() + "s";
        }

        if(attackSO.attackType == AttackSO.AttackType.healAllyMeleeTargeting || attackSO.attackType == AttackSO.AttackType.healAllyRangedTargeting) {
            attackSpeedGameObject.SetActive(true);
            attackDamageGameObject.SetActive(true);
            attackDamageImage.sprite = healSprite;
        } else {
            attackDamageImage.sprite = damageSprite;
        }

        if (attackSO.attackType == AttackSO.AttackType.deathTrigger ) {
            attackSpeedGameObject.SetActive(false);
            if(attackSO.attackSpecialList.Count >0) {
                attackDamageImage.sprite = GetStatusEffectSprite(attackSO.attackSpecialList[0]);
            }
        } else {
            attackDamageImage.sprite = damageSprite;
        }

        if(attackSO.attackType == AttackSO.AttackType.jump) {
            attackSpeedGameObject.SetActive(false);
            attackDamageGameObject.SetActive(true);
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

        // Pierce ?
        if (attackSO.attackSpecialList.Contains(AttackSO.UnitAttackSpecial.pierce)) {
            attackDamageImage.sprite = pierceSprite;
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

            if (attackSO.attackSpecialList[0] == AttackSO.UnitAttackSpecial.pierce) return;

            statusEffect1Text.gameObject.SetActive(true);
            statusEffect1Image.gameObject.SetActive(true);
            statusEffectGameObject.SetActive(true);

            if (attackSO.specialEffectDuration != 0) {
                statusEffect1Text.text = "+" + attackSO.specialEffectDuration.ToString();
            } else {
                statusEffect1Text.text = "+";
            }

            statusEffect1Image.sprite = GetStatusEffectSprite(attackSO.attackSpecialList[0]);

        } else {
            statusEffect1Text.gameObject.SetActive(false);
            statusEffect1Image.gameObject.SetActive(false);
            statusEffectGameObject.SetActive(false);
        }

        if (attackSO.attackSpecialList.Count > 1) {
            statusEffect2Text.gameObject.SetActive(true);
            statusEffect2Image.gameObject.SetActive(true);
            statusEffect2Text.text = "+";
            statusEffect2Image.sprite = GetStatusEffectSprite(attackSO.attackSpecialList[1]);
        } else {
            statusEffect2Text.gameObject.SetActive(false);
            statusEffect2Image.gameObject.SetActive(false);
        }

    }

    private void RefreshKeywords(UnitSO unitSO) {
        if (unitSO == currentUnitSO) return;
        keywordDescriptionContainer.gameObject.SetActive(false);

        // Refresh card keywords

        string allUnitsKeywords = "";
        foreach (UnitSO.UnitKeyword unitKeyword in unitSO.unitKeywordsList) {

                string unitKeywordString = SetKeywordName(unitKeyword);

                if (unitKeyword != unitSO.unitKeywordsList[unitSO.unitKeywordsList.Count -1]) {
                        unitKeywordString += ", ";
                }

                allUnitsKeywords += unitKeywordString;
        }
        keywordText.text = allUnitsKeywords;

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

    private void RefreshKeywords(BuildingSO buildingSO) {
        keywordDescriptionContainer.gameObject.SetActive(false);

        // Refresh card keywords

        string allUnitsKeywords = "";
        foreach (UnitSO.UnitKeyword unitKeyword in buildingSO.buildingKeyworkdList) {

            string unitKeywordString = SetKeywordName(unitKeyword);

            if (unitKeyword != buildingSO.buildingKeyworkdList[buildingSO.buildingKeyworkdList.Count - 1]) {
                unitKeywordString += ", ";
            }

            allUnitsKeywords += unitKeywordString;
        }
        keywordText.text = allUnitsKeywords;

        // Refresh description keywords
        if (buildingSO.buildingKeyworkdList.Count > 0) {
            keywordDescriptionContainer.gameObject.SetActive(true);

            foreach (Transform child in keywordDescriptionContainer) {
                if (child == keywordDescriptionTemplate) continue;
                Destroy(child.gameObject);
            }

            foreach (UnitSO.UnitKeyword unitKeyword in buildingSO.buildingKeyworkdList) {
                Transform instantiatedKeywordTemplate = Instantiate(keywordDescriptionTemplate, keywordDescriptionContainer);

                instantiatedKeywordTemplate.Find("KeyWordName").GetComponent<TextMeshProUGUI>().text = SetKeywordName(unitKeyword);
                instantiatedKeywordTemplate.Find("KeyWordDescription").GetComponent<TextMeshProUGUI>().text = SetKeywordDescription(unitKeyword);
                instantiatedKeywordTemplate.gameObject.SetActive(true);
            }
        }
    }

    private void SetChildTroopSO(TroopSO troopSO) {
        if (troopSO.additionalUnit1TroopSO != null) {
            otherTroop1GameObject.SetActive(true);
            OtherTroop1NumberText.text = "x" + troopSO.troopPrefab.GetComponent<Troop>().GetBaseUnit1Positions().Count;
            showOtherTroop1Button.SetChildTroopSO(troopSO.additionalUnit1TroopSO);
        }
        else {
            otherTroop1GameObject.SetActive(false);
        }

        if (troopSO.additionalUnit2TroopSO != null) {
            otherTroop2GameObject.SetActive(true);
            OtherTroop2NumberText.text = "x" + troopSO.troopPrefab.GetComponent<Troop>().GetBaseUnit2Positions().Count;
            showOtherTroop2Button.SetChildTroopSO(troopSO.additionalUnit2TroopSO);
        }
        else {
            otherTroop2GameObject.SetActive(false);
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

        if (attackSpecial == AttackSO.UnitAttackSpecial.webbed) {
            return slowedSprite;
        }

        if (attackSpecial == AttackSO.UnitAttackSpecial.knockback) {
            return knockbackSprite;
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
            return "Deals x2 damage to large units";
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
            return "Deals x3 damage to buildings";
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

        if (unitKeyword == UnitSO.UnitKeyword.Destructible) {
            return "Does not refill HP at the end of a battle";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Uncrossable) {
            return "Units cannot walk past this building";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Garrisoned) {
            return "Does not move during the battle";
        }

        if (unitKeyword == UnitSO.UnitKeyword.PerforatingAttack) {
            return "Attacks can hit multiple enemies at once";
        }

        if (unitKeyword == UnitSO.UnitKeyword.Trample) {
            return "Deals x2 damage to non-large units";
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

        if (unitKeyword == UnitSO.UnitKeyword.PerforatingAttack) {
            return "Perforating Attack";
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

    public void SetBuildingDescriptionPanel(BuildingSO buildingSO) {

        if(buildingSO.buildingName == "Farm" || buildingSO.buildingName == "Mine" || buildingSO.buildingName == "Wolf Den" || buildingSO.buildingName == "Blood Altar") {

            economicalBuildingGameObject.SetActive(true);

            if(buildingSO.buildingName == "Farm" || buildingSO.buildingName == "Mine") {
                economicalBuildingRevenueText.text = "+" + buildingSO.economicalBuildingRevenue.ToString();
                economicalBuildingImage.sprite = coinSprite;
                economicalBuildingDescriptionText.text = "/turn";
            }

            if (buildingSO.buildingName == "Wolf Den") {
                economicalBuildingRevenueText.text = "+" + buildingSO.economicalBuildingRevenue.ToString();
                economicalBuildingImage.sprite = elfWolfDenWolfSprite;
                economicalBuildingDescriptionText.text = "/turn";
            }

            if (buildingSO.buildingName == "Blood Altar") {
                economicalBuildingRevenueText.text = "+" + buildingSO.economicalBuildingRevenue.ToString();
                economicalBuildingImage.sprite = elfWolfDenWolfSprite;
                economicalBuildingImage.sprite = coinSprite;
                economicalBuildingDescriptionText.text = "/slay";
            }

        } else {
            economicalBuildingGameObject.SetActive(false);
        }

        if (buildingSO.buildingName == "Tree Wall") {
            treeWallGameObject.SetActive(true);
        } else {
            treeWallGameObject.SetActive(false);
        }

        if (buildingSO.buildingName == "Trapped Wall") {
            trappedWallGameObject.SetActive(true);
        }
        else {
            trappedWallGameObject.SetActive(false);
        }

        if (buildingSO.buildingName == "Spiked Wall" || buildingSO.buildingName == "Spiked Tower") {
            reflectDamageGameObject.SetActive(true);
            reflectDamageAmountText.text = buildingSO.reflectMeleeDamageAmount.ToString();
        }
        else {
            reflectDamageGameObject.SetActive(false);
        }
    }

    public void Show() {
        descriptionCardGameObject.SetActive(true);
        cardOpened = true;
    }

    public void Hide() {
        if (pointerEntered) return;
        cardOpened = false;

        if (animator != null) {
            StartCoroutine(HideCoroutine());
        } else {
            descriptionCardGameObject.SetActive(false);
        }
    }

    private IEnumerator HideCoroutine() {

        animator.SetTrigger("Close");

        yield return new WaitForSeconds(.3f);
        descriptionCardGameObject.SetActive(false);
    }

    private void DeckManager_OnSelectedDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        SetFactionVisuals();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        pointerEntered = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        pointerEntered = false;
    }

    public bool GetPointerEntered() {
        return pointerEntered;
    }

    public bool GetCardOpen() {
        return cardOpened;
    }
}
