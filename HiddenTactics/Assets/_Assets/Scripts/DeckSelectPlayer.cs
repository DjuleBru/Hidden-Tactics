using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DeckSelectPlayer : MonoBehaviour
{

    [SerializeField] private int playerIndex;

    [SerializeField] private Button kickButton;
    [SerializeField] private SpriteRenderer readySpriteRenderer;
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite unreadySprite;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [SerializeField] private PlayerVisual playerVisual;

    private void Awake() {
        kickButton.onClick.AddListener(() => {
            PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            HiddenTacticsLobby.Instance.KickPlayer(playerData.playerId.ToString());
            HiddenTacticsMultiplayer.Instance.KickPlayer(playerData.clientId);
        });
    }

    private void Start() {
        HiddenTacticsMultiplayer.Instance.OnPlayerDataNetworkListChanged += HiddenTactics_OnPlayerDataNetworkListChanged;
        DeckSelectReady.Instance.OnReadyChanged += DeckSelectReady_OnReadyChanged;

        kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
        UpdatePlayer();
    }

    private void DeckSelectReady_OnReadyChanged(object sender, System.EventArgs e) {
        UpdatePlayer();
    }

    private void HiddenTactics_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e) {
        UpdatePlayer();
    }

    private void UpdatePlayer() {
        if(HiddenTacticsMultiplayer.Instance.IsPlayerIndexConnected(playerIndex)) {
            Show();

            PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);

            playerVisual.SetPlayerIcon(HiddenTacticsMultiplayer.Instance.GetPlayerSprite(playerIndex));
            playerNameText.text = playerData.playerName.ToString();

            if (DeckSelectReady.Instance.IsPlayerReady(playerData.clientId)) {
                readySpriteRenderer.sprite = readySprite;
            } else {
                readySpriteRenderer.sprite = unreadySprite;
            }
        } else {
            Hide();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        HiddenTacticsMultiplayer.Instance.OnPlayerDataNetworkListChanged -= HiddenTactics_OnPlayerDataNetworkListChanged;
    }
}
