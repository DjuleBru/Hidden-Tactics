using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePhaseUI : MonoBehaviour
{
    [SerializeField] Image battlePhaseTimerImage;
    [SerializeField] Button playerSpeedUpButton;
    [SerializeField] GameObject playerSpeedUpGameObject;

    [SerializeField] GameObject playerSpeedUpIndicator;
    [SerializeField] GameObject opponentSpeedUpIndicator;

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
}
