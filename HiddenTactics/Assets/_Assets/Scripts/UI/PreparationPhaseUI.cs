using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreparationPhaseUI : MonoBehaviour
{

    [SerializeField] Image preparationPhaseTimerImage;
    [SerializeField] Button playerReadyButton;

    private bool isReady;

    private void Awake() {
        playerReadyButton.onClick.AddListener(() => {
            isReady = !isReady;
            PlayerReadyManager.Instance.SetPlayerReady(isReady);
        });
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        Show();
    }

    private void Update() {
        preparationPhaseTimerImage.fillAmount = BattleManager.Instance.GetPreparationPhaseTimerNormalized();
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (BattleManager.Instance.IsPreparationPhase()) {
            Show();
            return;
        }
        if(BattleManager.Instance.IsBattlePhase()) {
            SetPlayerUnready();
            return;
        }

        Hide();
    }

    private void SetPlayerUnready() {
        isReady = false;
        PlayerReadyManager.Instance.SetPlayerReady(isReady);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
