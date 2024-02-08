using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button multiplayerButton;
    [SerializeField] Button exitGameButton;

    private void Awake() {
        multiplayerButton.onClick.AddListener(() => {
            SceneLoader.Load(SceneLoader.Scene.LobbyScene);
        });
        exitGameButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }
}
