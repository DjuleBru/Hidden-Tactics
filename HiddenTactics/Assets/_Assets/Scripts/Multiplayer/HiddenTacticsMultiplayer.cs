using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using QFSW.QC;

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

    private NetworkList<PlayerData> playerDataNetworkList;

    private void Awake() {
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
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
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

    public PlayerData GetPlayerData() {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
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


    public void DestroyTroop(NetworkObjectReference troopNetworkObjectReference) {
        DestroyTroopServerRpc(troopNetworkObjectReference);
    }

    [ServerRpc(RequireOwnership =false)]
    private void DestroyTroopServerRpc(NetworkObjectReference troopNetworkObjectReference) {
        troopNetworkObjectReference.TryGet(out NetworkObject troopNetworkObject);
        RemoveTroopFromGridClientRpc(troopNetworkObjectReference);

        Troop troop = troopNetworkObject.GetComponent<Troop>();


        troop.DestroySelf();
    }

    [ClientRpc]
    public void RemoveTroopFromGridClientRpc(NetworkObjectReference troopNetworkObjectReference) {
        troopNetworkObjectReference.TryGet(out NetworkObject troopNetworkObject);
        Troop troop = troopNetworkObject.GetComponent<Troop>();

        GridPosition troopGridPosition = troop.GetTroopGridPosition();
        BattleGrid.Instance.RemoveTroopAtGridPosition(troopGridPosition, troop);
    }

    public bool IsMultiplayer() {
        return isMultiplayer;
    }
}
