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

        //ShowPlayerDataDebugInfo();
        //ShowOpponentDataDebugInfo();
    }

    private void HiddenTacticsMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e) {
        playerIconSpriteId = HiddenTacticsMultiplayer.Instance.GetLocalPlayerCustomizationData().iconSpriteId;
        opponentIconSpriteId = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData().iconSpriteId;

        playerGridVisualSOId = HiddenTacticsMultiplayer.Instance.GetLocalPlayerCustomizationData().gridVisualSOId;
        opponentGridVisualSOId = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData().gridVisualSOId;
    }

    [Button]
    private void SlowMo() {
        Time.timeScale = .25f;
    }

    [Button]
    private void Play() {
        Time.timeScale = 1f;
    }

    [Button]
    private void FastMo() {
        Time.timeScale = 2f;
    }

    [Button]
    private void ShowPlayerCustomizationDataDebugInfo() {
        PlayerCustomizationData playerData = HiddenTacticsMultiplayer.Instance.GetLocalPlayerCustomizationData();

        Debug.Log("player icon sprite id " + playerData.iconSpriteId);
        Debug.Log("player grid sprite id " + playerData.gridVisualSOId);
        Debug.Log("player battlefield base id " + playerData.battlefieldBaseSpriteId);
        Debug.Log("player village sprite count " + playerData.villageSpriteNumber);
        Debug.Log("player village sprite id 0 " + playerData.villageSprite0Id);
        Debug.Log("player village sprite id 1 " + playerData.villageSprite1Id);
        Debug.Log("player village sprite id 2 " + playerData.villageSprite2Id);
        Debug.Log("player village sprite id 3 " + playerData.villageSprite3Id);
        Debug.Log("player village sprite id 4 " + playerData.villageSprite4Id);
        Debug.Log("player village sprite id 5 " + playerData.villageSprite5Id);
    }

    [Button]
    private void ShowOpponentCustomizationDataDebugInfo() {
        PlayerCustomizationData opponentData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData();

        Debug.Log("opponent icon sprite id " + opponentData.iconSpriteId);
        Debug.Log("opponent grid sprite id " + opponentData.gridVisualSOId);
        Debug.Log("opponent battlefield base id " + opponentData.battlefieldBaseSpriteId);
        Debug.Log("opponent village sprite count " + opponentData.villageSpriteNumber);
        Debug.Log("opponent village sprite id 0 " + opponentData.villageSprite0Id);
        Debug.Log("opponent village sprite id 1 " + opponentData.villageSprite1Id);
        Debug.Log("opponent village sprite id 2 " + opponentData.villageSprite2Id);
        Debug.Log("opponent village sprite id 3 " + opponentData.villageSprite3Id);
        Debug.Log("opponent village sprite id 4 " + opponentData.villageSprite4Id);
        Debug.Log("opponent village sprite id 5 " + opponentData.villageSprite5Id);
    }


    [Button]
    private void ShowPlayerDataDebugInfo() {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetLocalPlayerData();
    }

    [Button]
    private void ShowOpponentDataDebugInfo() {
        PlayerData opponentData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData();
    }

}
