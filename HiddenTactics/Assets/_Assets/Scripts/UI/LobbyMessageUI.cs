using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Awake() {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start() {
        HiddenTacticsMultiplayer.Instance.OnFailedToJoinGame += HiddenTacticsMultiplayer_OnFailedToJoinGame;
        HiddenTacticsLobby.Instance.OnCreateLobbyStarted += HiddenTacticsLobby_OnCreateLobbyStarted;
        HiddenTacticsLobby.Instance.OnCreateLobbyFailed += HiddenTactics_OnCreateLobbyFailed;
        HiddenTacticsLobby.Instance.OnJoinStarted += HiddenTacticsLobby_OnJoinStarted;
        HiddenTacticsLobby.Instance.OnQuickJoinFailed += HiddenTacticsLobby_OnQuickJoinFailed;
        HiddenTacticsLobby.Instance.OnJoinFailed += HiddenTacticsLobby_OnJoinFailed;

        Hide();
    }

    private void HiddenTacticsLobby_OnJoinFailed(object sender, System.EventArgs e) {
        ShowMessage("Failed to join lobby");
    }

    private void HiddenTacticsLobby_OnQuickJoinFailed(object sender, System.EventArgs e) {
        ShowMessage("Could not find a lobby to quick join");
    }

    private void HiddenTacticsLobby_OnJoinStarted(object sender, System.EventArgs e) {
        ShowMessage("Joining lobby ...");
    }

    private void HiddenTacticsMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e) {
        messageText.text = NetworkManager.Singleton.DisconnectReason;

        if (messageText.text == "") {
            ShowMessage("Failed to connect");
        }
    }

    private void HiddenTactics_OnCreateLobbyFailed(object sender, System.EventArgs e) {
        ShowMessage("Failed to create lobby");
    }

    private void HiddenTacticsLobby_OnCreateLobbyStarted(object sender, System.EventArgs e) {
        ShowMessage("Creating lobby ...");
    }

    private void ShowMessage(string message) {
        Show();
        messageText.text = message;
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        HiddenTacticsMultiplayer.Instance.OnFailedToJoinGame -= HiddenTacticsMultiplayer_OnFailedToJoinGame;
        HiddenTacticsLobby.Instance.OnCreateLobbyStarted -= HiddenTacticsLobby_OnCreateLobbyStarted;
        HiddenTacticsLobby.Instance.OnCreateLobbyFailed -= HiddenTactics_OnCreateLobbyFailed;
        HiddenTacticsLobby.Instance.OnJoinStarted -= HiddenTacticsLobby_OnJoinStarted;
        HiddenTacticsLobby.Instance.OnQuickJoinFailed -= HiddenTacticsLobby_OnQuickJoinFailed;
        HiddenTacticsLobby.Instance.OnJoinFailed -= HiddenTacticsLobby_OnJoinFailed;
    }
}
