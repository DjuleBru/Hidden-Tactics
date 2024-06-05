using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionSelectionButtonUI : MonoBehaviour
{
    [SerializeField] private FactionSO factionSO;
    [SerializeField] private Image factionImage;
    private Button button;

    private void Awake() {
        button = GetComponent<Button>();

        button.onClick.AddListener(() => {
            DeckVisualUI.Instance.CloseChangeDeckFactionContainer();
            DeckManager.LocalInstance.SetDeckSelected(factionSO);
            ChangeDeckFactionButtonUI.Instance.SetSelected(false);
        });

        if(factionSO != null) {
            factionImage.sprite = factionSO.factionSprite;
        }
    }

    public void SetFactionSO(FactionSO factionSO) {
        this.factionSO = factionSO;
        factionImage.sprite = factionSO.factionSprite;
    }


}
