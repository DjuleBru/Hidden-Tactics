using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckSlotButtonUI : MonoBehaviour
{
    private Button button;
    [SerializeField] private int deckSlotNumber;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material cleanMaterial;

    private Image deckSlotImage;

    private void Awake() {
        deckSlotImage = GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            DeckManager.LocalInstance.SetDeckSelected(deckSlotNumber);
        });
    }

    private void Start() {
        RefreshSelected();
        DeckManager.LocalInstance.OnSelectedDeckChanged += DeckManager_OnSelectedDeckChanged;
    }

    private void DeckManager_OnSelectedDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshSelected();
    }

    public void RefreshSelected() {
        if (deckSlotNumber == DeckManager.LocalInstance.GetDeckSelected().deckNumber) {
            deckSlotImage.material = selectedMaterial;
        }
        else {
            deckSlotImage.material = cleanMaterial;
        }
    }
}
