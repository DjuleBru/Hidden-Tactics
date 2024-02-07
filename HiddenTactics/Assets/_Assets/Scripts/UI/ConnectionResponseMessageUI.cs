using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionResponseMessageUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Awake() {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start() {
        HiddenTacticsMultiplayer.Instance.OnFailedToJoinGame += HiddenTacticsMultiplayer_OnFailedToJoinGame;

        Hide();
    }

    private void HiddenTacticsMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e) {
        Show();

        messageText.text = NetworkManager.Singleton.DisconnectReason;

        if(messageText.text == "") {
            messageText.text = "Connection timeout";
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        HiddenTacticsMultiplayer.Instance.OnFailedToJoinGame -= HiddenTacticsMultiplayer_OnFailedToJoinGame;
    }
}
