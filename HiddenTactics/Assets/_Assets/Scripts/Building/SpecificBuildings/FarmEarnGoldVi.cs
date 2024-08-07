using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingEarnGoldVisual : MonoBehaviour
{
    [SerializeField] private ParticleSystem goldPS;
    private Building building;

    private void Awake() {
        building = GetComponent<Building>();
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsPreparationPhase()) {

        }
    }
}
