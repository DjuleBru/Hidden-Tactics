using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class HiddenTacticsBattleManager : NetworkBehaviour
{
    public static HiddenTacticsBattleManager Instance;

    public event EventHandler OnStateChanged;


    private enum State {
        WaitingToStart,
        PreparationPhase,
        BattlePhase,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private Transform playerSpawnTransform;


    private void Awake() {
        Instance = this;
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;

        if(IsServer) {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
           Transform playerTransform = Instantiate(playerPrefab, playerSpawnTransform.position, Quaternion.identity);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue) {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

}
