using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePhaseUI : MonoBehaviour
{
    [SerializeField] Image battlePhaseTimerImage;
    [SerializeField] Button playerSpeedUpButton;
    [SerializeField] GameObject playerSpeedUpGameObject;

    [SerializeField] GameObject playerSpeedUpIndicator;
    [SerializeField] GameObject opponentSpeedUpIndicator;

    [SerializeField] private TextMeshProUGUI turnText;

    private void Awake() {
        playerSpeedUpButton.onClick.AddListener(() => {
            Player.LocalInstance.SetPlayerWantsToSpeedUp();
            playerSpeedUpButton.GetComponent<ReadyPanelButtonUI>().HandleButtonClickVisual();
        });

        playerSpeedUpGameObject.SetActive(false);
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        BattleManager.Instance.OnSpeedUpButtonActivation += BattleManager_OnSpeedUpButtonActivation;
        PlayerReadyManager.Instance.OnAllPlayersWantToSpeedUp += PlayerReadyManager_OnAllPlayersWantToSpeedUp;
        PlayerReadyManager.Instance.OnPlayerWantsToSpeedUpChanged += PlayerReadyManager_OnPlayerWantsToSpeedUpChanged;

        turnText.text = "I/" + ConvertIntToRomanNumber(BattleManager.Instance.GetMaxTurns());
    }


    private void Update() {
        battlePhaseTimerImage.fillAmount = BattleManager.Instance.GetBattlePhaseTimerNormalized();
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {

        if (BattleManager.Instance.IsBattlePhase()) {
            Show(); 
            turnText.text = ConvertIntToRomanNumber(BattleManager.Instance.GetCurrentTurn()) + "/" + ConvertIntToRomanNumber(BattleManager.Instance.GetMaxTurns());
            playerSpeedUpGameObject.SetActive(false);
        } else {
            Hide();
        }
    }

    private void BattleManager_OnSpeedUpButtonActivation(object sender, System.EventArgs e) {
        playerSpeedUpGameObject.SetActive(true);
        playerSpeedUpButton.enabled = true;
    }

    private void PlayerReadyManager_OnAllPlayersWantToSpeedUp(object sender, System.EventArgs e) {
        playerSpeedUpButton.enabled = false;
        playerSpeedUpGameObject.GetComponent<Animator>().SetTrigger("SlideUp");
    }

    private void PlayerReadyManager_OnPlayerWantsToSpeedUpChanged(object sender, System.EventArgs e) {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetLocalPlayerData();
        PlayerData opponentData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData();

        playerSpeedUpIndicator.SetActive(PlayerReadyManager.Instance.PlayerWantingToSpeedUp(playerData.clientId));
        opponentSpeedUpIndicator.SetActive(PlayerReadyManager.Instance.PlayerWantingToSpeedUp(opponentData.clientId));
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
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
}
