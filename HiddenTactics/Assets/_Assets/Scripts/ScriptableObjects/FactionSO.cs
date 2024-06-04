using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class FactionSO : ScriptableObject
{
    public Sprite factionSprite;
    public List<TroopSO> troopsInFaction;
    public List<BuildingSO> buildingsInFaction;
}
