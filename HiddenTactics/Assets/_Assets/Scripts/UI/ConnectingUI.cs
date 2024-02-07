using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
   private void Start() {
        HiddenTacticsMultiplayer.Instance.OnFailedToJoinGame += HiddenTacticsMultiplayer_OnFailedToJoinGame;
        HiddenTacticsMultiplayer.Instance.OnTryingToJoinGame += HiddenTacticsMultiplayer_OnTryingToJoinGame;

        Hide();
   }

    private void HiddenTacticsMultiplayer_OnTryingToJoinGame(object sender, System.EventArgs e) {
        Show();
    }

    private void HiddenTacticsMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e) {
        Hide();
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        HiddenTacticsMultiplayer.Instance.OnFailedToJoinGame -= HiddenTacticsMultiplayer_OnFailedToJoinGame;
        HiddenTacticsMultiplayer.Instance.OnTryingToJoinGame -= HiddenTacticsMultiplayer_OnTryingToJoinGame;
    }
}
