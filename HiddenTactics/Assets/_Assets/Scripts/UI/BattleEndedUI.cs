using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BattleEndedUI : MonoBehaviour
{
    [SerializeField] private GameObject battleEndedUIPanel;

    [SerializeField] private GameObject playerWantsToReplayGameObject;
    [SerializeField] private GameObject opponentWantsToReplayGameObject;

    [SerializeField] private TextMeshProUGUI battleEndedText;

    private void Awake() {
        battleEndedUIPanel.gameObject.SetActive(false);
    }

    private void Start() {
        BattleManager.Instance.OnGameEnded += BattleManager_OnGameEnded;
        HiddenTacticsMultiplayer.Instance.OnPlayerSurrendered += HiddenTacticsMultiplayer_OnPlayerSurrendered;
        HiddenTacticsMultiplayer.Instance.OnOpponentSurrendered += HiddenTacticsMultiplayer_OnOpponentSurrendered;
        PlayerReadyManager.Instance.OnPlayerWantsToReplayChanged += PlayerReadyManager_OnPlayerWantsToReplayChanged;
    }

    private void PlayerReadyManager_OnPlayerWantsToReplayChanged(object sender, System.EventArgs e) {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetLocalPlayerData();
        PlayerData opponentData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData();

        playerWantsToReplayGameObject.SetActive(PlayerReadyManager.Instance.PlayerWantingToReplay(playerData.clientId));
        opponentWantsToReplayGameObject.SetActive(PlayerReadyManager.Instance.PlayerWantingToReplay(opponentData.clientId));
    }

    private void HiddenTacticsMultiplayer_OnOpponentSurrendered(object sender, System.EventArgs e) {
        battleEndedUIPanel.gameObject.SetActive(true);
        battleEndedText.text = "You Won (opponent surrendered)";
    }

    private void HiddenTacticsMultiplayer_OnPlayerSurrendered(object sender, System.EventArgs e) {
        battleEndedUIPanel.gameObject.SetActive(true);
        battleEndedText.text = "You loose (you surrendered)";
    }

    private void BattleManager_OnGameEnded(object sender, System.EventArgs e) {
        battleEndedUIPanel.gameObject.SetActive(true);

        //Check if player won
        if(Player.LocalInstance.CheckIfPlayerWon()) {
            battleEndedText.text = "You Won !";
        } else {
            if(Player.LocalInstance.CheckIfPlayerTie()) {
                battleEndedText.text = "Tie !";
            } else {
                battleEndedText.text = "You LOOSE";
            }
        }
    }

    public void LoadMainMenu() {
        SceneLoader.Load(SceneLoader.Scene.MultiplayerCleanupScene);
    }

    public void SetPlayerWantsToReplay() {
        Player.LocalInstance.SetPlayerWantsReplay();
    }
}
