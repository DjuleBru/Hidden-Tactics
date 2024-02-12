using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BattleDataManager : NetworkBehaviour
{
    public static BattleDataManager Instance;

    [SerializeField] private TroopListSO troopListSO;

    private void Awake() {
        Instance = this;
    }

    public TroopListSO GetTroopListSO() {
        return troopListSO;
    }

    public int GetTroopSOIndex(TroopSO troopSO) {
        return troopListSO.troopSOList.IndexOf(troopSO);
    }

    public TroopSO GetTroopSOFromIndex(int troopIndex) {
        return troopListSO.troopSOList[troopIndex];
    }

}
