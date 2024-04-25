using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using QFSW.QC;
using Sirenix.OdinInspector;

public class HiddenTacticsMultiplayer : NetworkBehaviour
{
    public static HiddenTacticsMultiplayer Instance;

    [SerializeField] private bool isMultiplayer;

    [SerializeField] private List<Sprite> playerIconSpriteList;

    private string playerName;
    private int playerIconSpriteId;

    public const int MAX_PLAYER_AMOUNT = 2;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    public event EventHandler<OnPlayerGoldChangedEventArgs> OnPlayerGoldChanged;
    public event EventHandler<OnPlayerRevenueChangedEventArgs> OnPlayerRevenueChanged;
    public class OnPlayerGoldChangedEventArgs: EventArgs {
        public int previousGold;
        public int newGold;
        public ulong clientId;
    }
    public class OnPlayerRevenueChangedEventArgs : EventArgs {
        public int newRevenue;
        public ulong clientId;
    }

    private NetworkList<PlayerData> playerDataNetworkList;

    private void Awake() {

        Application.targetFrameRate = 60;

        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;

        playerName = ES3.Load(PlayerSaveConstString.PLAYER_NAME_MULTIPLAYER, defaultValue: "Player#" + UnityEngine.Random.Range(0, 1000));
    }

    private void Start() {
        if (!isMultiplayer) {
            NetworkManager.Singleton.StartHost();
        }
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent) {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost() {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_Server_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId) {
        for (int i = 0; i < playerDataNetworkList.Count; i++) {
            PlayerData playerData = playerDataNetworkList[i];
            if(playerData.clientId == clientId) {
                // Disconnected!
                playerDataNetworkList.RemoveAt(i);
            }
        }
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId) {
        playerDataNetworkList.Add(new PlayerData {
            clientId = clientId,
        });

        // Set True player number (1 or 2)
        PlayerData playerData = GetPlayerDataFromClientId(clientId);
        playerData.truePlayerNumber = playerDataNetworkList.Count;

        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void NetworkManager_Server_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse) {

        if (SceneManager.GetActiveScene().name != SceneLoader.Scene.DeckSelectionScene.ToString()) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }

        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    public void StartClient() {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId) {
        if (!IsServer) return;
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
        SetPlayerNameServerRpc(GetPlayerName());
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId) {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId) {
        foreach (PlayerData playerData in playerDataNetworkList) {
            if(playerData.clientId == clientId) {
                return playerData;
            }
        }
        Debug.Log("player data not found");
        return default;
    }


    public int GetPlayerDataIndexFromClientId(ulong clientId) {
        for (int i = 0; i < playerDataNetworkList.Count; i++) {
            if (playerDataNetworkList[i].clientId == clientId) {
                return i;
            }
        }
        return -1;
    }

    public int GetOtherPlayerDataIndexFromClientId(ulong clientId) {
        for (int i = 0; i < playerDataNetworkList.Count; i++) {
            if (playerDataNetworkList[i].clientId != clientId) {
                return i;
            }
        }
        return -1;
    }


    [Button]
    public PlayerData GetLocalPlayerData() {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    [Button]
    public PlayerData GetLocalOpponentData() {
        foreach (PlayerData playerData in playerDataNetworkList) {
            if (playerData.clientId != NetworkManager.Singleton.LocalClientId) {
                return playerData;
            }
        }
        return default;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex) {
        return playerDataNetworkList[playerIndex];
    }

    public bool IsPlayerIndexConnected(int playerIndex) {
        return playerIndex < playerDataNetworkList.Count;
    }

    public void KickPlayer(ulong clientId) {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }

    public string GetPlayerName() {
        return playerName;
    }

    public void SetPlayerName(string playerName) {
        this.playerName = playerName;

        ES3.Save(PlayerSaveConstString.PLAYER_NAME_MULTIPLAYER, playerName);
    }

    public Sprite GetPlayerSprite(int iconSpriteId) {
        return playerIconSpriteList[iconSpriteId];
    }

    public void SetPlayerIconSprite(int iconSpriteId) {
        playerIconSpriteId = iconSpriteId;
        SetPlayerIconSpriteServerRpc(iconSpriteId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIconSpriteServerRpc(int iconSpriteId, ServerRpcParams serverRpcParams = default) {
        Debug.Log("SettingPlayerIconSprite to " + iconSpriteId);
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.iconSpriteId = iconSpriteId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }


    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerId = playerId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }


    public void DestroyIPlaceable(NetworkObjectReference iPlaceableNetworkObjectReference) {
        DestroyIPlaceableServerRpc(iPlaceableNetworkObjectReference);
    }

    [ServerRpc(RequireOwnership =false)]
    private void DestroyIPlaceableServerRpc(NetworkObjectReference iPlaceableNetworkObjectReference) {
        iPlaceableNetworkObjectReference.TryGet(out NetworkObject iPlaceableNetworkObject);

        RemoveIPlaceableFromGridClientRpc(iPlaceableNetworkObject);

        // IF IPLACEABLE IS TROOP : DESTROY UNITS IN TROOP
        if(iPlaceableNetworkObject.TryGetComponent<Troop>(out Troop troop)) {
            foreach (Unit unit in troop.GetUnitInTroopList()) {
                NetworkObject unitNetworkObject = unit.GetComponent<NetworkObject>();
                RemoveUnitFromGridClientRpc(unitNetworkObject);
                unit.DestroySelf();
            }
        }

        IPlaceable iPlaceable = iPlaceableNetworkObject.GetComponent<IPlaceable>();
        iPlaceable.DestroySelf();
    }

    [ClientRpc]
    public void RemoveUnitFromGridClientRpc(NetworkObjectReference unitNetworkObjectReference) {
        unitNetworkObjectReference.TryGet(out NetworkObject unitNetworkObject);
        Unit unit = unitNetworkObject.GetComponent<Unit>();

        GridPosition unitGridPosition = unit.GetCurrentGridPosition();
        BattleGrid.Instance.RemoveUnitAtGridPosition(unitGridPosition, unit);
    }

    [ClientRpc]
    public void RemoveIPlaceableFromGridClientRpc(NetworkObjectReference iPlaceableNetworkObjectReference) {
        iPlaceableNetworkObjectReference.TryGet(out NetworkObject iPlaceableNetworkObject);
        IPlaceable iPlaceable = iPlaceableNetworkObject.GetComponent<IPlaceable>();

        GridPosition iPlaceableGridPosition = iPlaceable.GetIPlaceableGridPosition();
        BattleGrid.Instance.RemoveIPlaceableAtGridPosition(iPlaceableGridPosition, iPlaceable);
    }

    public bool IsMultiplayer() {
        return isMultiplayer;
    }

    #region GOLD&VILLAGES

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerInitialConditionsServerRpc(ulong clientId) {
        Debug.Log("setting player initial conditions");
        int initialPlayerGold = PlayerGoldManager.Instance.GetPlayerInitialgGold();
        int initialPlayerVillages = 20;
        int initialPlayerRevenue = PlayerGoldManager.Instance.GetPlayerBaseIncome();

        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerGold = initialPlayerGold;
        playerData.playerVillagesRemaining = initialPlayerVillages;
        playerData.playerRevenue = initialPlayerRevenue;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    public void DistributePlayerRevenue() {
        for (int i = 0; i < playerDataNetworkList.Count; i++) {
            PlayerData playerData = playerDataNetworkList[i];
            ChangePlayerGoldServerRpc(playerData.clientId, playerData.playerRevenue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangePlayerGoldServerRpc(ulong clientId, int goldAmount) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerGold += goldAmount;

        playerDataNetworkList[playerDataIndex] = playerData;
        ChangePlayerGoldClientRpc(clientId, playerData.playerGold - goldAmount, playerData.playerGold);
    }

    [ClientRpc]
    private void ChangePlayerGoldClientRpc(ulong clientId, int previousGold, int newGold) {

        OnPlayerGoldChanged?.Invoke(this, new OnPlayerGoldChangedEventArgs {
            previousGold = previousGold,
            newGold = newGold,
            clientId = clientId,
        });
    }


    public void RemoveOnePlayerVillage(ulong clientID) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(clientID);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerVillagesRemaining--;
        playerDataNetworkList[playerDataIndex] = playerData;

        // Add revenue to player that lost the village
        ChangePlayerRevenueServerRpc(clientID, PlayerGoldManager.Instance.GetPlayerVillageDestroyedBonusIncome());

    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangePlayerRevenueServerRpc(ulong clientId, int revenueAmount) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerRevenue += revenueAmount;

        playerDataNetworkList[playerDataIndex] = playerData;

        ChangePlayerRevenueClientRpc(clientId, playerDataNetworkList[playerDataIndex].playerRevenue);
    }

    [ClientRpc]
    private void ChangePlayerRevenueClientRpc(ulong clientId, int newRevenue) {
        OnPlayerRevenueChanged?.Invoke(this, new OnPlayerRevenueChangedEventArgs {
            clientId = clientId,
            newRevenue = newRevenue
        });
    }

    #endregion

    public void CheckPlayerConditions(ulong clientID) {

        int playerDataIndex = GetPlayerDataIndexFromClientId(clientID);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        Debug.Log("player gold " + playerData.playerGold + " player villages " + playerData.playerVillagesRemaining +  " player revenue " + playerData.playerRevenue);
    }
}
