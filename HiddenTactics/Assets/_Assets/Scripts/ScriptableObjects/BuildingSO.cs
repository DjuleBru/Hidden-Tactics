using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BuildingSO : ScriptableObject
{
    public GameObject buildingPrefab;
    public float buildingHP;
    public bool buildingBlocksUnitMovement;
    public bool hasGarrisonedUnits;
}
