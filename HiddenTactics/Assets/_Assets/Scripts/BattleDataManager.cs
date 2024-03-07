using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BattleDataManager : NetworkBehaviour
{
    public static BattleDataManager Instance;

    [SerializeField] private IPlaceableListSO IPlaceableListSO;

    private void Awake() {
        Instance = this;
    }

    public int GetTroopSOIndex(TroopSO troopSO) {
        return IPlaceableListSO.troopSOList.IndexOf(troopSO);
    }

    public TroopSO GetTroopSOFromIndex(int troopIndex) {
        return IPlaceableListSO.troopSOList[troopIndex];
    }

    public int GetBuildingSOIndex(BuildingSO buildingSO) {
        return IPlaceableListSO.buildingSOList.IndexOf(buildingSO);
    }

    public BuildingSO GetBuildingSOFromIndex(int buildingSOIndex) {
        return IPlaceableListSO.buildingSOList[buildingSOIndex];
    }

}
