using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpawnIPlaceableButton : MonoBehaviour
{
    protected TroopSO troopToSpawnSO;
    protected BuildingSO buildingToSpawnSO;

    [SerializeField] protected Button spawnIPlaceableButton;

    protected virtual void SpawnTroopButton() {
        Debug.Log("click");
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
        // Check if player can pay ! 
        bool playerCanPay = false;
        bool validGridPositionsLeft = false;

        if(troopToSpawnSO != null) {
            if(PlayerGoldManager.Instance.CanSpendGold(troopToSpawnSO.spawnTroopCost, NetworkManager.Singleton.LocalClientId)) {
                playerCanPay = true;
            } else {
                playerCanPay = false; 
            }
        }

        if(buildingToSpawnSO != null) {
            if (PlayerGoldManager.Instance.CanSpendGold(buildingToSpawnSO.spawnBuildingCost, NetworkManager.Singleton.LocalClientId)) {
                playerCanPay = true;
            }
            else {
                playerCanPay = false;
            }
        }


        // Check if there are spots remaining on the battlefield
        if(BattleGrid.Instance.ValidGridPositionLeft()) {
            validGridPositionsLeft = true;
        } else {
            validGridPositionsLeft = false;
        }

        return validGridPositionsLeft && playerCanPay;
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
