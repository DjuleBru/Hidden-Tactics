using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    private void Start() {
        BattleManager.Instance.OnAllPlayersLoaded += BattleManager_OnAllPlayersLoaded;
        Show();
    }

    private void BattleManager_OnAllPlayersLoaded(object sender, System.EventArgs e) {
        Hide();
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
