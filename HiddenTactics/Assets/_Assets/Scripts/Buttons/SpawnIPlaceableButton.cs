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
                SpawnTroopButton();
            });
        }

        if(buildingToSpawnSO != null ) {
            spawnIPlaceableButton.onClick.AddListener(() => {
                SpawnBuildingButton();
            });
        }
    }

    private void SpawnTroopButton() {
        if(!CheckSpawnConditions()) return;
        int troopIndex = BattleDataManager.Instance.GetTroopSOIndex(troopToSpawnSO);
        PlayerStateUI.Instance.SetPlayerGoldChangingUI(troopToSpawnSO.spawnTroopCost);

        PlayerActionsManager.LocalInstance.SelectTroopToSpawn(troopIndex);
    }

    private void SpawnBuildingButton() {
        if (!CheckSpawnConditions()) return;
        int buildingIndex = BattleDataManager.Instance.GetBuildingSOIndex(buildingToSpawnSO);


        PlayerActionsManager.LocalInstance.SelectBuildingToSpawn(buildingIndex);
    }

    private bool CheckSpawnConditions() {

        if(troopToSpawnSO != null) {
            if(PlayerGoldManager.Instance.CanSpendGold(troopToSpawnSO.spawnTroopCost, NetworkManager.Singleton.LocalClientId)) {
                return true;
            } else {
                return false; 
            }
        }

        if(buildingToSpawnSO != null) {
            if (PlayerGoldManager.Instance.CanSpendGold(buildingToSpawnSO.spawnBuildingCost, NetworkManager.Singleton.LocalClientId)) {
                return true;
            }
            else {
                return false;
            }
        }
        return false;
    }
}
