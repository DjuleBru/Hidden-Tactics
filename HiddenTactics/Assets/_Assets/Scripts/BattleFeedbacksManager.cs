using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleFeedbacksManager : MonoBehaviour
{

    public static BattleFeedbacksManager Instance;
    [SerializeField] MMF_Player battlefieldsSlammedFeebacks;
    [SerializeField] BattlefieldAnimatorManager playerBattlefieldAnimatorManager;

    private bool battlefieldsSlammed;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        playerBattlefieldAnimatorManager.OnBattlefieldsSlammed += PlayerBattlefieldAnimatorManager_OnBattlefieldsSlammed;
    }

    private void PlayerBattlefieldAnimatorManager_OnBattlefieldsSlammed(object sender, System.EventArgs e) {
        PlayBattlefieldsSlammedFeedbacks();
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (BattleManager.Instance.IsPreparationPhase()) {
            battlefieldsSlammed = false;
        }
    }

    public void PlayBattlefieldsSlammedFeedbacks() {
        if(!battlefieldsSlammed) {
            battlefieldsSlammedFeebacks.PlayFeedbacks();
        }
        battlefieldsSlammed = true;
    }
}
