using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{

    public static MainMenuUI Instance;

    [SerializeField] Button exitGameButton;

    [SerializeField] private Image deckNameOutline;

    [SerializeField] private TextMeshProUGUI deckNameText;

    [SerializeField] private Animator playerPanelAnimator;
    private Animator mainMenuAnimator;

    private void Awake() {
        Instance = this;
        mainMenuAnimator = GetComponent<Animator>();
        exitGameButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }

    private void Start() {
        DeckManager.LocalInstance.OnSelectedDeckChanged += DeckManager_OnSelectedDeckChanged;
        RefreshDeckVisuals(DeckManager.LocalInstance.GetDeckSelected());
    }

    private void DeckManager_OnSelectedDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshDeckVisuals(e.selectedDeck);
    }

    private void RefreshDeckVisuals(Deck deck) {
        deckNameOutline.sprite = deck.deckFactionSO.slotBorder;
        deckNameText.text = deck.deckName;
    }

    public void MainMenuUIToEditDeckTransition() {
        mainMenuAnimator.SetTrigger("ToEditDeck");
    }

    public void MainMenuUIFromEditDeckTransition() {
        mainMenuAnimator.SetTrigger("FromEditDeck");
    }
    public void MainMenuUIToEditBattlefieldTransition() {
        mainMenuAnimator.SetTrigger("ToEditBattlefield");
    }

    public void MainMenuUIFromEditBattlefieldTransition() {
        mainMenuAnimator.SetTrigger("FromEditBattlefield");
    }

}
