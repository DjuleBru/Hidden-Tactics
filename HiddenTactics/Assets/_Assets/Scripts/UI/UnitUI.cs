using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : NetworkBehaviour
{
    [SerializeField] private Unit unit;
    private UnitHP unitHP;

    [SerializeField] private Image unitPoisonBarImage;
    [SerializeField] private GameObject unitPoisonBarGameObject;

    [SerializeField] private Image unitBurningBarImage;
    [SerializeField] private GameObject unitBurningBarGameObject;

    [SerializeField] private Image unitFearBarImage;
    [SerializeField] private GameObject unitFearBarGameObject;

    [SerializeField] private Image unitHPBarImage;
    [SerializeField] private GameObject unitHPBarGameObject;
    [SerializeField] private Image unitHPBarDamageImage;
    [SerializeField] private Image unitHPBarHealImage;
    [SerializeField] private Image unitTargetImage;

    [SerializeField] private Sprite meleeTargetSprite;
    [SerializeField] private Sprite rangedTargetSprite;
    [SerializeField] private Sprite healTargetSprite;
    [SerializeField] private Sprite armorTargetSprite;
    [SerializeField] private Sprite coinSprite;
    [SerializeField] private Sprite webbedSprite;

    [SerializeField] private SpriteRenderer selectedCircleSpriteRenderer;

    private float damageBarUpdateTimer;
    private float damageBarUpdateRate = 1f;

    private float delayToUpdateDamageBar = .3f;

    private bool updateHPBarFinished;
    private bool unitHPBarIsActive;
    private float updateHPBarDuration = 1f;
    private float updateHPBarTimer;

    private float hideHPBarTimer;
    private float hideHPBarDuration = 1f;

    private bool poisoned;
    private bool burning;
    private bool scared;

    [SerializeField] private string debug;

    private void Awake() {
        unitHPBarGameObject.SetActive(false);
        unitTargetImage.gameObject.SetActive(false);
        hideHPBarTimer = hideHPBarDuration;
        damageBarUpdateTimer = delayToUpdateDamageBar;

        unitHP = unit.gameObject.GetComponent<UnitHP>();

        unitFearBarGameObject.SetActive(false);
        unitPoisonBarGameObject.SetActive(false);
        unitBurningBarGameObject.SetActive(false);

    }

    public override void OnNetworkSpawn() {
        unitHP.OnHealthChanged += Unit_OnHealthChanged;
        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitReset += Unit_OnUnitReset;
        unit.OnUnitSetAsAdditionalUnit += Unit_OnUnitSetAsAdditionalUnit;

        unit.OnUnitPoisoned += Unit_OnUnitPoisoned;
        unit.OnUnitPoisonedEnded += Unit_OnUnitPoisonedEnded;

        unit.OnUnitFlamed += Unit_OnUnitFlamed;
        unit.OnUnitFlamedEnded += Unit_OnUnitFlamedEnded;
        unit.OnUnitScared += Unit_OnUnitScared;
        unit.OnUnitScaredEnded += Unit_OnUnitScaredEnded;
        unit.OnUnitWebbed += Unit_OnUnitWebbed;
        unit.OnUnitWebbedEnded += Unit_OnUnitWebbedEnded;
        unit.OnUnitSold += Unit_OnUnitSold;
    }


    private void Update() {

        debug = unitHPBarIsActive.ToString();

        if (poisoned)
        {
            unitPoisonBarImage.fillAmount = unit.GetPoisonedDurationNormalized();

        }

        if (burning) {
            unitBurningBarImage.fillAmount = unit.GetBurningDurationNormalized();

        }

        if (scared) {
            unitFearBarImage.fillAmount = unit.GetScaredDurationNormalized();

        }

        if (!updateHPBarFinished) {
            updateHPBarTimer -= Time.deltaTime;
            damageBarUpdateTimer -= Time.deltaTime;

            if(updateHPBarTimer < 0) {
                updateHPBarFinished = true;
            }

            if(damageBarUpdateTimer < 0) {

                if (unitHPBarDamageImage.fillAmount > unitHP.GetHP()/unitHP.GetMaxHP()) {
                    unitHPBarDamageImage.fillAmount = unitHPBarDamageImage.fillAmount - damageBarUpdateRate * Time.deltaTime;
                }

                if (unitHPBarImage.fillAmount < unitHP.GetHP() / unitHP.GetMaxHP()) {
                    unitHPBarImage.fillAmount = unitHPBarImage.fillAmount + damageBarUpdateRate * Time.deltaTime;
                }
            }

        } else {

            if (!unitHPBarIsActive || unitHP.GetHP() <= unitHP.GetMaxHP()) return;

            hideHPBarTimer -= Time.deltaTime;
            if(hideHPBarTimer < 0) {
                unitHPBarGameObject.SetActive(false);
                unitHPBarIsActive = false;
            }
        }
    }

    private void Unit_OnUnitSold(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }
    private void Unit_OnUnitDied(object sender, System.EventArgs e) {
        unitHPBarGameObject.SetActive(false);
        unitHPBarIsActive = false;
    }

    private void Unit_OnHealthChanged(object sender, UnitHP.OnHealthChangedEventArgs e) {
        UpdateUnitHPBar(e.previousHealth, e.newHealth);
    }

    private void Unit_OnUnitScaredEnded(object sender, System.EventArgs e) {
        scared = false;
        unitFearBarGameObject.SetActive(false);
    }

    private void Unit_OnUnitScared(object sender, Unit.OnUnitSpecialEventArgs e) {
        scared = true;
        unitFearBarGameObject.SetActive(true);
    }

    private void Unit_OnUnitFlamedEnded(object sender, System.EventArgs e) {
        burning = false;
        unitBurningBarGameObject.SetActive(false);
    }

    private void Unit_OnUnitFlamed(object sender, Unit.OnUnitSpecialEventArgs e) {
        burning = true;
        unitBurningBarGameObject.SetActive(true);
    }

    private void Unit_OnUnitPoisonedEnded(object sender, System.EventArgs e) {
        poisoned = false;
        unitPoisonBarGameObject.gameObject.SetActive(false);
    }

    private void Unit_OnUnitPoisoned(object sender, Unit.OnUnitSpecialEventArgs e) {
        poisoned = true;
        unitPoisonBarGameObject.gameObject.SetActive(true);
    }
    private void Unit_OnUnitWebbedEnded(object sender, System.EventArgs e) {
        unitTargetImage.gameObject.SetActive(false);
    }

    private void Unit_OnUnitWebbed(object sender, Unit.OnUnitSpecialEventArgs e) {
        unitTargetImage.gameObject.SetActive(true);
        unitTargetImage.sprite = webbedSprite;

    }

    private void Unit_OnUnitSetAsAdditionalUnit(object sender, System.EventArgs e) {
        unitHPBarGameObject.SetActive(false);
    }

    private void Unit_OnUnitReset(object sender, System.EventArgs e) {
        if (!unit.GetUnitIsBought() || unit.GetUnitIsDynamicallySpawnedUnit()) return;
        unitHPBarGameObject.SetActive(true);
        hideHPBarTimer = hideHPBarDuration;
        StartCoroutine(RefillHPBars());
    }

    private IEnumerator RefillHPBars() {
        // TO DO : change HP bar color to green and refill them
        yield return new WaitForSeconds(.1f);
        unitHPBarGameObject.SetActive(false);
    }

    private void UpdateUnitHPBar(float initialUnitHP, float newUnitHP) {
        unitHPBarGameObject.SetActive(true);
        unitHPBarIsActive = true;
        updateHPBarFinished = false;
        hideHPBarTimer = hideHPBarDuration;

        damageBarUpdateTimer = delayToUpdateDamageBar;

        unitHPBarGameObject.SetActive(true);
        unitHPBarIsActive = true;

        if(newUnitHP < initialUnitHP) {
            unitHPBarImage.fillAmount = newUnitHP / unitHP.GetMaxHP();
            unitHPBarHealImage.fillAmount = newUnitHP / unitHP.GetMaxHP();

            updateHPBarTimer = updateHPBarDuration;

        } else {
            unitHPBarHealImage.fillAmount = newUnitHP / unitHP.GetMaxHP();
            updateHPBarTimer = updateHPBarDuration;
            
        }
    }

    public void ShowUnitAsMeleeTarget() {
        if (unit.IsOwnedByPlayer()) return;
        unitTargetImage.sprite = meleeTargetSprite;
        unitTargetImage.gameObject.SetActive(true);
    }

    public void ShowUnitUIAsRangedTarget() {
        if (unit.IsOwnedByPlayer()) return;
        unitTargetImage.sprite = rangedTargetSprite;
        unitTargetImage.gameObject.SetActive(true);
    }

    public void ShowUnitAsHealTarget() {
        if (unit.IsOwnedByPlayer()) return;
        unitTargetImage.sprite = healTargetSprite;
        unitTargetImage.gameObject.SetActive(true);
    }

    public void ShowUnitAsArmorTarget() {
        if (unit.IsOwnedByPlayer()) return;
        unitTargetImage.sprite = armorTargetSprite;
        unitTargetImage.gameObject.SetActive(true);
    }

    public void ShowUnitAsSellingUnit() {
        unitTargetImage.sprite = coinSprite;
        unitTargetImage.gameObject.SetActive(true);
    }

    public void HideUnitTargetUI() {
        unitTargetImage.gameObject.SetActive(false);
    }
}
