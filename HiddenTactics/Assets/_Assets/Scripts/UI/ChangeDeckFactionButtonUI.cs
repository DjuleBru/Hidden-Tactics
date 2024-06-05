using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeDeckFactionButtonUI : MonoBehaviour
{

    public static ChangeDeckFactionButtonUI Instance;

    [SerializeField] private FactionSO factionSO;
    [SerializeField] private Image factionImage;
    private Button button;

    private bool selected;

    private void Awake() {
        Instance = this;

        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            if (!selected) {
                DeckVisualUI.Instance.OpenChangeDeckFactionContainer();
                selected = true;
            }
            else {
                DeckVisualUI.Instance.CloseChangeDeckFactionContainer();
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
        DeckVisualUI.Instance.CloseChangeDeckFactionContainer();
    }
}
