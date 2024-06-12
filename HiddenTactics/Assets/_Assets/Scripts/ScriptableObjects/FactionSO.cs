using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class FactionSO : ScriptableObject
{
    public Sprite factionSprite;
    public GridTileVisualSO factionDefaultGridTileVisualSO;
    public List<TroopSO> troopsInFaction;
    public List<BuildingSO> buildingsInFaction;
    public List<Sprite> villageSpritesInFaction;
    public List<Sprite> deckSlotSpritesInFaction;
}
