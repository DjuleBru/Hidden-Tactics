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

    public override void OnNetworkSpawn() {
        if (IsOwner) {
            LocalInstance = this;
            VillageManager.Instance.GeneratePlayerVillagesServerRpc(NetworkManager.Singleton.LocalClientId);
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

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if (BattleManager.Instance.IsPreparationPhase()) {
            isReady = false;
            wantsToSpeedUp = false;

            PlayerReadyManager.Instance.SetPlayerReadyOrUnready(isReady);
            PlayerReadyManager.Instance.TogglePlayerWantsToSpeedUp(wantsToSpeedUp);
        }
    }

}
