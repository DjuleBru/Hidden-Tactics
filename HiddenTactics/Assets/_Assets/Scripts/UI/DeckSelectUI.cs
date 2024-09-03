using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class DeckSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI readyButtonText;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    private bool ready;

    private void Awake() {
        mainMenuButton.onClick.AddListener(() => {
            HiddenTacticsLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.MultiplayerCleanupScene);
        });

        readyButton.onClick.AddListener(() => {
            ready = !ready;

            if(ready) {
                readyButtonText.text = "Unready";
            } else {
                readyButtonText.text = "Ready";
            }

            DeckSelectReady.Instance.SetPlayerReadyOrUnready(ready);
        });
    }

    private void Start() {
        Lobby lobby = HiddenTacticsLobby.Instance.GetLobby();

        lobbyNameText.text = "Lobby Name " + lobby.Name;
        lobbyCodeText.text = "Lobby Code : " + lobby.LobbyCode;
        DeckSelectReady.Instance.OnAllPlayersReady += DeckSelectReady_OnAllPlayersReady;
    }

    private void DeckSelectReady_OnAllPlayersReady(object sender, System.EventArgs e) {
        readyButton.interactable = false;
        readyButton.GetComponent<HoverButtonBehavior>().SetButtonEnabled(false);
    }
}
