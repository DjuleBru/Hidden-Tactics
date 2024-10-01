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
        List<TroopSO> implementedLevel1MercenaryList = new List<TroopSO>();
        foreach(TroopSO troopSO in level1MercenaryTroopSOList) {
            if(troopSO.troopIsImplemented) {
                implementedLevel1MercenaryList.Add(troopSO);
            }
        }
        return implementedLevel1MercenaryList;
    }

    public List<TroopSO> GetLevel2MercenaryTroopSOList() {
        List<TroopSO> implementedLevel2MercenaryList = new List<TroopSO>();
        foreach (TroopSO troopSO in level2MercenaryTroopSOList) {
            if (troopSO.troopIsImplemented) {
                implementedLevel2MercenaryList.Add(troopSO);
            }
        }
        return implementedLevel2MercenaryList;
    }

    public List<TroopSO> GetLevel3MercenaryTroopSOList() {
        List<TroopSO> implementedLevel3MercenaryList = new List<TroopSO>();
        foreach (TroopSO troopSO in level3MercenaryTroopSOList) {
            if (troopSO.troopIsImplemented) {
                implementedLevel3MercenaryList.Add(troopSO);
            }
        }
        return implementedLevel3MercenaryList;
    }

    public List<TroopSO> GetLevel4MercenaryTroopSOList() {
        List<TroopSO> implementedLevel4MercenaryList = new List<TroopSO>();
        foreach (TroopSO troopSO in level4MercenaryTroopSOList) {
            if (troopSO.troopIsImplemented) {
                implementedLevel4MercenaryList.Add(troopSO);
            }
        }
        return implementedLevel4MercenaryList;
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
