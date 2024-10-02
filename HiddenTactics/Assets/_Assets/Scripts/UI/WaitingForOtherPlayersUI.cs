using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForOtherPlayersUI : MonoBehaviour
{

    [SerializeField] private Animator cloudsBeforeStartAnimator;
    [SerializeField] private Image loadingBarFill;


    private void Start() {
        BattleManager.Instance.OnAllIPlaceablesSpawned += BattleManager_OnAllIPlaceablesSpawned;
        BattleManager.Instance.OnIPlaceableSpawned += BattleManager_OnIPlaceableSpawned;

        if (HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
            Show();
        } else {
            //Hide();
        }
    }

    private void BattleManager_OnIPlaceableSpawned(object sender, System.EventArgs e) {
        RefreshLoadingBar();
    }

    private void BattleManager_OnAllIPlaceablesSpawned(object sender, System.EventArgs e) {
        Hide();
    }

    private void RefreshLoadingBar() {
        loadingBarFill.fillAmount = BattleManager.Instance.GetIPlaceableSpawnProgress();
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        cloudsBeforeStartAnimator.SetTrigger("Fade");
        gameObject.SetActive(false);
    }
}
