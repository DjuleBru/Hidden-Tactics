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
        Mercenaries,
    }

    public FactionName factionName;

    public Sprite factionSprite;
    public GridTileVisualSO factionDefaultGridTileVisualSO;
    public Sprite panelBackground;
    public Sprite panelBackgroundBorder;
    public Sprite panelBackgroundBorderSimple;
    public Sprite panelBackgroundInnerShadow;
    public Sprite panelBorder;
    public Sprite panelTopItem;
    public Sprite panelBottomItem;
    public Sprite panelLeftItem;
    public Sprite panelRightItem;
    public Sprite slotBackground;
    public Sprite slotBackgroundSquare;
    public Sprite slotBorder;

    public Sprite troopIconBackgroundSprite_differentPlayerFaction;
    public Sprite troopIconBackgroundSprite_samePlayerFaction_Opponent;
    public Color color_differentPlayerFaction_outline;
    public Color color_differentPlayerFaction_fill;
    public Color color_samePlayerFaction_Opponent_outline;
    public Color color_samePlayerFaction_Opponent_fill;

    public List<TroopSO> troopsInFaction;
    public List<BuildingSO> buildingsInFaction;
    public List<Sprite> villageSpritesInFaction;
    public List<Sprite> deckSlotSpritesInFaction;
}
