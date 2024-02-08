using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] Image playerIconImage;
    [SerializeField] Image opponentIconImage;
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] TextMeshProUGUI opponentNameText;

    private void Start() {
        PlayerData localPlayerData = HiddenTacticsMultiplayer.Instance.GetPlayerData();


        //playerIconImage.sprite = HiddenTacticsMultiplayer.Instance.GetPlayerSprite(playerData.spriteId);
    }

}
