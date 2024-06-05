using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BattlefieldVisual_Opponent : MonoBehaviour
{

    [SerializeField] private SpriteRenderer battlefieldOutlineSpriteRenderer;
    [SerializeField] private SpriteRenderer battlefieldBaseSpriteRenderer;
    [SerializeField] private Sprite battlefieldBaseDefaultSprite;

    private void Start() {
        PlayerData opponentData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData();


    }

}
