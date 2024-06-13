using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BattleDataManager : NetworkBehaviour
{
    public static BattleDataManager Instance;

    [SerializeField] private IPlaceableListSO IPlaceableListSO;


    [SerializeField] private List<TroopSO> level1MercenaryTroopSOList;
    [SerializeField] private List<TroopSO> level2MercenaryTroopSOList;
    [SerializeField] private List<TroopSO> level3MercenaryTroopSOList;
    [SerializeField] private List<TroopSO> level4MercenaryTroopSOList;

    private void Awake() {
        Instance = this;
    }

    public List<TroopSO> GetLevel1MercenaryTroopSOList() {
        return level1MercenaryTroopSOList;
    }

    public List<TroopSO> GetLevel2MercenaryTroopSOList() {
        return level2MercenaryTroopSOList;
    }

    public List<TroopSO> GetLevel3MercenaryTroopSOList() {
        return level3MercenaryTroopSOList;
    }

    public List<TroopSO> GetLevel4MercenaryTroopSOList() {
        return level4MercenaryTroopSOList;
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
