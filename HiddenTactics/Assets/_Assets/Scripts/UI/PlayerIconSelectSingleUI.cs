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

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material hoveredMaterial;

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
            backgroundImage.material = selectedMaterial;
            outlineImage.material = selectedMaterial;
        } else {
            backgroundImage.material = cleanMaterial;
            outlineImage.material = cleanMaterial;
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

        backgroundImage.material = cleanMaterial;
        outlineImage.material = cleanMaterial;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (isSelected) return;
        backgroundImage.material = hoveredMaterial;
        outlineImage.material = hoveredMaterial;
    }
}
