using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIconSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int iconSpriteId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;

    private bool isSelected = false;
    private CustomizationUI customizationUI;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            HiddenTacticsMultiplayer.Instance.SetPlayerIconSprite(iconSpriteId);
            UpdateIsSelected();
        });
    }

    private void Start() {
        customizationUI = GetComponentInParent<CustomizationUI>();

        image.sprite = HiddenTacticsMultiplayer.Instance.GetPlayerSprite(iconSpriteId);
        selectedGameObject.SetActive(false);
    }

    private void UpdateIsSelected() {

        foreach(PlayerIconSelectSingleUI playerIconSelectSingleUI in customizationUI.GetPlayerIconsArray()) {
            if(playerIconSelectSingleUI.GetIsSelected()) {
                playerIconSelectSingleUI.SetIsSelected(false);
            }
        }

        isSelected = true;
        selectedGameObject.SetActive(isSelected);
    }

    public bool GetIsSelected() {
        return isSelected;
    }
    public void SetIsSelected(bool isSelected) {
        this.isSelected = isSelected;
        selectedGameObject.SetActive(isSelected);
    }
}