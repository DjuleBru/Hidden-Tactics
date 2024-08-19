using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodAltar : Building
{
    [SerializeField] private float bloodAmountToFillGold;
    [SerializeField] private Image bloodBarFill;
    [SerializeField] private Image fillingBarFill;
    [SerializeField] private ParticleSystem goldPS;
    [SerializeField] private GameObject bloodBarGameObject;

    private bool updateBloodBarFinished;
    private float bloodAmount;

    private float fillingBarUpdateTimer;
    private float bloodBarUpdateTimer;
    private float bloodBarUpdateDuration = 1f;

    private float delayToUpdateFillingBar = .3f;
    private float fillingBarUpdateRate = 1f;

    protected override void Start() {
        base.Start();
        Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;
        RefreshBloodBar();
        fillingBarFill.fillAmount = 0;
        bloodBarFill.fillAmount = 0;
    }

    protected override void Update() {
        base.Update();

        if (!updateBloodBarFinished) {

            bloodBarUpdateTimer -= Time.deltaTime;
            fillingBarUpdateTimer -= Time.deltaTime;

            if (bloodBarUpdateTimer < 0) {
                updateBloodBarFinished = true;
            }

            if (fillingBarUpdateTimer < 0) {

                if (bloodBarFill.fillAmount < bloodAmount / bloodAmountToFillGold) {
                    bloodBarFill.fillAmount = bloodBarFill.fillAmount + fillingBarUpdateRate * Time.deltaTime;
                }
            }

        }
    }

    private void Unit_OnAnyUnitDied(object sender, System.EventArgs e) {
        Unit unit = (Unit)sender;

        if(bloodAmount + unit.GetUnitSO().damageToVillages < bloodAmountToFillGold) {
            bloodAmount += unit.GetUnitSO().damageToVillages;
        } else {
            EarnGold();
            bloodAmount = unit.GetUnitSO().damageToVillages - (bloodAmountToFillGold - bloodAmount);
            bloodBarFill.fillAmount = 0;
        }
        UpdateBloodBar();
    }

    private void EarnGold() {
        goldPS.Play();
        if(IsServer) {
            HiddenTacticsMultiplayer.Instance.ChangePlayerGoldServerRpc(ownerClientId, 1);
        }
    }

    private void RefreshBloodBar() {
        bloodBarFill.fillAmount = bloodAmount/bloodAmountToFillGold;
    }

    public override void OnDestroy() {
        base.OnDestroy();
        Unit.OnAnyUnitDied -= Unit_OnAnyUnitDied;
    }

    private void UpdateBloodBar() {
        updateBloodBarFinished = false;
        fillingBarUpdateTimer = delayToUpdateFillingBar;
        bloodBarUpdateTimer = bloodBarUpdateDuration;
        fillingBarFill.fillAmount = bloodAmount / bloodAmountToFillGold;

    }
}
