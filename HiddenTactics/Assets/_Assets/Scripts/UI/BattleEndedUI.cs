using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BattleEndedUI : NetworkBehaviour
{
    [SerializeField] private GameObject battleEndedUIPanel;
    [SerializeField] private TextMeshProUGUI battleEndedText;

    private void Awake() {
        battleEndedUIPanel.gameObject.SetActive(false);
    }

    private void Start() {
        BattleManager.Instance.OnGameEnded += BattleManager_OnGameEnded;
    }

    private void BattleManager_OnGameEnded(object sender, System.EventArgs e) {
        battleEndedUIPanel.gameObject.SetActive(true);

        //Check if player won
        if(HiddenTacticsMultiplayer.Instance.PlayerWon(NetworkManager.Singleton.LocalClientId)) {
            battleEndedText.text = "You Won !";
        } else {
            if(HiddenTacticsMultiplayer.Instance.PlayerTie(NetworkManager.Singleton.LocalClientId)) {
                battleEndedText.text = "Tie !";
            } else {
                battleEndedText.text = "You LOOSE";
            }
        }
    }

    public void LoadMainMenu() {
        SceneLoader.Load(SceneLoader.Scene.LobbyScene);
    }

    public void Replay() {
        Debug.Log("Player set ready to replay");
    }
}
