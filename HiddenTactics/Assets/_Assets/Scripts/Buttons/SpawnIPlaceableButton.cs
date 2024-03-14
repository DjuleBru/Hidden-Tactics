using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpawnIPlaceableButton : MonoBehaviour
{
    [SerializeField] TroopSO troopToSpawnSO;
    [SerializeField] BuildingSO buildingToSpawnSO;

    private Button spawnIPlaceableButton;

    private void Awake() {
        spawnIPlaceableButton = GetComponent<Button>();

        if(troopToSpawnSO != null ) {
            spawnIPlaceableButton.onClick.AddListener(() => {
                int troopIndex = BattleDataManager.Instance.GetTroopSOIndex(troopToSpawnSO);
                PlayerActionsManager.LocalInstance.SelectTroopToSpawn(troopIndex);
            });
        }

        if(buildingToSpawnSO != null ) {
            spawnIPlaceableButton.onClick.AddListener(() => {
                int buildingIndex = BattleDataManager.Instance.GetBuildingSOIndex(buildingToSpawnSO);
                PlayerActionsManager.LocalInstance.SelectBuildingToSpawn(buildingIndex);
            });
        }

    }

}
