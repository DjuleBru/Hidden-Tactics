using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{

    public static MainMenuUI Instance;

    [SerializeField] GameObject mainMenuButtonsGameObject;
    [SerializeField] GameObject updatesPanelGameObject;
    [SerializeField] GameObject lobbyPanelGameObject;
    [SerializeField] GameObject playButtonsGameObject;

    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button collectionButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button exitGameButton;

    [SerializeField] private Image deckNameOutline;

    [SerializeField] private TextMeshProUGUI deckNameText;

    [SerializeField] private Animator leftPanelAnimator;
    [SerializeField] private Animator mainMenuButtonsAnimator;
    [SerializeField] private Animator playButtonsAnimator;
    private Animator mainMenuAnimator;

    private void Awake() {
        Instance = this;
        mainMenuAnimator = GetComponent<Animator>();
        exitGameButton.onClick.AddListener(() => {
            Application.Quit();
        });

        playButton.onClick.AddListener(() => {
            StartCoroutine(ShowLobbyPanel(.7f));
        });

        playButtonsGameObject.SetActive(false);
        lobbyPanelGameObject.SetActive(false);
        leftPanelAnimator.SetTrigger("Down");
        mainMenuButtonsAnimator.Play("MainMenuButtons_Idle_Unfolded");
        playButtonsAnimator.Play("MainMenuButtons_Idle_Folded");
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

    private IEnumerator ShowLobbyPanel(float delatToLeftPanelDown) {

        leftPanelAnimator.SetTrigger("Up");
        mainMenuButtonsAnimator.SetTrigger("Fold");

        yield return new WaitForSeconds(delatToLeftPanelDown);

        mainMenuButtonsGameObject.SetActive(false);
        lobbyPanelGameObject.SetActive(true);
        updatesPanelGameObject.SetActive(false);
        playButtonsGameObject.SetActive(true);

        leftPanelAnimator.SetTrigger("Down");
        playButtonsAnimator.SetTrigger("Unfold");

    }

}
