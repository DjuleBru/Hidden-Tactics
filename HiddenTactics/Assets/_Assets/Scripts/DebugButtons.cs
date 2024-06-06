using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugButtons : MonoBehaviour
{
    int playerIconSpriteId;
    int opponentIconSpriteId;
    int playerGridVisualSOId;
    int opponentGridVisualSOId;

    private void Start() {
        HiddenTacticsMultiplayer.Instance.OnPlayerDataNetworkListChanged += HiddenTacticsMultiplayer_OnPlayerDataNetworkListChanged;

        ShowPlayerDataDebugInfo();
        ShowOpponentDataDebugInfo();
    }

    private void HiddenTacticsMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e) {
        playerIconSpriteId = HiddenTacticsMultiplayer.Instance.GetLocalPlayerData().iconSpriteId;
        opponentIconSpriteId = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData().iconSpriteId;

        playerGridVisualSOId = HiddenTacticsMultiplayer.Instance.GetLocalPlayerData().gridVisualSOId;
        opponentGridVisualSOId = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData().gridVisualSOId;
    }

    [Button]
    private void ShowPlayerDataDebugInfo() {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetLocalPlayerData();

        Debug.Log("player id " + playerData.playerId);
        Debug.Log("player gold " + playerData.playerGold);
        Debug.Log("player icon sprite id " + playerData.iconSpriteId);
        Debug.Log("player grid sprite id " + playerData.gridVisualSOId);
        Debug.Log("player battlefield base id " + playerData.battlefieldBaseSpriteId);
    }

    [Button]
    private void ShowOpponentDataDebugInfo() {
        PlayerData opponentData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData();

        Debug.Log("opponent id " + opponentData.playerId);
        Debug.Log("opponent gold " + opponentData.playerGold);
        Debug.Log("opponent icon sprite id " + opponentData.iconSpriteId);
        Debug.Log("opponent grid sprite id " + opponentData.gridVisualSOId);
        Debug.Log("opponent battlefield base id " + opponentData.battlefieldBaseSpriteId);
    }

    [Button]
    private void ShowPlayer0DataDebugInfo() {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromPlayerIndex(0);

        Debug.Log("player 1 id " + playerData.playerId);
        Debug.Log("player 1 gold " + playerData.playerGold);
        Debug.Log("player 1 icon sprite id " + playerData.iconSpriteId);
        Debug.Log("player 1 grid sprite id " + playerData.gridVisualSOId);
        Debug.Log("player 1 battlefield base id " + playerData.battlefieldBaseSpriteId);
    }

    [Button]
    private void ShowPlayer1DataDebugInfo() {
        PlayerData opponentData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromPlayerIndex(1);

        Debug.Log("player 2 id " + opponentData.playerId);
        Debug.Log("player 2 gold " + opponentData.playerGold);
        Debug.Log("player 2 icon sprite id " + opponentData.iconSpriteId);
        Debug.Log("player 2 grid sprite id " + opponentData.gridVisualSOId);
        Debug.Log("player 2 battlefield base id " + opponentData.battlefieldBaseSpriteId);
    }
}
