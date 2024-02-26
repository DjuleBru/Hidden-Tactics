using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : NetworkBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private GameObject unitHPBarGameObject;
    [SerializeField] private Image unitHPBarImage;
    [SerializeField] private Image unitHPBarDamageImage;

    private float damageBarUpdateTimer;
    private float damageBarUpdateRate = .8f;

    private float delayToUpdateDamageBar = .6f;

    private bool updateHPBarFinished;
    private float updateHPBarDuration = 1f;
    private float updateHPBarTimer;

    private float hideHPBarTimer;
    private float hideHPBarDuration = 1f;

    private void Awake() {
        unitHPBarGameObject.SetActive(false);
        hideHPBarTimer = hideHPBarDuration;
        damageBarUpdateTimer = delayToUpdateDamageBar;
    }

    public override void OnNetworkSpawn() {
        unit.OnHealthChanged += Unit_OnHealthChanged;
        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitReset += Unit_OnUnitReset;
        unit.OnUnitSetAsAdditionalUnit += Unit_OnUnitSetAsAdditionalUnit;
    }

    private void Update() {
        if(!updateHPBarFinished) {
            updateHPBarTimer -= Time.deltaTime;
            damageBarUpdateTimer -= Time.deltaTime;

            if(updateHPBarDuration < 0) {
                updateHPBarFinished = true;
            }


            if(damageBarUpdateTimer < 0) {
                if (unitHPBarDamageImage.fillAmount > unit.GetUnitHP()/unit.GetUnitMaxHP()) {
                    unitHPBarDamageImage.fillAmount = unitHPBarDamageImage.fillAmount - damageBarUpdateRate * Time.deltaTime;
                }
            }

        } else {
            hideHPBarTimer -= Time.deltaTime;
            if(hideHPBarDuration < 0) {
                unitHPBarGameObject.SetActive(false);
            }
        }
    }

    private void Unit_OnUnitDied(object sender, System.EventArgs e) {
        unitHPBarGameObject.SetActive(false);
    }

    private void Unit_OnHealthChanged(object sender, Unit.OnHealthChangedEventArgs e) {
        UpdateUnitHPBar(e.previousHealth, e.newHealth);
    }

    private void Unit_OnUnitSetAsAdditionalUnit(object sender, System.EventArgs e) {
        unitHPBarGameObject.SetActive(false);
    }

    private void Unit_OnUnitReset(object sender, System.EventArgs e) {
        if (!unit.GetUnitIsBought()) return;
        unitHPBarGameObject.SetActive(false);
    }

    private void UpdateUnitHPBar(float initialUnitHP, float newUnitHP) {
        unitHPBarGameObject.SetActive(true);
        updateHPBarFinished = false;
        hideHPBarTimer = hideHPBarDuration;

        damageBarUpdateTimer = delayToUpdateDamageBar;

        unitHPBarGameObject.SetActive(true);
        unitHPBarImage.fillAmount = newUnitHP / unit.GetUnitMaxHP();

        if(updateHPBarTimer > 0) {
            updateHPBarTimer = updateHPBarDuration;
        }
    }



}
