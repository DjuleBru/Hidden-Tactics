using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionSelectionButtonUI : MonoBehaviour
{
    [SerializeField] private FactionSO factionSO;
    [SerializeField] private Image factionImage;
    [SerializeField] private Image factionImageShadow;
    [SerializeField] private Image factionOutlineImage;
    [SerializeField] private Image factionOutlineShadowImage;
    [SerializeField] private Image factionBackgroundImage;

    [SerializeField] private ChangeDeckFactionButtonUI changeDeckFactionButtonUI;
    private Button button;

    private void Awake() {
        button = GetComponent<Button>();

        button.onClick.AddListener(() => {
            PlayerCustomizationUI.Instance.CloseChangeDeckFactionContainer();
            DeckEditUI.Instance.CloseChangeDeckFactionContainer();
            DeckManager.LocalInstance.SetDeckSelected(factionSO);
            if(changeDeckFactionButtonUI != null) {
                changeDeckFactionButtonUI.SetSelected(false);
            }
        });

        if(factionSO != null) {
            factionImage.sprite = factionSO.factionSprite;
            factionImageShadow.sprite = factionSO.factionSprite;
            factionOutlineImage.sprite = factionSO.slotBorder;
            factionOutlineShadowImage.sprite = factionSO.slotBorder;
            factionBackgroundImage.sprite = factionSO.slotBackgroundSquare;
            factionImage.preserveAspect = true;
        }
    }

    public void SetFactionSO(FactionSO factionSO) {
        this.factionSO = factionSO;
        factionImage.sprite = factionSO.factionSprite;
        factionImageShadow.sprite = factionSO.factionSprite;
        factionOutlineImage.sprite = factionSO.slotBorder;
        factionOutlineShadowImage.sprite = factionSO.slotBorder;
        factionBackgroundImage.sprite = factionSO.slotBackgroundSquare;
    }


}
