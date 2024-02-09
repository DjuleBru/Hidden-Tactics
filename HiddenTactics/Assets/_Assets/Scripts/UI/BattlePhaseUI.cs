using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePhaseUI : MonoBehaviour
{
    [SerializeField] Image battlePhaseTimerImage;

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        Hide();
    }

    private void Update() {
        battlePhaseTimerImage.fillAmount = BattleManager.Instance.GetBattlePhaseTimerNormalized();
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsBattlePhase()) {
            Show();
        } else {
            Hide();
        }
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
