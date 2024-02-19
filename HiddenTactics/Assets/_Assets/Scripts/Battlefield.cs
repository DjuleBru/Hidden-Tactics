using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battlefield : MonoBehaviour
{
    [SerializeField] public int playerNumber;
    private BattlefieldAnimatorManager battlefieldAnimatorManager;

    private void Awake() {
        battlefieldAnimatorManager = GetComponent<BattlefieldAnimatorManager>();
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsBattlePhase()) {
            battlefieldAnimatorManager.SlamBattlefields();
        }
        if(BattleManager.Instance.IsPreparationPhase()) {
            battlefieldAnimatorManager.SplitBattlefields();
        }
    }
}
