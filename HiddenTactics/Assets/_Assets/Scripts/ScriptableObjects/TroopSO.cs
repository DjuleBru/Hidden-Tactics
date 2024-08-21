using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TroopSO : ScriptableObject {

    public FactionSO troopFactionSO;
    public string troopName;
    public string troopDescription;
    public Sprite troopIllustrationSlotSprite;
    public Sprite troopDescriptionSlotSprite;
    public Sprite troopTypeIconSprite;

    public GameObject troopPrefab;
    public GameObject unitPrefab;
    public GameObject additionalUnit1Prefab;
    public TroopSO additionalUnit1TroopSO;
    public GameObject additionalUnit2Prefab;
    public TroopSO additionalUnit2TroopSO;
    public GameObject spawnedUnitPrefab;

    public int spawnTroopCost;
    public int upgradeTroopCost;
    public int buyAdditionalUnitsCost;

    public List<Vector2> buffedGridPositions;
    public SupportUnit.SupportType supportType;
    public float buffAmount;

    public bool isGarrisonedTroop;
    public bool troopIsImplemented;

}
