using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu()]
public class WeaponSO : ScriptableObject
{
    public Sprite idleSprite;
    public Sprite fireSprite;
    public Sprite iceSprite;
    public Sprite fearSprite;
    public Sprite bleedSprite;
    public Sprite poisonSprite;
    public Sprite shockSprite;

    public AnimatorOverrideController weaponAnimator;

}
