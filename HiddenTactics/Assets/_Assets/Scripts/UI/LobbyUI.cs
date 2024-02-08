using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button joinPrivateLobbyButton;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private LobbyCreateUI lobbyCreateUI;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Start() {
        playerNameInputField.text = HiddenTacticsMultiplayer.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string newText) => {
            HiddenTacticsMultiplayer.Instance.SetPlayerName(newText);
        });

        HiddenTacticsLobby.Instance.OnLobbyListChanged += HiddenTacticsLobby_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void HiddenTacticsLobby_OnLobbyListChanged(object sender, HiddenTacticsLobby.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    private void Awake() {

        lobbyTemplate.gameObject.SetActive(false);

        createLobbyButton.onClick.AddListener(() => {
            lobbyCreateUI.Show();
        });
        quickJoinButton.onClick.AddListener(() => {
            HiddenTacticsLobby.Instance.QuickJoin();
        });
        mainMenuButton.onClick.AddListener(() => {
            HiddenTacticsLobby.Instance.LeaveLobby();
            SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
        });
        joinPrivateLobbyButton.onClick.AddListener(() => {
            HiddenTacticsLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });
    }

    private void UpdateLobbyList(List<Lobby> lobbyList) {
        foreach(Transform child in lobbyContainer) {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in lobbyList) {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void OnDestroy() {
        HiddenTacticsLobby.Instance.OnLobbyListChanged -= HiddenTacticsLobby_OnLobbyListChanged;
    }
}
