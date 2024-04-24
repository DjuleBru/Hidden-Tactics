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
    public bool allPlayersLoaded;
    public bool isFirstPreparationPhase = true;

    public event EventHandler OnSpeedUpButtonActivation;
    private bool speedUpButtonActive;

    [SerializeField] private float preparationPhaseMaxTime;
    [SerializeField] private float battlePhaseMaxTime;
    [SerializeField] private float battlePhaseActivationDelay;
    [SerializeField] private float battlePhaseSpeedUpButtonActivationDelay;

    [SerializeField] private Transform playerPrefab;

    private NetworkVariable<float> battlePhaseTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> preparationPhaseTimer = new NetworkVariable<float>(0f);

    private List<Unit> unitsOnBattlefieldList = new List<Unit>();
    private List<Unit> unitsStillInBattle = new List<Unit>();

    private enum State {
        WaitingToStart,
        PreparationPhase,
        BattlePhaseStarting,
        BattlePhase,
        BattlePhaseEnding,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>();

    private void Awake() {
        Instance = this;
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;
        PlayerReadyManager.Instance.OnAllPlayersReady += PlayersReadyManager_OnAllPlayersReady;
        PlayerReadyManager.Instance.OnAllPlayersWantToSpeedUp += PlayersReadyManager_OnAllPlayersWantToSpeedUp;

        battleFieldAnimatorControllingPhases.OnBattlefieldsSlammed += BattleFieldAnimatorControllingPhases_OnBattlefieldsSlammed;
        battleFieldAnimatorControllingPhases.OnBattlefieldsSplit += BattleFieldAnimatorControllingPhases_OnBattlefieldsSplit;

        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            state.Value = State.WaitingToStart;
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
                if (!HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
                    state.Value = State.PreparationPhase;
                }
                break;

            case State.BattlePhaseStarting:
            break;

            case State.PreparationPhase:
                isFirstPreparationPhase = false;
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
                // Timer
                battlePhaseTimer.Value -= Time.deltaTime;

                // Battle phase timer 
                if(!speedUpButtonActive && battlePhaseTimer.Value < (battlePhaseMaxTime - battlePhaseSpeedUpButtonActivationDelay)) {
                    SetSpeedUpButtonActiveServerRpc();
                }

                if (battlePhaseTimer.Value < 0) {
                    preparationPhaseTimer.Value = preparationPhaseMaxTime;
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

    private void PlayersReadyManager_OnAllPlayersWantToSpeedUp(object sender, EventArgs e) {
        Time.timeScale = 1.75f;
    }

    [ServerRpc(RequireOwnership =false)]
    private void SetPlayersLoadedServerRpc() {
        state.Value = State.PreparationPhase;
        SetPlayersLoadedClientRpc();
    }

    [ClientRpc]
    private void SetPlayersLoadedClientRpc() {
        allPlayersLoaded = true;
        OnAllPlayersLoaded?.Invoke(this, EventArgs.Empty);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
        // All players have loaded the scene
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            HiddenTacticsMultiplayer.Instance.SetPlayerInitialConditionsServerRpc(clientId);
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
        Time.timeScale = 1f;

        if (IsServer) {
            if(state.Value == State.PreparationPhase) {
                AStarRecalculation.Instance.RecalculateGraph();
            }

            if (state.Value == State.BattlePhase) {
                //Manage unit lists
                unitsStillInBattle.Clear();
                foreach (Unit unit in unitsOnBattlefieldList) {
                    AddUnitToUnitsStillInBattleList(unit);
                }
            }
        }

        speedUpButtonActive = false;
    }

    [ServerRpc] 
    private void SetSpeedUpButtonActiveServerRpc() {
        SetSpeedUpButtonActiveClientRpc();
        speedUpButtonActive = true;
    }

    [ClientRpc]
    private void SetSpeedUpButtonActiveClientRpc() {
        OnSpeedUpButtonActivation?.Invoke(this, EventArgs.Empty);
    }

    #region SET PARAMETERS

    public void AddUnitToUnitListInBattlefield(Unit unit) {
        unitsOnBattlefieldList.Add(unit);
    }

    public void RemoveUnitFromUnitListInBattlefield(Unit unit) {
        unitsOnBattlefieldList.Remove(unit);
    }

    public void AddUnitToUnitsStillInBattleList(Unit unit) {
        unitsStillInBattle.Add(unit);
    }

    public void RemoveUnitFromUnitsStillInBattleList(Unit unit) {
        unitsStillInBattle.Remove(unit);

        if(unitsStillInBattle.Count == 0) {
            state.Value = State.BattlePhaseEnding;
        }
    }

    #endregion


    #region GET PARAMETERS

    public List<Unit> GetUnitsInBattlefieldList() {
        return unitsOnBattlefieldList;
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

    public bool AllPlayersLoaded() {
        return allPlayersLoaded;
    }

    public bool IsFirstPreparationPhase() {
        return isFirstPreparationPhase;
    }
    #endregion
}
