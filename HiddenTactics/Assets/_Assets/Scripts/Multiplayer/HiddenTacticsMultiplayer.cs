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

    private string playerName;
    private int playerIconSpriteId;
    private int playerFactionId;
    private int playerGridVisualSOId;
    private List<int> villageSpriteIdList;
    private int playerBattlefieldBaseSpriteId;
    private int playerRevenueSinglePlayer;
    private int playerGoldSinglePlayer;

    public const int MAX_PLAYER_AMOUNT = 2;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler OnPlayerCustomizationDataNetworkListChanged;
    public event EventHandler OnPlayerSurrendered;
    public event EventHandler OnOpponentSurrendered;

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
    private NetworkList<PlayerCustomizationData> playerCustomizationDataNetworkList;

    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;

        playerCustomizationDataNetworkList = new NetworkList<PlayerCustomizationData>();
        playerCustomizationDataNetworkList.OnListChanged += PlayerCustomizationDataNetworkList_OnListChanged;

        LoadPlayerCustomizationData();
    }

    private void Start() {
        if (!isMultiplayer) {
            NetworkManager.Singleton.StartHost();
        }
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent) {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PlayerCustomizationDataNetworkList_OnListChanged(NetworkListEvent<PlayerCustomizationData> changeEvent) {
        OnPlayerCustomizationDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerId = playerId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    public bool IsPlayerIndexConnected(int playerIndex) {
        return playerIndex < playerDataNetworkList.Count;
    }

    public void KickPlayer(ulong clientId) {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }
    public bool IsMultiplayer() {
        return isMultiplayer;
    }
    public bool GetPlayerAndOpponentSameFaction() {
        if (GetLocalPlayerCustomizationData().factionID == GetLocalOpponentCustomizationData().factionID) return true;
        return false;
    }

    #region HOST
    public void StartHost() {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_Server_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId) {
        for (int i = 0; i < playerDataNetworkList.Count; i++) {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId) {
                // Disconnected!
                playerDataNetworkList.RemoveAt(i);
            }
        }

        for (int i = 0; i < playerCustomizationDataNetworkList.Count; i++) {
            PlayerCustomizationData playerCustomizationData = playerCustomizationDataNetworkList[i];
            if (playerCustomizationData.clientId == clientId) {
                // Disconnected!
                playerCustomizationDataNetworkList.RemoveAt(i);
            }
        }

        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId) {
        Debug.Log("SERVER client connected :" + clientId);
        playerDataNetworkList.Add(new PlayerData {
            clientId = clientId,
        });

        playerCustomizationDataNetworkList.Add(new PlayerCustomizationData {
            clientId = clientId,
        });

        // Set True player number (1 or 2)
        PlayerData playerData = GetPlayerDataFromClientId(clientId);
        playerData.truePlayerNumber = playerDataNetworkList.Count;

        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerFactionServerRpc(GetPlayerFactionSOId());
        SetPlayerIconSpriteServerRpc(GetPlayerIconSpriteId());
        SetPlayerBattlefieldBaseSpriteServerRpc(GetPlayerBattlefieldBaseSpriteId());
        SetPlayerGridVisualSOServerRpc(GetPlayerGridVisualSOId());
        SetPlayerVillageSpritesServerRpc(GetPlayerVillageSpriteIdList().ToArray());

        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void NetworkManager_Server_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse) {

        if (SceneManager.GetActiveScene().name != SceneLoader.Scene.DeckSelectionScene.ToString()) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    #endregion

    #region CLIENT
    public void StartClient() {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId) {
        Debug.Log("CLIENT client connected " + clientId);

        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
        SetPlayerFactionServerRpc(GetPlayerFactionSOId());
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIconSpriteServerRpc(GetPlayerIconSpriteId());
        SetPlayerBattlefieldBaseSpriteServerRpc(GetPlayerBattlefieldBaseSpriteId());
        SetPlayerGridVisualSOServerRpc(GetPlayerGridVisualSOId());
        SetPlayerVillageSpritesServerRpc(GetPlayerVillageSpriteIdList().ToArray());
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId) {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region HANDLE IPLACEABLES
    public void DestroyIPlaceable(NetworkObjectReference iPlaceableNetworkObjectReference) {
        DestroyIPlaceableServerRpc(iPlaceableNetworkObjectReference);
    }

    [ServerRpc(RequireOwnership =false)]
    private void DestroyIPlaceableServerRpc(NetworkObjectReference iPlaceableNetworkObjectReference) {
        iPlaceableNetworkObjectReference.TryGet(out NetworkObject iPlaceableNetworkObject);
        if (iPlaceableNetworkObject == null) return;

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

        Debug.Log("removed IPlaceable client received");
    }
    #endregion

    #region GET PLAYER DATA
    public PlayerData GetPlayerDataFromClientId(ulong clientId) {
        foreach (PlayerData playerData in playerDataNetworkList) {
            if (playerData.clientId == clientId) {
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
    #endregion

    #region GET PLAYER CUSTOMIZATION DATA
    public PlayerCustomizationData GetPlayerCustomizationDataFromClientId(ulong clientId) {
        foreach (PlayerCustomizationData playerCustomizationData in playerCustomizationDataNetworkList) {
            if (playerCustomizationData.clientId == clientId) {
                return playerCustomizationData;
            }
        }
        return default;
    }

    public int GetPlayerCustomizationDataIndexFromClientId(ulong clientId) {
        for (int i = 0; i < playerCustomizationDataNetworkList.Count; i++) {
            if (playerCustomizationDataNetworkList[i].clientId == clientId) {
                return i;
            }
        }
        return -1;
    }

    public int GetOtherPlayerCustomizationDataIndexFromClientId(ulong clientId) {
        for (int i = 0; i < playerCustomizationDataNetworkList.Count; i++) {
            if (playerCustomizationDataNetworkList[i].clientId != clientId) {
                return i;
            }
        }
        return -1;
    }

    [Button]
    public PlayerCustomizationData GetLocalPlayerCustomizationData() {
        return GetPlayerCustomizationDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    [Button]
    public PlayerCustomizationData GetLocalOpponentCustomizationData() {
        foreach (PlayerCustomizationData playerCustomizationData in playerCustomizationDataNetworkList) {
            if (playerCustomizationData.clientId != NetworkManager.Singleton.LocalClientId) {
                return playerCustomizationData;
            }
        }
        return default;
    }

    public PlayerCustomizationData GetPlayerCustomizationDataFromPlayerIndex(int playerIndex) {
        return playerCustomizationDataNetworkList[playerIndex];
    }

    #endregion

    #region GOLD&VILLAGES

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerInitialConditionsServerRpc(ulong clientId) {

        int initialPlayerGold = PlayerGoldManager.Instance.GetPlayerInitialGold();
        int initialPlayerVillages = 20;
        int initialPlayerRevenue = PlayerGoldManager.Instance.GetPlayerBaseIncome() + Mathf.FloorToInt(initialPlayerGold*PlayerGoldManager.Instance.GetPlayerSavingsRevenueRate());

        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerGold = initialPlayerGold;
        playerData.playerVillagesRemaining = initialPlayerVillages;
        playerData.playerRevenue = initialPlayerRevenue;

        playerRevenueSinglePlayer = initialPlayerRevenue;
        playerGoldSinglePlayer = initialPlayerGold;

        playerDataNetworkList[playerDataIndex] = playerData;

        OnPlayerGoldChanged?.Invoke(this, new OnPlayerGoldChangedEventArgs {
            previousGold = playerData.playerGold,
            newGold = playerData.playerGold
        });
    }

    public int GetPlayerSavingsRevenue(ulong clientId) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        int playerSavingsRevenue = Mathf.FloorToInt(playerData.playerGold * PlayerGoldManager.Instance.GetPlayerSavingsRevenueRate());
        return playerSavingsRevenue;
    }

    public int GetPlayerRevenue(ulong clientId) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        return playerData.playerRevenue;
    }

    public void DistributePlayerRevenue() {
        for (int i = 0; i < playerDataNetworkList.Count; i++) {
            PlayerData playerData = playerDataNetworkList[i];
            ChangePlayerGoldServerRpc(playerData.clientId, playerData.playerRevenue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangePlayerGoldServerRpc(ulong clientId, int goldAmount) {

        if(isMultiplayer) {

            int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
            PlayerData playerData = playerDataNetworkList[playerDataIndex];

            int initialPlayerSavingsRevenue = GetPlayerSavingsRevenue(clientId);

            playerData.playerGold += goldAmount;
            playerDataNetworkList[playerDataIndex] = playerData;

            int newPlayerSavingsRevenue = GetPlayerSavingsRevenue(clientId);

            int changeInPlayerSavingsRevenue = newPlayerSavingsRevenue - initialPlayerSavingsRevenue;
            ChangePlayerRevenueServerRpc(clientId, changeInPlayerSavingsRevenue);

            ChangePlayerGoldClientRpc(clientId, playerData.playerGold - goldAmount, playerData.playerGold);

        } else {

            int newGold = playerGoldSinglePlayer + goldAmount;
            OnPlayerGoldChanged?.Invoke(this, new OnPlayerGoldChangedEventArgs {
                previousGold = playerGoldSinglePlayer,
                newGold = newGold,
                clientId = clientId,
            });
            playerGoldSinglePlayer = newGold;

        }
    }

    [ClientRpc]
    public void ChangePlayerGoldClientRpc(ulong clientId, int previousGold, int newGold) {

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

        if(isMultiplayer) {
            int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
            PlayerData playerData = playerDataNetworkList[playerDataIndex];

            playerData.playerRevenue += revenueAmount;

            playerDataNetworkList[playerDataIndex] = playerData;

            ChangePlayerRevenueClientRpc(clientId, playerDataNetworkList[playerDataIndex].playerRevenue);
        } else {
            playerRevenueSinglePlayer += revenueAmount;
            OnPlayerRevenueChanged?.Invoke(this, new OnPlayerRevenueChangedEventArgs {
                clientId = clientId,
                newRevenue = playerRevenueSinglePlayer
            });
        }
    }

    [ClientRpc]
    private void ChangePlayerRevenueClientRpc(ulong clientId, int newRevenue) {
        OnPlayerRevenueChanged?.Invoke(this, new OnPlayerRevenueChangedEventArgs {
            clientId = clientId,
            newRevenue = newRevenue
        });
    }

    public int GetPlayerGoldSinglePlayer() {
        return playerGoldSinglePlayer;
    }

    public void SetPlayerSurrender(ulong clientId) {
        SetPlayerSurrenderServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerSurrenderServerRpc(ulong clientId) {
        SetPlayerSurrendererClientRpc(clientId);
    }

    [ClientRpc]
    private void SetPlayerSurrendererClientRpc(ulong clientId) {
        if(clientId == NetworkManager.Singleton.LocalClientId) {
            OnPlayerSurrendered?.Invoke(this, EventArgs.Empty);
        } else {
            OnOpponentSurrendered?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool PlayerWon(ulong clientId) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        int opponentDataIndex = GetOtherPlayerDataIndexFromClientId(clientId);
        PlayerData opponentPlayerData = playerDataNetworkList[opponentDataIndex];

        if(playerData.playerVillagesRemaining > opponentPlayerData.playerVillagesRemaining) {
            // Player has more villages remaining : he won !
            return true;
        }

        if(playerData.playerVillagesRemaining == opponentPlayerData.playerVillagesRemaining) {
            // Tie on villages : 
        }

        // Opponent won
        return false;
    }

    public bool PlayerTie(ulong clientId) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        int opponentDataIndex = GetOtherPlayerDataIndexFromClientId(clientId);
        PlayerData opponentPlayerData = playerDataNetworkList[opponentDataIndex];

        if (playerData.playerVillagesRemaining == opponentPlayerData.playerVillagesRemaining) {
            // Tie on villages 
            return true;
        }

        // Opponent won
        return false;
    }


    #endregion

    #region PLAYER CUSTOMIZATION
    private void LoadPlayerCustomizationData() {
        playerName = SavingManager.Instance.LoadPlayerName();
        playerIconSpriteId = SavingManager.Instance.LoadPlayerIconSpriteId();
        playerFactionId = SavingManager.Instance.LoadPlayerFactionId();
        playerGridVisualSOId = SavingManager.Instance.LoadPlayerGridVisualSOId();
        playerBattlefieldBaseSpriteId = SavingManager.Instance.LoadPlayerBattlefieldBaseSpriteId();
        villageSpriteIdList = SavingManager.Instance.LoadPlayerVillageSpriteIdList();
    }
    public string GetPlayerName() {
        return playerName;
    }

    public int GetPlayerIconSpriteId() {
        return playerIconSpriteId;
    }

    public int GetPlayerFactionSOId() {
        return playerFactionId;
    }

    public int GetPlayerGridVisualSOId() {
        return playerGridVisualSOId;
    }

    public int GetPlayerBattlefieldBaseSpriteId() {
        return playerBattlefieldBaseSpriteId;
    }

    public List<int> GetPlayerVillageSpriteIdList() {
        return villageSpriteIdList;
    }

    public void SetPlayerName(string playerName) {
        this.playerName = playerName;
        SavingManager.Instance.SavePlayerName(playerName);
    }

    public void SetPlayerIconSprite(int iconSpriteId) {
        playerIconSpriteId = iconSpriteId;
        SavingManager.Instance.SavePlayerIconSpriteId(iconSpriteId);
    }

    public void SetPlayerFactionSO(int factionID) {
        playerFactionId = factionID;
        SavingManager.Instance.SavePlayerFactionId(factionID);
    }

    public void SetPlayerGridVisualSO(int gridVisualSOId) {
        playerGridVisualSOId = gridVisualSOId;
        SavingManager.Instance.SavePlayerGridVisualSOId(gridVisualSOId);
    }

    public void SetPlayerBattlefieldBaseSO(int battlefieldBaseSpriteId) {
        playerBattlefieldBaseSpriteId = battlefieldBaseSpriteId;
        SavingManager.Instance.SavePlayerBattlefieldBaseSpriteId(battlefieldBaseSpriteId);
    }

    public void SetPlayerVillageSprites(List<int> villageSpriteIdList) {
        this.villageSpriteIdList = villageSpriteIdList;
        SavingManager.Instance.SavePlayerVillagesSpriteIdList(villageSpriteIdList);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default) {
        int playerCustomizationDataIndex = GetPlayerCustomizationDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerCustomizationData playerCustomizationData = playerCustomizationDataNetworkList[playerCustomizationDataIndex];

        playerCustomizationData.playerName = playerName;

        playerCustomizationDataNetworkList[playerCustomizationDataIndex] = playerCustomizationData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerFactionServerRpc(int factionId, ServerRpcParams serverRpcParams = default) {
        int playerCustomizationDataIndex = GetPlayerCustomizationDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerCustomizationData playerCustomizationData = playerCustomizationDataNetworkList[playerCustomizationDataIndex];

        playerCustomizationData.factionID = factionId;

        playerCustomizationDataNetworkList[playerCustomizationDataIndex] = playerCustomizationData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIconSpriteServerRpc(int iconSpriteId, ServerRpcParams serverRpcParams = default) {
        int playerCustomizationDataIndex = GetPlayerCustomizationDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerCustomizationData playerCustomizationData = playerCustomizationDataNetworkList[playerCustomizationDataIndex];

        playerCustomizationData.iconSpriteId = iconSpriteId;

        playerCustomizationDataNetworkList[playerCustomizationDataIndex] = playerCustomizationData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerBattlefieldBaseSpriteServerRpc(int battlefieldBaseSpriteId, ServerRpcParams serverRpcParams = default) {
        int playerCustomizationDataIndex = GetPlayerCustomizationDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerCustomizationData playerCustomizationData = playerCustomizationDataNetworkList[playerCustomizationDataIndex];

        playerCustomizationData.battlefieldBaseSOId = battlefieldBaseSpriteId;

        playerCustomizationDataNetworkList[playerCustomizationDataIndex] = playerCustomizationData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerGridVisualSOServerRpc(int gridVisualSOId, ServerRpcParams serverRpcParams = default) {
        int playerCustomizationDataIndex = GetPlayerCustomizationDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerCustomizationData playerCustomizationData = playerCustomizationDataNetworkList[playerCustomizationDataIndex];

        playerCustomizationData.gridVisualSOId = gridVisualSOId;

        playerCustomizationDataNetworkList[playerCustomizationDataIndex] = playerCustomizationData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerVillageSpritesServerRpc(int[] villageSpriteIdList, ServerRpcParams serverRpcParams = default) {
        int playerCustomizationDataIndex = GetPlayerCustomizationDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerCustomizationData playerCustomizationData = playerCustomizationDataNetworkList[playerCustomizationDataIndex];
        playerCustomizationData.villageSpriteNumber = villageSpriteIdList.Length;

        if (villageSpriteIdList.Length >= 1) {
            playerCustomizationData.villageSprite0Id = villageSpriteIdList[0];
        }

        if (villageSpriteIdList.Length >= 2) {
            playerCustomizationData.villageSprite1Id = villageSpriteIdList[1];
        }

        if (villageSpriteIdList.Length >= 3) {
            playerCustomizationData.villageSprite2Id = villageSpriteIdList[2];
        }

        if (villageSpriteIdList.Length >= 4) {
            playerCustomizationData.villageSprite3Id = villageSpriteIdList[3];
        }

        if (villageSpriteIdList.Length >= 5) {
            playerCustomizationData.villageSprite4Id = villageSpriteIdList[4];
        }

        if (villageSpriteIdList.Length >= 6) {
            playerCustomizationData.villageSprite5Id = villageSpriteIdList[5];
        }

        playerCustomizationDataNetworkList[playerCustomizationDataIndex] = playerCustomizationData;
    }

    #endregion
}
