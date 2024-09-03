using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class DeckSelectReady : NetworkBehaviour {
    
    public static DeckSelectReady Instance { get; private set; }

    public event EventHandler OnReadyChanged;
    public event EventHandler OnAllPlayersReady;

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake() {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReadyOrUnready(bool ready) {
        SetPlayerReadyServerRpc(ready);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(bool ready, ServerRpcParams serverRpcParams = default) {
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId, ready);

        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = ready;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) {
                // This player is NOT ready
                allClientsReady = false;
                break;
            }
        }

        if (playerReadyDictionary.Count < 2) {
            // There are not enough players to start the game
            allClientsReady = false;
        }

        if (allClientsReady) {
            HiddenTacticsLobby.Instance.DeleteLobby();
            StartCoroutine(LoadBattleScene());
            SetAllPlayerSReadyClientRpc();
        }
    }

    private IEnumerator LoadBattleScene() {
        yield return new WaitForSeconds(1f);
        SceneLoader.LoadNetwork(SceneLoader.Scene.BattleScene);
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId, bool ready) {
        playerReadyDictionary[clientId] = ready;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void SetAllPlayerSReadyClientRpc() {
        CrossfadeTransition.Instance.FadeIn();
        OnAllPlayersReady?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId) {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
