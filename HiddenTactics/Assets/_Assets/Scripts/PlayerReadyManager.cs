using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerReadyManager : NetworkBehaviour
{
    public static PlayerReadyManager Instance { get; private set; }

    public event EventHandler OnAllPlayersReady;
    public event EventHandler OnReadyChanged;

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake() {

        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>() { };
    }

    public void SetPlayerReady(bool ready) {
        Debug.Log(ready);
        if(HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
            SetPlayerReadyServerRpc(ready);
        } else {
            OnAllPlayersReady?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(bool ready, ServerRpcParams serverRpcParams = default) {

        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId, ready);
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = ready;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) {
                // Player is NOT ready
                allClientsReady = false;
            }
        }
        
        if (allClientsReady) {
            //Reset ready state

            OnAllPlayersReady?.Invoke(this, EventArgs.Empty);
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
                SetPlayerReadyClientRpc(clientId, false);
            }
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId, bool playerIsReady) {
        playerReadyDictionary[clientId] = playerIsReady;
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId) {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
