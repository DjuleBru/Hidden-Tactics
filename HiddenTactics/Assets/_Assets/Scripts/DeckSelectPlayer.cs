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
    [SerializeField] private Image readyIcon;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [SerializeField] private Image playerIconImage;
    [SerializeField] private Image playerIconOutlineImage;
    [SerializeField] private Image playerIconOutlineShadowImage;
    [SerializeField] private Image playerIconBackgroundImage;
    [SerializeField] private Image playerFactionImage;
    [SerializeField] private Image playerFactionShadowImage;

    private void Awake() {
        if (playerIndex == 1) {
            kickButton.onClick.AddListener(() => {
                PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData();
                HiddenTacticsLobby.Instance.KickPlayer(playerData.playerId.ToString());
                HiddenTacticsMultiplayer.Instance.KickPlayer(playerData.clientId);
            });
        }
    }

    private void Start() {
        HiddenTacticsMultiplayer.Instance.OnPlayerCustomizationDataNetworkListChanged += HiddenTactics_OnPlayerCustomizationDataNetworkListChanged;
        HiddenTacticsMultiplayer.Instance.OnPlayerDataNetworkListChanged += HiddenTactics_OnPlayerDataNetworkListChanged;
        DeckSelectReady.Instance.OnReadyChanged += DeckSelectReady_OnReadyChanged;

        if(playerIndex == 1) {
            //Opponent kick only if is server
            kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
        }

        UpdatePlayer();
    }

    private void DeckSelectReady_OnReadyChanged(object sender, System.EventArgs e) {
        UpdatePlayer();
    }

    private void HiddenTactics_OnPlayerCustomizationDataNetworkListChanged(object sender, System.EventArgs e) {
        UpdatePlayer();
    }

    private void HiddenTactics_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e) {
        UpdatePlayer();
    }

    private void UpdatePlayer() {
        if(HiddenTacticsMultiplayer.Instance.IsPlayerIndexConnected(playerIndex)) {
            Show();

            PlayerCustomizationData playerCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalPlayerCustomizationData();

            if (playerIndex == 1) {
                playerCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData();
            }

            playerIconImage.sprite = PlayerCustomizationDataManager.Instance.GetPlayerIconSpriteFromSpriteId(playerCustomizationData.iconSpriteId);
            FactionSO faction = PlayerCustomizationDataManager.Instance.GetFactionSOFromId(playerCustomizationData.factionID);
            playerFactionImage.sprite = faction.factionSprite;
            playerFactionShadowImage.sprite = faction.factionSprite;
            playerIconOutlineImage.sprite = faction.slotBorder;
            playerIconOutlineShadowImage.sprite = faction.slotBorder;
            playerIconBackgroundImage.sprite = faction.slotBackgroundSquare;

            playerNameText.text = playerCustomizationData.playerName.ToString();

            if (DeckSelectReady.Instance.IsPlayerReady(playerCustomizationData.clientId)) {
                readyIcon.gameObject.SetActive(true);
            } else {
                readyIcon.gameObject.SetActive(false);
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
        HiddenTacticsMultiplayer.Instance.OnPlayerDataNetworkListChanged -= HiddenTactics_OnPlayerCustomizationDataNetworkListChanged;
        HiddenTacticsMultiplayer.Instance.OnPlayerDataNetworkListChanged -= HiddenTactics_OnPlayerDataNetworkListChanged;
    }
}
