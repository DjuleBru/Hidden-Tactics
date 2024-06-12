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
    public event EventHandler OnAllPlayersWantToReplay;

    public event EventHandler OnPlayerWantsToSpeedUpChanged;
    public event EventHandler OnPlayerWantsToReplayChanged;

    private Dictionary<ulong, bool> playerWantsToSpeedUpDictionary;
    private Dictionary<ulong, bool> playerWantsToReplayDictionary;

    private void Awake() {

        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>() { };
        playerWantsToSpeedUpDictionary = new Dictionary<ulong, bool>() { };
        playerWantsToReplayDictionary = new Dictionary<ulong, bool>() { };
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

    #region  REPLAY MANAGEMENT

    public void TogglePlayerWantsToReplay(bool wantsToReplay) {
        if (HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
            TogglePlayerWantsToReplayServerRpc(wantsToReplay);
        }
        else {
            if (wantsToReplay) {
                OnAllPlayersWantToReplay?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePlayerWantsToReplayServerRpc(bool wantsToReplay, ServerRpcParams serverRpcParams = default) {
        TogglePlayerWantsToReplayClientRpc(serverRpcParams.Receive.SenderClientId, wantsToReplay);

        if (!IsServer) return;
        playerWantsToReplayDictionary[serverRpcParams.Receive.SenderClientId] = wantsToReplay;

        bool allClientsWantToReplay = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerWantsToReplayDictionary.ContainsKey(clientId) || !playerWantsToReplayDictionary[clientId]) {
                // Player is NOT ready
                allClientsWantToReplay = false;
            }
        }

        if (allClientsWantToReplay) {
            //Reset ready state
            InvokeAllPlayerWantsToReplayClientRpc();
        }
    }

    [ClientRpc]
    private void TogglePlayerWantsToReplayClientRpc(ulong clientId, bool playerWantsToReplay) {
        playerWantsToReplayDictionary[clientId] = playerWantsToReplay;
        OnPlayerWantsToReplayChanged?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void InvokeAllPlayerWantsToReplayClientRpc() {
        OnAllPlayersWantToReplay?.Invoke(this, EventArgs.Empty);
    }

    public bool PlayerWantingToReplay(ulong clientId) {
        return playerWantsToReplayDictionary.ContainsKey(clientId) && playerWantsToReplayDictionary[clientId];
    }
    #endregion

}
