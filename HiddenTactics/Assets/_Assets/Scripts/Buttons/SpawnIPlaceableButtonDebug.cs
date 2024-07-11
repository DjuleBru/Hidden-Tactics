using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpawnIPlaceableButtonDebug : SpawnIPlaceableButton
{
    [SerializeField] protected TroopSO debugTroopToSpawnSO;
    [SerializeField] protected BuildingSO debugBuildingToSpawnSO;

    protected void Start() {
        spawnIPlaceableButton.onClick.AddListener(() => {
            if(debugTroopToSpawnSO != null) {
                SpawnTroopButton();
            }
            if(debugBuildingToSpawnSO != null) {
                SpawnBuildingButton();
            }
        });
    }

    protected override void SpawnTroopButton() {
        int troopIndex = BattleDataManager.Instance.GetTroopSOIndex(debugTroopToSpawnSO);

        PlayerActionsManager.LocalInstance.SelectTroopToSpawn(troopIndex);
    }

    protected override void SpawnBuildingButton() {
        int buildingIndex = BattleDataManager.Instance.GetBuildingSOIndex(debugBuildingToSpawnSO);
        PlayerActionsManager.LocalInstance.SelectBuildingToSpawn(buildingIndex);
    }
}
