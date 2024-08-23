using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BuildingSO : ScriptableObject
{
    public string buildingName;
    public string buildingDescription;

    public Sprite buildingRecruitmentSlotSprite;
    public Sprite buildingDescriptionSlotSprite;
    public Sprite buildingTypeSprite;

    public GameObject buildingPrefab;
    public float buildingHP;
    public bool buildingBlocksUnitMovement;
    public bool hasGarrisonedTroop;
    public TroopSO garrisonedTroopSO;

    public int spawnBuildingCost;
    public int upgradeBuildingCost;

    public int economicalBuildingRevenue;
    public int reflectMeleeDamageAmount;

    public List<UnitSO.UnitKeyword> buildingKeyworkdList;

    public bool buildingIsImplemented;

    public FactionSO buildingFactionSO;
}
