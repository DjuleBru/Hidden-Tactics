using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeDeckFactionButtonUI : MonoBehaviour
{
    [SerializeField] private FactionSO factionSO;
    [SerializeField] private Image factionImage;
    [SerializeField] private Image factionImageShadow;
    [SerializeField] private Image factionBackgroundImage;
    [SerializeField] private Image factionOutlineImage;
    [SerializeField] private Image factionOutlineShadowImage;
    private Button button;

    private bool selected;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            if (!selected) {
                DeckEditUI.Instance.OpenChangeDeckFactionContainer();
                selected = true;
            }
            else {
                DeckEditUI.Instance.CloseChangeDeckFactionContainer();
                selected = false;
            }
        });
    }

    public void SetSelected(bool selected) {
        this.selected = selected;
    }

    public void SetFactionSO(FactionSO factionSO) {
        this.factionSO = factionSO;
        factionImage.sprite = factionSO.factionSprite;
        factionImageShadow.sprite = factionSO.factionSprite;
        factionBackgroundImage.sprite = factionSO.slotBackgroundSquare;
        factionOutlineImage.sprite = factionSO.slotBorder;
        factionOutlineShadowImage.sprite = factionSO.slotBorder;
    }
}
