using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class HiddenTacticsLobby : MonoBehaviour
{
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    public static HiddenTacticsLobby Instance { get; private set; }
    private Lobby joinedLobby;

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnJoinFailed;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs {
        public List<Lobby> lobbyList;
    }

    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private void Update() {
        HandleLobbyHeartbeat();
        HandlePeriodicLobbyListRefresh();
    }

    private async void HandleLobbyHeartbeat() {
        if(IsLobbyHost()) {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0) {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private void HandlePeriodicLobbyListRefresh() {
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn && SceneManager.GetActiveScene().name == SceneLoader.Scene.LobbyScene.ToString()) {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0) {
                float lobbyUpdateTimerMax = 3f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                ListLobbies();
            }
        }
    }

    private bool IsLobbyHost() {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void ListLobbies() {
        try {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions() {
                Filters = new List<QueryFilter> {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs {
                lobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void InitializeUnityAuthentication() {
        if(UnityServices.State != ServicesInitializationState.Initialized) {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0,1000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);

            // TO DO : upgrade this anonymous signing method to a steam account signing method
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        };
    }

    private async Task<Allocation> AllocateRelay() {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(HiddenTacticsMultiplayer.MAX_PLAYER_AMOUNT - 1);

            return allocation;
        } catch(RelayServiceException e) {
            Debug.Log(e);
            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation) {
        try {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        } catch(RelayServiceException e) {
            Debug.Log(e);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode) {
        try {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        } catch(RelayServiceException e) {
            Debug.Log(e);
            return default;
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate) {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);

        try {

            CrossfadeTransition.Instance.FadeIn();
            MainMenuCameraManager.Instance.SetFadeInCamera();

            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, HiddenTacticsMultiplayer.MAX_PLAYER_AMOUNT, new CreateLobbyOptions {
                IsPrivate = isPrivate
            });

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    {KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            HiddenTacticsMultiplayer.Instance.StartHost();

            SceneLoader.LoadNetwork(SceneLoader.Scene.DeckSelectionScene);

        } catch(LobbyServiceException e) {
            Debug.Log(e);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void QuickJoin() {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            HiddenTacticsMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e) {
            Debug.Log(e);
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    public async void JoinWithCode(string lobbyCode) {
        if(lobbyCode == "") {
            lobbyCode = "XXX";
        }
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            HiddenTacticsMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e) {
            Debug.Log(e);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinWithId(string lobbyId) {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            HiddenTacticsMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e) {
            Debug.Log(e);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void DeleteLobby() {
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                joinedLobby = null;
            }
            catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void LeaveLobby() {
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            }
            catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void KickPlayer(string playerId) {
        if (IsLobbyHost()) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }



    public Lobby GetLobby() {
        return joinedLobby;
    }

}
