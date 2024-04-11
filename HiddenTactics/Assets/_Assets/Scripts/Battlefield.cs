using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Battlefield : NetworkBehaviour
{
    [SerializeField] public int playerNumber;
    [SerializeField] private Collider2D battlefieldBorderCollider;
    private BattlefieldAnimatorManager battlefieldAnimatorManager;

    private void Awake() {
        battlefieldAnimatorManager = GetComponent<BattlefieldAnimatorManager>();
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseStarting()) {
            battlefieldAnimatorManager.SlamBattlefields();
        }
        if(BattleManager.Instance.IsBattlePhaseEnding()) {
            battlefieldAnimatorManager.SplitBattlefields();
        }
    }
}
