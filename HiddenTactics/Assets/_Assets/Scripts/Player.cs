using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{

    public static Player LocalInstance;

    private Deck playerDeck;
    private bool isReady;
    private bool wantsToSpeedUp;
    private bool wantsToReplay;

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
        }
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    public void SetPlayerReadyOrUnready() {
        isReady = !isReady;
        PlayerReadyManager.Instance.SetPlayerReadyOrUnready(isReady);
    }

    public bool GetPlayerReady() { return isReady; }

    public bool GetPlayerWantsToSpeedUp() { return wantsToSpeedUp; }

    public void SetPlayerWantsToSpeedUp() {
        wantsToSpeedUp = !wantsToSpeedUp;
        PlayerReadyManager.Instance.TogglePlayerWantsToSpeedUp(wantsToSpeedUp);
    }

    public void SetPlayerWantsReplay() {
        wantsToReplay = !wantsToReplay;
        PlayerReadyManager.Instance.TogglePlayerWantsToReplay(wantsToReplay);
    }

    public void SetPlayerSurrenders() {
        HiddenTacticsMultiplayer.Instance.SetPlayerSurrender(NetworkManager.Singleton.LocalClientId);
    }
    
    public bool CheckIfPlayerWon() {
        Debug.Log("check if player won");
        return HiddenTacticsMultiplayer.Instance.PlayerWon(NetworkManager.Singleton.LocalClientId);
    }

    public bool CheckIfPlayerTie() {
        return HiddenTacticsMultiplayer.Instance.PlayerTie(NetworkManager.Singleton.LocalClientId);
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if (BattleManager.Instance.IsPreparationPhase()) {
            isReady = false;
            wantsToSpeedUp = false;

            PlayerReadyManager.Instance.SetPlayerReadyOrUnready(isReady);
            PlayerReadyManager.Instance.TogglePlayerWantsToSpeedUp(wantsToSpeedUp);
        }
    }

}
