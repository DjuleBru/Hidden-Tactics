using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanup : MonoBehaviour
{
    private void Awake() {
        if(NetworkManager.Singleton != null) {
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if(HiddenTacticsLobby.Instance != null) {
            Destroy(HiddenTacticsLobby.Instance.gameObject);
        }
        if(HiddenTacticsMultiplayer.Instance != null) {
            Destroy(HiddenTacticsMultiplayer.Instance.gameObject);
        }
        SceneLoader.Load(SceneLoader.Scene.LobbyScene);
    }
}
