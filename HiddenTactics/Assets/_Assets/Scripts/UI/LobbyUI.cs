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
    [SerializeField] private Button customizeButton;
    [SerializeField] private TMP_InputField joinCodeInputField;

    [SerializeField] private TextMeshProUGUI playerNameText;

    [SerializeField] private SpriteRenderer playerIconSpriteRenderer;

    [SerializeField] private LobbyCreateUI lobbyCreateUI;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Start() {
        playerNameText.text = HiddenTacticsMultiplayer.Instance.GetPlayerName();
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
            SceneLoader.Load(SceneLoader.Scene.MultiplayerCleanupScene);
        });
        joinPrivateLobbyButton.onClick.AddListener(() => {
            HiddenTacticsLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });
        customizeButton.onClick.AddListener(() => {
            CustomizationUI.Instance.Show();
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
