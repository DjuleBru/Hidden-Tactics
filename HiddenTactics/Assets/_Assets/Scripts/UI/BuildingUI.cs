using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : NetworkBehaviour
{
    [SerializeField] private Building building;
    [SerializeField] private BuildingHP buildingHP;
    [SerializeField] private GameObject buildingHPBarGameObject;
    [SerializeField] private Image buildingHPBarImage;

    [SerializeField] private TextMeshPro buildingHPText;

    private void Awake() {
        buildingHPBarGameObject.SetActive(false);
    }

    public override void OnNetworkSpawn() {
        buildingHP.OnHealthChanged += BuildingHP_OnHealthChanged;
        building.OnBuildingDestroyed += Building_OnBuildingDestroyed;
        building.OnBuildingPlaced += Building_OnBuildingPlaced;
    }

    private void Building_OnBuildingPlaced(object sender, System.EventArgs e) {
        if(!building.IsOwnedByPlayer()) {
            buildingHPBarGameObject.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void Building_OnBuildingDestroyed(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
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
