using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckSlotButtonUI : MonoBehaviour
{
    private Button button;
    [SerializeField] private int deckSlotNumber;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            DeckManager.LocalInstance.SetDeckSelected(deckSlotNumber);
        });
    }
}
