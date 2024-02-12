using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreparationPhaseUI : MonoBehaviour
{

    [SerializeField] Image preparationPhaseTimerImage;
    [SerializeField] Button playerReadyButton;

    private void Awake() {
        playerReadyButton.onClick.AddListener(() => {
            Player.LocalInstance.SetPlayerReadyOrUnready();
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
        if (BattleManager.Instance.IsPreparationPhase()) {;
            Show();
            return;
        }

        Hide();
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
