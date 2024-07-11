using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class FactionSO : ScriptableObject
{
    public enum FactionName {
        Humans,
        Elves,
        Dwarves,
        Greenskins,
    }

    public FactionName factionName;

    public Sprite factionSprite;
    public GridTileVisualSO factionDefaultGridTileVisualSO;
    public Sprite panelBackground;
    public Sprite panelBackgroundBorder;
    public Sprite panelBackgroundInnerShadow;
    public Sprite panelBorder;
    public Sprite panelTopItem;
    public Sprite panelBottomItem;
    public Sprite panelLeftItem;
    public Sprite panelRightItem;
    public Sprite slotBackground;
    public Sprite slotBorder;
    public List<TroopSO> troopsInFaction;
    public List<BuildingSO> buildingsInFaction;
    public List<Sprite> villageSpritesInFaction;
    public List<Sprite> deckSlotSpritesInFaction;
}
