using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerIconSelectSingleUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int iconSpriteId;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private PlayerIconSO playerIconSO;

    [SerializeField] private Color unselectedColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color hoveredColor;

    private bool isSelected = false;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            HiddenTacticsMultiplayer.Instance.SetPlayerIconSprite(iconSpriteId);
            PlayerCustomizationUI.Instance.SetSelectedIconSO(playerIconSO);
            UpdateIsSelected();
        });
    }

    private void UpdateIsSelected() {
        PlayerCustomizationUI.Instance.SetSelectedIconSO(playerIconSO);
        isSelected = true;
    }

    public bool GetIsSelected() {
        return isSelected;
    }

    public void SetIsSelected(bool isSelected) {
        this.isSelected = isSelected;

        if(isSelected) {
            backgroundImage.color = selectedColor;
            outlineImage.color = selectedColor;
        } else {
            backgroundImage.color = unselectedColor;
            outlineImage.color = unselectedColor;
        }
    }

    public void SetPlayerIcon(PlayerIconSO playerIconSO) {
        iconImage.sprite = playerIconSO.iconSprite;
        iconSpriteId = playerIconSO.iconId;
        this.playerIconSO = playerIconSO;
    }

    public PlayerIconSO GetPlayerIconSO() {
        return playerIconSO;
    }

    public void SetFactionVisuals(Deck deckSelected) {
        backgroundImage.sprite = deckSelected.deckFactionSO.slotBackgroundSquare;
        outlineImage.sprite = deckSelected.deckFactionSO.slotBorder;
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (isSelected) return;

        backgroundImage.color = unselectedColor;
        outlineImage.color = unselectedColor;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (isSelected) return;
        backgroundImage.color = hoveredColor;
        outlineImage.color = hoveredColor;
    }
}
