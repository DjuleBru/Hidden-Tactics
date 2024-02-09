using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : MonoBehaviour
{
    [SerializeField] private int playerIndex;

    [SerializeField] Image playerIconImage;

    [SerializeField] GameObject playerReadyGameObject;

    [SerializeField] TextMeshProUGUI playerNameText;

    private void Awake() {
        playerReadyGameObject.gameObject.SetActive(false);
    }

    private void Start() {
        PlayerData localPlayerData = HiddenTacticsMultiplayer.Instance.GetPlayerData();

        PlayerReadyManager.Instance.OnReadyChanged += PlayerReadyManager_OnReadyChanged;
    }

    private void PlayerReadyManager_OnReadyChanged(object sender, System.EventArgs e) {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);

        playerReadyGameObject.SetActive(PlayerReadyManager.Instance.IsPlayerReady(playerData.clientId));
    }
}
