using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using QFSW;
using QFSW.QC;

public class BattleManager : NetworkBehaviour
{
    public static BattleManager Instance;

    [SerializeField] private BattlefieldAnimatorManager battleFieldAnimatorControllingPhases;
    public event EventHandler OnStateChanged;
    public event EventHandler OnAllPlayersLoaded;

    [SerializeField] private float preparationPhaseMaxTime;
    [SerializeField] private float battlePhaseMaxTime;
    [SerializeField] private float battlePhaseActivationDelay;

    [SerializeField] private Transform playerPrefab;

    private NetworkVariable<float> battlePhaseTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> preparationPhaseTimer = new NetworkVariable<float>(0f);

    private enum State {
        WaitingToStart,
        PreparationPhase,
        BattlePhaseStarting,
        BattlePhase,
        BattlePhaseEnding,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);

    private void Awake() {
        Instance = this;
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;
        PlayerReadyManager.Instance.OnAllPlayersReady += PlayersReadyManager_OnAllPlayersReady;

        battleFieldAnimatorControllingPhases.OnBattlefieldsSlammed += BattleFieldAnimatorControllingPhases_OnBattlefieldsSlammed;
        battleFieldAnimatorControllingPhases.OnBattlefieldsSplit += BattleFieldAnimatorControllingPhases_OnBattlefieldsSplit;

        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }

        battlePhaseTimer.Value = battlePhaseMaxTime;
        preparationPhaseTimer.Value = preparationPhaseMaxTime;
    }

    private void Update() {

        if (!IsServer) {
            return;
        }

        switch (state.Value) {

            case State.WaitingToStart:
            break;

            case State.BattlePhaseStarting:
            break;

            case State.PreparationPhase:
                battlePhaseTimer.Value = battlePhaseMaxTime;
                if (!HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
                    return;
                }

                preparationPhaseTimer.Value -= Time.deltaTime;
                if(preparationPhaseTimer.Value < 0 ) {
                    state.Value = State.BattlePhaseStarting;
                }
            break;

            case State.BattlePhase:
                preparationPhaseTimer.Value = preparationPhaseMaxTime;
                battlePhaseTimer.Value -= Time.deltaTime;

                if (battlePhaseTimer.Value < 0) {
                    state.Value = State.BattlePhaseEnding;
                }
                break;

            case State.BattlePhaseEnding:
            break;
        }
    }

    private void PlayersReadyManager_OnAllPlayersReady(object sender, EventArgs e) {
        state.Value = State.BattlePhaseStarting;
    }

    [ServerRpc(RequireOwnership =false)]
    private void SetPlayersLoadedServerRpc() {
        state.Value = State.PreparationPhase;
        SetPlayersLoadedClientRpc();
    }

    [ClientRpc]
    private void SetPlayersLoadedClientRpc() {
        OnAllPlayersLoaded?.Invoke(this, EventArgs.Empty);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
        // All players have loaded the scene
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
        SetPlayersLoadedServerRpc();
    }


    private void BattleFieldAnimatorControllingPhases_OnBattlefieldsSlammed(object sender, EventArgs e) {
        if (!IsServer) {
            return;
        }
        Invoke("SetBattlePhase", battlePhaseActivationDelay);
    }

    private void BattleFieldAnimatorControllingPhases_OnBattlefieldsSplit(object sender, EventArgs e) {
        if (!IsServer) {
            return;
        }
        state.Value = State.PreparationPhase;
    }

    [Command]
    public void SetBattlePhase() {
        state.Value = State.BattlePhase;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    [Command]
    public void SetPreparationPhase() {
        state.Value = State.PreparationPhase;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void State_OnValueChanged(State previousValue, State newValue) {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetBattlePhaseTimerNormalized() {
        return battlePhaseTimer.Value / battlePhaseMaxTime;
    }

    public float GetPreparationPhaseTimerNormalized() {
        return preparationPhaseTimer.Value / preparationPhaseMaxTime;
    }

    public bool IsBattlePhase() {
        return state.Value == State.BattlePhase;
    }

    public bool IsPreparationPhase() {
        return state.Value == State.PreparationPhase;
    }

    public bool IsBattlePhaseStarting() {
        return state.Value == State.BattlePhaseStarting;
    }

    public bool IsBattlePhaseEnding() {
        return state.Value == State.BattlePhaseEnding;
    }

    public bool IsWaitingToStart() {
        return state.Value == State.WaitingToStart;
    }

}