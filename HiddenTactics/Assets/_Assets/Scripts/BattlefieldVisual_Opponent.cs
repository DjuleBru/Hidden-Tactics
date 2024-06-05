using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BattlefieldVisual_Opponent : MonoBehaviour
{

    [SerializeField] private SpriteRenderer battlefieldBaseSpriteRenderer;
    [SerializeField] private SpriteRenderer battlefieldOutlineSpriteRenderer;

    private void Start() {
        PlayerData opponentData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentData();

        Sprite opponentBattlefieldBaseSprite = PlayerCustomizationData.Instance.GetBattlefieldBaseSpriteFromId(opponentData.battlefieldBaseSpriteId);
        battlefieldBaseSpriteRenderer.sprite = opponentBattlefieldBaseSprite;

        GridTileVisualSO opponentGridTileVisualSO = PlayerCustomizationData.Instance.GetPlayerGridTileVisualSOFromId(opponentData.gridVisualSOId);
        battlefieldOutlineSpriteRenderer.sprite = opponentGridTileVisualSO.battlefieldOutlineSprite;
    }

}
