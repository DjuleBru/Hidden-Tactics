using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class IPlaceableListSO : ScriptableObject
{
   public List<TroopSO> troopSOList;
   public List<BuildingSO> buildingSOList;
}
