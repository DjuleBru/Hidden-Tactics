using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour {

    [SerializeField] private Button closeButton;
    [SerializeField] private Button createPublicButton;
    [SerializeField] private Button createPrivateButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;

    private void Awake() {
        createPublicButton.onClick.AddListener(() => {
            CreateLobbyWithName(false);
        });
        createPrivateButton.onClick.AddListener(() => {
            CreateLobbyWithName(true);
        });
        closeButton.onClick.AddListener(Hide);
    }

    private void CreateLobbyWithName(bool isPrivate) {
        string lobbyName = lobbyNameInputField.text;
        if (lobbyNameInputField.text == "") {
            lobbyName = "Lobby#" + Random.Range(0, 10000);
        }
        HiddenTacticsLobby.Instance.CreateLobby(lobbyName, isPrivate);
    }

    private void Start() {
        Hide();
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}
