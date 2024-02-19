using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridVisual : MonoBehaviour
{

    [SerializeField] List<Sprite> playerGridSprites;
    [SerializeField] List<Sprite> opponentGridSprites;

    [SerializeField] List<Sprite> playerVillageSprites;
    [SerializeField] List<Sprite> opponentVillageSprites;

    [SerializeField] List<Sprite> playerSettlementSprites;
    [SerializeField] List<Sprite> opponentSettlementSprites;

    private BattleGrid battleGrid;

    private void Awake() {
        battleGrid = GetComponentInParent<BattleGrid>();
    }

    private void Start() {
        battleGrid.GetGridSystem().SetGridObjectVisualSprites(playerGridSprites, opponentGridSprites, playerSettlementSprites, opponentSettlementSprites, playerVillageSprites, opponentVillageSprites);
    }
}
