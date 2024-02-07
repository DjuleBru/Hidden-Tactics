using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    private void Awake() {
        createGameButton.onClick.AddListener(() => {
            HiddenTacticsMultiplayer.Instance.StartHost();
            SceneLoader.LoadNetwork(SceneLoader.Scene.DeckSelectionScene);
        });
        joinGameButton.onClick.AddListener(() => {
            HiddenTacticsMultiplayer.Instance.StartClient();
        });
    }
}
