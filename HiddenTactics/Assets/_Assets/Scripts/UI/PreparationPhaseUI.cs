using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreparationPhaseUI : MonoBehaviour
{

    [SerializeField] Image preparationPhaseTimerImage;
    [SerializeField] Button playerReadyButton;

    [SerializeField] private TextMeshProUGUI turnText;

    private void Awake() {
        playerReadyButton.onClick.AddListener(() => {
            Player.LocalInstance.SetPlayerReadyOrUnready();
            playerReadyButton.GetComponent<ReadyPanelButtonUI>().HandleButtonClickVisual();
        });
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        turnText.text = "I/" + ConvertIntToRomanNumber(BattleManager.Instance.GetMaxTurns());
        Show();
    }

    private void Update() {
        preparationPhaseTimerImage.fillAmount = BattleManager.Instance.GetPreparationPhaseTimerNormalized();
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (BattleManager.Instance.IsPreparationPhase()) {
            Show();
            turnText.text = ConvertIntToRomanNumber(BattleManager.Instance.GetCurrentTurn()) + "/" + ConvertIntToRomanNumber(BattleManager.Instance.GetMaxTurns());
            return;
        }

        if(BattleManager.Instance.GameEnded()) {
            Hide();
            return;
        }

        Hide();
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
