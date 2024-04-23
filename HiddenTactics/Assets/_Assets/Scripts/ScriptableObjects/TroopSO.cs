using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TroopSO : ScriptableObject {
    public GameObject troopPrefab;
    public GameObject unitPrefab;

    public AttackSO mainTroopAttackSO;

    public int spawnTroopCost;
    public int upgradeTroopCost;
    public int buyAdditionalUnitsCost;

}
