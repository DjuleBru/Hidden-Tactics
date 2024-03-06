using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    [SerializeField] private Building building;
    [SerializeField] private BuildingHP buildingHP;
    [SerializeField] private GameObject buildingHPBarGameObject;
    [SerializeField] private Image buildingHPBarImage;

    [SerializeField] private TextMeshPro buildingHPText;

    private void Awake() {
        buildingHPBarGameObject.SetActive(false);
    }

    private void Start() {
        buildingHP.OnHealthChanged += BuildingHP_OnHealthChanged;
    }

    private void BuildingHP_OnHealthChanged(object sender, BuildingHP.OnHealthChangedEventArgs e) {
        UpdateHealthBar(e.previousHealth, e.newHealth);
    }

    private void UpdateHealthBar(float initialHP, float newHP) {
        buildingHPBarGameObject.SetActive(true);
        buildingHPBarImage.fillAmount = newHP/ buildingHP.GetMaxHP();
        buildingHPText.text = Mathf.RoundToInt((newHP / buildingHP.GetMaxHP())*100).ToString();
    }
}
