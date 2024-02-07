using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class HiddenTacticsLobby : MonoBehaviour
{
    public static HiddenTacticsLobby Instance { get; private set; }
    private Lobby joinedLobby;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication() {
        if(UnityServices.State != ServicesInitializationState.Initialized) {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(Random.Range(0,1000).ToString());

            await UnityServices.InitializeAsync();

            // TO DO : upgrade this anonymous signing method to a steam account signing method
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        };
    }

    public async void CreateLobby(string lobbyName, bool isPrivate) {
        try {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, HiddenTacticsMultiplayer.MAX_PLAYER_AMOUNT, new CreateLobbyOptions {
                IsPrivate = isPrivate
            });

            HiddenTacticsMultiplayer.Instance.StartHost();

        } catch(LobbyServiceException e){
            Debug.Log(e);
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

}
