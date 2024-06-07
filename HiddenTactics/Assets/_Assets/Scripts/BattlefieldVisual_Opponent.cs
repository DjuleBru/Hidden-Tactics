using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BattlefieldVisual_Opponent : MonoBehaviour
{

    [SerializeField] private SpriteRenderer battlefieldBaseSpriteRenderer;
    [SerializeField] private SpriteRenderer battlefieldOutlineSpriteRenderer;

    private void Start() {
        PlayerCustomizationData opponentCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData();

        Sprite opponentBattlefieldBaseSprite = PlayerCustomizationDataManager.Instance.GetBattlefieldBaseSpriteFromId(opponentCustomizationData.battlefieldBaseSpriteId);
        battlefieldBaseSpriteRenderer.sprite = opponentBattlefieldBaseSprite;

        GridTileVisualSO opponentGridTileVisualSO = PlayerCustomizationDataManager.Instance.GetPlayerGridTileVisualSOFromId(opponentCustomizationData.gridVisualSOId);
        battlefieldOutlineSpriteRenderer.sprite = opponentGridTileVisualSO.battlefieldOutlineSprite;
    }

}
