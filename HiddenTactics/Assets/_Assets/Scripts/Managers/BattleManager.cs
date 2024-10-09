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
    public event EventHandler OnGameEnded;
    public event EventHandler OnAllIPlaceablesSpawned;
    public event EventHandler OnIPlaceableSpawned;

    public bool allPlayersLoaded;
    public bool allIPlaceablesSpawned;
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

    private float battlePhaseTimerLocal;
    private float preparationPhaseTimerLocal;

    private float phaseTimerSyncTimer;
    private float phaseTimerSyncRate = .5f;

    private List<Unit> unitsOnBattlefieldList = new List<Unit>();
    private List<Unit> unitsStillInBattle = new List<Unit>();

    private int turnNumber;
    [SerializeField] private int maxTurnNumber;

    private TroopSO level1Mercenary;
    private TroopSO level2Mercenary;
    private TroopSO level3Mercenary;
    private TroopSO level4Mercenary;

    private enum State {
        WaitingToStart,
        PreparationPhase,
        BattlePhaseStarting,
        BattlePhase,
        BattlePhaseEnding,
        GameEnded,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>();

    private NetworkVariable<int> objectsSpawnedNumberServer = new NetworkVariable<int>();
    private int objectsSpawnedNumberClient;
    private int totalIPlaceablesToSpawn;
    private int totalIPlaceablesToSpawnClient;
    private int clientsConfirmedTotalObjectsSpawnedReceived;

    private List<ulong> spawnedObjectsIDs = new List<ulong>();

    private int clientsSetSpawnedIplaceablesNumber_Server;


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if(!HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
            //OnAllPlayersLoaded?.Invoke(this, EventArgs.Empty);
        }
        //StartCoroutine(DebugOnAllPlayersLoaded());
    }

    private IEnumerator DebugOnAllPlayersLoaded() {
        yield return new WaitForSeconds(1.5f);
        OnAllPlayersLoaded?.Invoke(this, EventArgs.Empty);
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;
        PlayerReadyManager.Instance.OnAllPlayersReady += PlayersReadyManager_OnAllPlayersReady;
        PlayerReadyManager.Instance.OnAllPlayersWantToSpeedUp += PlayersReadyManager_OnAllPlayersWantToSpeedUp;
        PlayerReadyManager.Instance.OnAllPlayersWantToReplay += PlayerReadyManager_OnAllPlayersWantToReplay;

        battleFieldAnimatorControllingPhases.OnBattlefieldsSlammed += BattleFieldAnimatorControllingPhases_OnBattlefieldsSlammed;
        battleFieldAnimatorControllingPhases.OnBattlefieldsSplit += BattleFieldAnimatorControllingPhases_OnBattlefieldsSplit;

        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            state.Value = State.WaitingToStart;
        }

        battlePhaseTimer.Value = battlePhaseMaxTime;
        battlePhaseTimerLocal = battlePhaseMaxTime;
        preparationPhaseTimer.Value = preparationPhaseMaxTime;
        preparationPhaseTimerLocal = preparationPhaseMaxTime;

        objectsSpawnedNumberServer.OnValueChanged += ObjectsSpawnedNumberServer_OnValueChanged;
    }


    private void Update() {

        if (!IsServer) {
            return;
        }

        phaseTimerSyncTimer -= Time.deltaTime;

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

                if (!HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
                    return;
                }


                if(preparationPhaseTimerLocal < 0 ) {
                    preparationPhaseTimer.Value = preparationPhaseMaxTime;
                    preparationPhaseTimerLocal = preparationPhaseMaxTime;
                    battlePhaseTimer.Value = battlePhaseMaxTime;
                    battlePhaseTimerLocal = battlePhaseMaxTime;

                    state.Value = State.BattlePhaseStarting;
                    return;
                }

                preparationPhaseTimerLocal -= Time.deltaTime;
                if (phaseTimerSyncTimer < 0) {
                    phaseTimerSyncTimer = phaseTimerSyncRate;
                    preparationPhaseTimer.Value = preparationPhaseTimerLocal;
                }

                break;

            case State.BattlePhase:
                // Timer
                if (!speedUpButtonActive && battlePhaseTimer.Value < (battlePhaseMaxTime - battlePhaseSpeedUpButtonActivationDelay)) {
                    SetSpeedUpButtonActiveServerRpc();
                }

                if (battlePhaseTimerLocal < 0) {
                    battlePhaseTimer.Value = battlePhaseMaxTime;
                    battlePhaseTimerLocal = battlePhaseMaxTime;
                    preparationPhaseTimer.Value = preparationPhaseMaxTime;
                    preparationPhaseTimerLocal = preparationPhaseMaxTime;

                    state.Value = State.BattlePhaseEnding;
                    return;
                }

                battlePhaseTimerLocal -= Time.deltaTime;
                if (phaseTimerSyncTimer < 0) {
                    phaseTimerSyncTimer = phaseTimerSyncRate;
                    battlePhaseTimer.Value = battlePhaseTimerLocal;
                }

                break;

            case State.BattlePhaseEnding:
            break;
            case State.GameEnded:
            break;
        }
    }

    #region LOAD PLAYER IPLACEABLES SPAWNING

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerIPlaceablesNumberToSpawnServerRpc(int objectsToSpawnNumber) {
        totalIPlaceablesToSpawn += objectsToSpawnNumber;
        clientsSetSpawnedIplaceablesNumber_Server++;

        if(clientsSetSpawnedIplaceablesNumber_Server == 2) {
            SetPlayerIPlaceablesNumberToSpawnClientRpc(totalIPlaceablesToSpawn);
        }
    }

    [ClientRpc]
    private void SetPlayerIPlaceablesNumberToSpawnClientRpc(int totalObjectsToSpawnNumber) {
        Debug.Log("SetPlayerIPlaceablesNumberToSpawnClientRpc " + totalObjectsToSpawnNumber);

        totalIPlaceablesToSpawnClient = totalObjectsToSpawnNumber;
        ConfirmTotalObjectsToSpawnNumberReceivedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ConfirmTotalObjectsToSpawnNumberReceivedServerRpc() {
        clientsConfirmedTotalObjectsSpawnedReceived++;

        Debug.Log("ConfirmTotalObjectsToSpawnNumberReceivedServerRpc " + clientsConfirmedTotalObjectsSpawnedReceived);
        if (clientsConfirmedTotalObjectsSpawnedReceived == 2) {
            ConfirmTotalObjectsToSpawnNumberReceivedClientRpc();
        }
    }

    [ClientRpc]
    private void ConfirmTotalObjectsToSpawnNumberReceivedClientRpc() {
        Debug.Log("ConfirmTotalObjectsToSpawnNumberReceivedClientRpc ");
        VillageManager.Instance.GeneratePlayerVillagesServerRpc(NetworkManager.Singleton.LocalClientId);
        PlayerAction_SpawnIPlaceable.LocalInstance.SpawnPlayerIPlaceablePoolList(NetworkManager.Singleton.LocalClientId, DeckManager.LocalInstance.GetDeckSelected());
        PlayerAction_SpawnIPlaceable.LocalInstance.SpawnMercenaryPool(NetworkManager.Singleton.LocalClientId);
    }

    public void AddIPlaceableSpawned(ulong networkObjectID) {
        AddIPlaceableSpawnedServerRpc(networkObjectID);
    }

    private void ObjectsSpawnedNumberServer_OnValueChanged(int previousValue, int newValue) {
        Debug.Log("ObjectsSpawnedNumberServer_OnValueChanged " + objectsSpawnedNumberServer.Value);
        OnIPlaceableSpawned?.Invoke(this, EventArgs.Empty);

        if (objectsSpawnedNumberServer.Value >= totalIPlaceablesToSpawnClient && !allIPlaceablesSpawned) {
            OnAllIPlaceablesSpawned?.Invoke(this, EventArgs.Empty);
            allIPlaceablesSpawned = true;
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void AddIPlaceableSpawnedServerRpc(ulong networkObjectID) {

        Debug.Log("BattleManager AddIPlaceableSpawnedServerRpc " + networkObjectID);

        if (spawnedObjectsIDs.Contains(networkObjectID)) {
            // Second time the ID is being added to the list : object has been loaded on both clients
            objectsSpawnedNumberServer.Value++;
            //ConfirmIPlaceableSpawnedClientRpc(objectsSpawnedNumberServer, networkObjectID);
        }
        else {
            spawnedObjectsIDs.Add(networkObjectID);
        }
    }

    [ClientRpc]
    private void ConfirmIPlaceableSpawnedClientRpc(int totalObjectsSpawned, ulong networkObjectID) {
        objectsSpawnedNumberClient = totalObjectsSpawned;
        Debug.Log("Total iPlaceable spawned on both clients = " + objectsSpawnedNumberClient + " / " + totalIPlaceablesToSpawnClient + " NETWORKID " + networkObjectID);

        OnIPlaceableSpawned?.Invoke(this, EventArgs.Empty);

        if (objectsSpawnedNumberClient == totalIPlaceablesToSpawnClient) {
            OnAllIPlaceablesSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    public float GetIPlaceableSpawnProgress() {
        return (float)objectsSpawnedNumberServer.Value / (float)totalIPlaceablesToSpawnClient;
    }

    #endregion



    [ServerRpc(RequireOwnership = false)]
    private void SetMercenariesForBattleServerRpc() {
        int level1Mercenary = UnityEngine.Random.Range(0, BattleDataManager.Instance.GetLevel1MercenaryTroopSOList().Count);
        int level2Mercenary = UnityEngine.Random.Range(0, BattleDataManager.Instance.GetLevel2MercenaryTroopSOList().Count);
        int level3Mercenary = UnityEngine.Random.Range(0, BattleDataManager.Instance.GetLevel3MercenaryTroopSOList().Count);
        int level4Mercenary = UnityEngine.Random.Range(0, BattleDataManager.Instance.GetLevel4MercenaryTroopSOList().Count);

        SetMercenariesForBattleClientRpc(level1Mercenary, level2Mercenary, level3Mercenary, level4Mercenary);
    }

    [ClientRpc]
    private void SetMercenariesForBattleClientRpc(int level1MercenaryIndex, int level2MercenaryIndex, int level3MercenaryIndex, int level4MercenaryIndex) {
        level1Mercenary = BattleDataManager.Instance.GetLevel1MercenaryTroopSOList()[level1MercenaryIndex];
        level2Mercenary = BattleDataManager.Instance.GetLevel2MercenaryTroopSOList()[level2MercenaryIndex];
        level3Mercenary = BattleDataManager.Instance.GetLevel3MercenaryTroopSOList()[level3MercenaryIndex];
        level4Mercenary = BattleDataManager.Instance.GetLevel4MercenaryTroopSOList()[level4MercenaryIndex];
    }

    private void PlayersReadyManager_OnAllPlayersReady(object sender, EventArgs e) {
        state.Value = State.BattlePhaseStarting;
    }

    private void PlayersReadyManager_OnAllPlayersWantToSpeedUp(object sender, EventArgs e) {
        Time.timeScale = 1.75f;
    }

    private void PlayerReadyManager_OnAllPlayersWantToReplay(object sender, EventArgs e) {
        Debug.Log("Battle manager - All players want to replay");
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
        Debug.Log("OnAllPlayersLoaded");
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

    public void SetBattlePhase() {
        state.Value = State.BattlePhase;
    }

    public void SetPreparationPhase() {
        state.Value = State.PreparationPhase;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void BattleFieldAnimatorControllingPhases_OnBattlefieldsSplit(object sender, EventArgs e) {
        if (!IsServer) {
            return;
        }
        state.Value = State.PreparationPhase;
    }

    private void State_OnValueChanged(State previousValue, State newValue) {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
        Time.timeScale = 1f;

        HandleTurnPass();

        if (IsServer) {
            if(state.Value == State.PreparationPhase) {
                AStarRecalculation.Instance.RecalculateGraph();
                StartCoroutine(ResetAllUnits());
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

    private IEnumerator ResetAllUnits() {

        yield return new WaitForSeconds(.1f);

    }

    private void HandleTurnPass() {
        // Change turn number on clients
        if (state.Value == State.PreparationPhase) {
            turnNumber++;

            if(turnNumber == 1) {
                if(IsServer) {
                    SetMercenariesForBattleServerRpc();
                }
            }

            // End game if turn > max turns
            if (turnNumber == maxTurnNumber) {
                EndGame();
            }

        }
    }

    public void EndGame() {
        state.Value = State.GameEnded;
        OnGameEnded?.Invoke(this, EventArgs.Empty);
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

    public bool GameEnded() {
        return state.Value == State.GameEnded;
    }

    public bool AllPlayersLoaded() {
        return allPlayersLoaded;
    }

    public bool IsFirstPreparationPhase() {
        return isFirstPreparationPhase;
    }

    public int GetCurrentTurn() {
        return turnNumber;
    }

    public int GetMaxTurns() {
        return maxTurnNumber;
    }

    public TroopSO GetLevel1Mercenary() {
        return level1Mercenary;
    }
    public TroopSO GetLevel2Mercenary() {
        return level2Mercenary;
    }
    public TroopSO GetLevel3Mercenary() {
        return level3Mercenary;
    }
    public TroopSO GetLevel4Mercenary() {
        return level4Mercenary;
    }

    #endregion
}
