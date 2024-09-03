using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BattlefieldVisual_Opponent : MonoBehaviour
{

    [SerializeField] private SpriteRenderer battlefieldBaseSpriteRenderer;
    [SerializeField] private SpriteRenderer battlefieldOutlineSpriteRenderer;
    [SerializeField] private SpriteRenderer battlefieldShadowSpriteRenderer;

    private void Start() {
        PlayerCustomizationData opponentCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData();

        Sprite opponentBattlefieldBaseSprite = PlayerCustomizationDataManager.Instance.GetBattlefieldBaseSOFromId(opponentCustomizationData.battlefieldBaseSOId).battlefieldBaseSprite;
        battlefieldBaseSpriteRenderer.sprite = opponentBattlefieldBaseSprite;

        GridTileVisualSO opponentGridTileVisualSO = PlayerCustomizationDataManager.Instance.GetPlayerGridTileVisualSOFromId(opponentCustomizationData.gridVisualSOId);
        battlefieldOutlineSpriteRenderer.sprite = opponentGridTileVisualSO.battlefieldOutlineSprite;

        battlefieldBaseSpriteRenderer.enabled = false;
        battlefieldOutlineSpriteRenderer.enabled = false;
        battlefieldShadowSpriteRenderer.enabled = false;

        BattleManager.Instance.OnAllPlayersLoaded += BattleManager_OnAllPlayersLoaded;
    }

    private void BattleManager_OnAllPlayersLoaded(object sender, System.EventArgs e) {
        battlefieldBaseSpriteRenderer.enabled = true;
        battlefieldOutlineSpriteRenderer.enabled = true;
        battlefieldShadowSpriteRenderer.enabled = true;
    }
}
