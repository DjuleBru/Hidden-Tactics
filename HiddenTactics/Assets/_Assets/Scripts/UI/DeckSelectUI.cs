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
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    private void Awake() {
        mainMenuButton.onClick.AddListener(() => {
            HiddenTacticsLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
        });
        readyButton.onClick.AddListener(() => {
            DeckSelectReady.Instance.SetPlayerReady();
        });
    }

    private void Start() {
        Lobby lobby = HiddenTacticsLobby.Instance.GetLobby();

        lobbyNameText.text = "Lobby Name " + lobby.Name;
        lobbyCodeText.text = "Lobby Code : " + lobby.LobbyCode;
    }


}
