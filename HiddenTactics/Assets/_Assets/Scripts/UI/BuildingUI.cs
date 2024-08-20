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
    [SerializeField] private GameObject buildingSelectedUI;
    [SerializeField] private Image buildingHPBarImage;

    [SerializeField] private Image buildingTargetImage;
    [SerializeField] private Sprite coinSprite;


    private void Awake() {
        buildingHPBarGameObject.SetActive(false);
        buildingSelectedUI.SetActive(false);
        HideBuildingTargetUI();
    }

    public override void OnNetworkSpawn() {
        buildingHP.OnHealthChanged += BuildingHP_OnHealthChanged;
        building.OnBuildingDestroyed += Building_OnBuildingDestroyed;
        building.OnBuildingPlaced += Building_OnBuildingPlaced;
        building.OnBuildingSelled += Building_OnBuildingSelled;
    }

    private void Building_OnBuildingSelled(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
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
    }

    public void ShowBuildingSelectedUI() {
        if (!building.IsOwnedByPlayer()) return;
        buildingSelectedUI.gameObject.SetActive(true);
    }

    public void HideBuildingSelectedUI() {
        if (!building.IsOwnedByPlayer()) return;
        buildingSelectedUI.gameObject.SetActive(false);
    }

    public void ShowUnitAsSellingBuilding() {
        buildingTargetImage.sprite = coinSprite;
        buildingTargetImage.gameObject.SetActive(true);
    }

    public void HideBuildingTargetUI() {
        buildingTargetImage.gameObject.SetActive(false);
    }
}
