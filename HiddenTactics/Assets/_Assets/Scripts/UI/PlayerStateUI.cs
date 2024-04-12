using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : MonoBehaviour
{
    [SerializeField] private bool isOpponentPanel;

    [SerializeField] Image playerIconImage;
    [SerializeField] GameObject playerReadyGameObject;
    [SerializeField] TextMeshProUGUI playerNameText;

    private void Awake() {
        playerReadyGameObject.gameObject.SetActive(false);
    }

    private void Start() {
        //PlayerData localPlayerData = HiddenTacticsMultiplayer.Instance.GetPlayerData();

        PlayerReadyManager.Instance.OnReadyChanged += PlayerReadyManager_OnReadyChanged;
        PlayerReadyManager.Instance.OnPlayerWantsToSpeedUpChanged += PlayerReadyManager_OnPlayerWantsToSpeedUpChanged;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseStarting() | BattleManager.Instance.IsPreparationPhase()) {
            playerReadyGameObject.SetActive(false);
        } 
    }

    private void PlayerReadyManager_OnReadyChanged(object sender, System.EventArgs e) {

        PlayerData playerData = new PlayerData();

        if (!isOpponentPanel) {
            playerData = HiddenTacticsMultiplayer.Instance.GetPlayerData();
        } else {
            playerData = HiddenTacticsMultiplayer.Instance.GetOpponentData();
        }

        playerReadyGameObject.SetActive(PlayerReadyManager.Instance.IsPlayerReady(playerData.clientId));
    }

    private void PlayerReadyManager_OnPlayerWantsToSpeedUpChanged(object sender, System.EventArgs e) {

        PlayerData playerData = new PlayerData();

        if (!isOpponentPanel) {
            playerData = HiddenTacticsMultiplayer.Instance.GetPlayerData();
        }
        else {
            playerData = HiddenTacticsMultiplayer.Instance.GetOpponentData();
        }

        playerReadyGameObject.SetActive(PlayerReadyManager.Instance.PlayerWantingToSpeedUp(playerData.clientId));
    }

}
