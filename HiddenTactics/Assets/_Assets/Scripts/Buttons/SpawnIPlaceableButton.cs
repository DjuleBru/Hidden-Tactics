using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpawnIPlaceableButton : MonoBehaviour
{
    protected TroopSO troopToSpawnSO;
    protected BuildingSO buildingToSpawnSO;

    protected Button spawnIPlaceableButton;

    protected void Awake() {
        spawnIPlaceableButton = GetComponent<Button>();
    }

    protected virtual void SpawnTroopButton() {
        if (!CheckSpawnConditions()) return;
        int troopIndex = BattleDataManager.Instance.GetTroopSOIndex(troopToSpawnSO);
        PlayerStateUI.Instance.SetPlayerGoldChangingUI(-troopToSpawnSO.spawnTroopCost);

        PlayerActionsManager.LocalInstance.SelectTroopToSpawn(troopIndex);
    }

    protected virtual void SpawnBuildingButton() {
        if (!CheckSpawnConditions()) return;
        int buildingIndex = BattleDataManager.Instance.GetBuildingSOIndex(buildingToSpawnSO);
        PlayerActionsManager.LocalInstance.SelectBuildingToSpawn(buildingIndex);
    }

    protected virtual bool CheckSpawnConditions() {

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

    public virtual void SetTroopToSpawn(TroopSO troopSO) {
        troopToSpawnSO = troopSO;

        spawnIPlaceableButton.onClick.AddListener(() => {
            SpawnTroopButton();
        });
    }

    public virtual void SetBuildingToSpawn(BuildingSO buildingSO) {
        buildingToSpawnSO = buildingSO;

        spawnIPlaceableButton.onClick.AddListener(() => {
            SpawnBuildingButton();
        });
    }
}
