using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePhaseUI : MonoBehaviour
{
    [SerializeField] Image battlePhaseTimerImage;
    [SerializeField] Button playerSpeedUpButton;
    [SerializeField] GameObject playerSpeedUpGameObject;

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

        Hide();
    }


    private void Update() {
        battlePhaseTimerImage.fillAmount = BattleManager.Instance.GetBattlePhaseTimerNormalized();
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {

        if (BattleManager.Instance.IsBattlePhase()) {
            Show();
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

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
