using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerPlayerDataManager : NetworkBehaviour
{
    public static MultiplayerPlayerDataManager Instance;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";

    [SerializeField] private List<Sprite> playerIconSpriteList;

    private string playerName;

    public Sprite GetPlayerSprite(int iconId) {
        return playerIconSpriteList[iconId];
    }

    private void Awake() {
        Instance = this;

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "Player#" + Random.Range(0,1000));
        DontDestroyOnLoad(gameObject);
    }

    public string GetPlayerName() {
        return playerName;
    }

    public void SetPlayerName(string playerName) {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }


}
