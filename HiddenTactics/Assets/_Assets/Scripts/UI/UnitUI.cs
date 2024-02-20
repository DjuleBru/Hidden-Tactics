using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private GameObject unitHPBarGameObject;
    [SerializeField] private Image unitHPBarImage;


    private void Start() {
        unit.OnHealthChanged += Unit_OnDamageTaken;
        unit.OnUnitDied += Unit_OnUnitDied;
    }

    private void Unit_OnUnitDied(object sender, System.EventArgs e) {
        unitHPBarGameObject.SetActive(false);
    }

    private void Unit_OnDamageTaken(object sender, System.EventArgs e) {
        UpdateUnitHPBar();
    }

    private void UpdateUnitHPBar() {
        unitHPBarImage.fillAmount = unit.GetUnitHPNormalized();
    }
}
