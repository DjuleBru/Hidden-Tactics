using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreparationPhaseUI : MonoBehaviour
{

    [SerializeField] Image preparationPhaseTimerImage;
    [SerializeField] Animator preparationPhasePanelAnimator;
    [SerializeField] Animator mercenariesPanelAnimator;
    [SerializeField] Animator battleDeckUIPanelAnimator;
    [SerializeField] Button playerReadyButton;

    [SerializeField] GameObject playerReadyIndicatorPlayerPanel;
    [SerializeField] GameObject playerReadyIndicatorBattlePhasePanel;
    [SerializeField] GameObject opponentReadyIndicatorPlayerPanel;
    [SerializeField] GameObject opponentReadyIndicatorBattlePhasePanel;

    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private GameObject allPlaceablesUI;
    [SerializeField] private GameObject allPreparationPhasePanelsUI;

    private bool panelOpen = true;
    [SerializeField] private bool debugMode;
    [SerializeField] private GameObject openCloseAllPlaceablesButton;

    private void Awake() {
        playerReadyButton.onClick.AddListener(() => {
            Player.LocalInstance.SetPlayerReadyOrUnready();
            playerReadyButton.GetComponent<ReadyPanelButtonUI>().HandleButtonClickVisual();
        });

        if(debugMode) {
            openCloseAllPlaceablesButton.SetActive(true);
        } else {
            openCloseAllPlaceablesButton.SetActive(false);
        }
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        BattleManager.Instance.OnAllIPlaceablesSpawned += BattleManager_OnAllIPlaceablesSpawned;
        PlayerReadyManager.Instance.OnReadyChanged += PlayerReadyManager_OnReadyChanged;
        turnText.text = "I/" + ConvertIntToRomanNumber(BattleManager.Instance.GetMaxTurns());
        allPreparationPhasePanelsUI.SetActive(false);
    }


    private void Update() {
        preparationPhaseTimerImage.fillAmount = BattleManager.Instance.GetPreparationPhaseTimerNormalized();
    }

    private void BattleManager_OnAllIPlaceablesSpawned(object sender, System.EventArgs e) {
        StartCoroutine(ShowPreparationPhaseUIAfterDelay(2f));
    }

    private IEnumerator ShowPreparationPhaseUIAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        allPreparationPhasePanelsUI.SetActive(true);
        Show();
    }

    private void PlayerReadyManager_OnReadyChanged(object sender, System.EventArgs e) {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetLocalPlayerData();
        PlayerData opponentData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData();

        playerReadyIndicatorPlayerPanel.SetActive(PlayerReadyManager.Instance.IsPlayerReady(playerData.clientId));
        opponentReadyIndicatorPlayerPanel.SetActive(PlayerReadyManager.Instance.IsPlayerReady(opponentData.clientId));

        playerReadyIndicatorBattlePhasePanel.SetActive(PlayerReadyManager.Instance.IsPlayerReady(playerData.clientId));
        opponentReadyIndicatorBattlePhasePanel.SetActive(PlayerReadyManager.Instance.IsPlayerReady(opponentData.clientId));
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (BattleManager.Instance.IsPreparationPhase()) {
            Show();
            turnText.text = ConvertIntToRomanNumber(BattleManager.Instance.GetCurrentTurn() +1) + "/" + ConvertIntToRomanNumber(BattleManager.Instance.GetMaxTurns());
            return;
        }

        if(BattleManager.Instance.IsBattlePhaseStarting()) {
            Hide();
            return;
        }

        if(BattleManager.Instance.GameEnded()) {
            Hide();
            return;
        }

    }

    public void Show() {
        preparationPhasePanelAnimator.ResetTrigger("SlideUp");
        preparationPhasePanelAnimator.SetTrigger("SlideDown");
        battleDeckUIPanelAnimator.SetTrigger("Show");
        battleDeckUIPanelAnimator.ResetTrigger("Hide");

        mercenariesPanelAnimator.SetTrigger("Show");
        mercenariesPanelAnimator.ResetTrigger("Hide");

        if(BattleManager.Instance.IsFirstPreparationPhase()) {
            MercenariesUI.Instance.SetPreparationPhaseUIReady();
        }
    }

    private void Hide() {
        preparationPhasePanelAnimator.ResetTrigger("SlideDown");
        preparationPhasePanelAnimator.SetTrigger("SlideUp");

        mercenariesPanelAnimator.SetTrigger("Hide");
        battleDeckUIPanelAnimator.SetTrigger("Hide");

        mercenariesPanelAnimator.ResetTrigger("Show");
        battleDeckUIPanelAnimator.ResetTrigger("Show");
    }

    
    private string ConvertIntToRomanNumber(int intToConvert) {
        if (intToConvert == 0) {
            return "I";
        }
        if (intToConvert == 1) {
            return "I";
        }
        if (intToConvert == 2) {
            return "II";
        }
        if (intToConvert == 3) {
            return "III";
        }
        if (intToConvert == 4) {
            return "IV";
        }
        if (intToConvert == 5) {
            return "V";
        }
        if (intToConvert == 6) {
            return "VI";
        }
        if (intToConvert == 7) {
            return "VII";
        }
        if (intToConvert == 8) {
            return "VIII";
        }
        if (intToConvert == 9) {
            return "IX";
        }
        if (intToConvert == 10) {
            return "X";
        }
        return "";
    }

    public void OpenClosePanel() {
        panelOpen = !panelOpen;
        allPlaceablesUI.SetActive(panelOpen);
    }
}
