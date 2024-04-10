using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{

    public static Player LocalInstance;

    private bool isReady;

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

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if (BattleManager.Instance.IsPreparationPhase()) {
            isReady = false;

            PlayerReadyManager.Instance.SetPlayerReadyOrUnready(isReady);
        }
    }
}
