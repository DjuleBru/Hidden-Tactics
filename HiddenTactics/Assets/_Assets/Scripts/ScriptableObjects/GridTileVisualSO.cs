using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GridTileVisualSO : ScriptableObject
{
    public FactionSO gridFactionSO;
    public List<Sprite> gridSpriteList;
    public Sprite battlefieldOutlineSprite;
    public List<Sprite> settlementSpriteList;
}
