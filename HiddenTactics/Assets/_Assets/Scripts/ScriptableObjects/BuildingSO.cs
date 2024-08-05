using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BuildingSO : ScriptableObject
{
    public string buildingName;

    public Sprite buildingIllustrationSlotSprite;
    public Sprite buildingDescriptionSlotSprite;
    public Sprite buildingTypeSprite;

    public GameObject buildingPrefab;
    public float buildingHP;
    public bool buildingBlocksUnitMovement;
    public bool hasGarrisonedTroop;
    public TroopSO garrisonedTroopSO;


    public int spawnBuildingCost;
    public int upgradeBuildingCost;

    public bool buildingIsImplemented;
}
