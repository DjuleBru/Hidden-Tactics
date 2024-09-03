using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public static class SceneLoader
{
    public enum Scene {
        MultiplayerCleanupScene,
        BattleScene,
        LoadingScene,
        LobbyScene,
        DeckSelectionScene,
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene) {
        SceneLoader.targetScene = targetScene;
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene) {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback() {
        SceneManager.LoadScene(targetScene.ToString());
    }

}
