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

    public event EventHandler OnAllPlayersWantToSpeedUp;
    public event EventHandler OnPlayerWantsToSpeedUpChanged;

    private Dictionary<ulong, bool> playerWantsToSpeedUpDictionary;

    private void Awake() {

        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>() { };
        playerWantsToSpeedUpDictionary = new Dictionary<ulong, bool>() { };
    }

    #region READY MANAGEMENT
    public void SetPlayerReadyOrUnready(bool ready) {
        if (HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
            SetPlayerReadyServerRpc(ready);
        } else {
            if(ready) {
                OnAllPlayersReady?.Invoke(this, EventArgs.Empty);
            }
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

    #endregion

    #region SPEEDUP MANAGEMENT

    public void TogglePlayerWantsToSpeedUp(bool wantsToSpeedUp) {
        if (HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
            TogglePlayerWantsToSpeedUpServerRpc(wantsToSpeedUp);
        }
        else {
            if (wantsToSpeedUp) {
                OnAllPlayersWantToSpeedUp?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePlayerWantsToSpeedUpServerRpc(bool wantsToSpeedUp, ServerRpcParams serverRpcParams = default) {
        TogglePlayerWantsToSpeedUpClientRpc(serverRpcParams.Receive.SenderClientId, wantsToSpeedUp);

        if (!IsServer) return;
        playerWantsToSpeedUpDictionary[serverRpcParams.Receive.SenderClientId] = wantsToSpeedUp;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerWantsToSpeedUpDictionary.ContainsKey(clientId) || !playerWantsToSpeedUpDictionary[clientId]) {
                // Player is NOT ready
                allClientsReady = false;
            }
        }

        if (allClientsReady) {
            //Reset ready state
            InvokeAllPlayerWantToSpeedUpClientRpc(); 
        }
    }

    [ClientRpc]
    private void TogglePlayerWantsToSpeedUpClientRpc(ulong clientId, bool playerIsReady) {
        playerWantsToSpeedUpDictionary[clientId] = playerIsReady;
        OnPlayerWantsToSpeedUpChanged?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void InvokeAllPlayerWantToSpeedUpClientRpc() {
        OnAllPlayersWantToSpeedUp?.Invoke(this, EventArgs.Empty);
    }

    public bool PlayerWantingToSpeedUp(ulong clientId) {
        return playerWantsToSpeedUpDictionary.ContainsKey(clientId) && playerWantsToSpeedUpDictionary[clientId];
    }

    #endregion

}
