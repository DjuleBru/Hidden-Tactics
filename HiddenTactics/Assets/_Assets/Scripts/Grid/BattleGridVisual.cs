using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridVisual : MonoBehaviour
{

    public static BattleGridVisual Instance { get; private set; }

    [SerializeField] List<Sprite> playerGridSprites;
    [SerializeField] List<Sprite> opponentGridSprites;

    [SerializeField] List<Sprite> playerVillageSprites;
    [SerializeField] List<Sprite> opponentVillageSprites;

    [SerializeField] List<Sprite> playerSettlementSprites;
    [SerializeField] List<Sprite> opponentSettlementSprites;

    private BattleGrid battleGrid;

    private void Awake() {
        Instance = this;
        battleGrid = GetComponentInParent<BattleGrid>();
    }

    private void Start() {
        battleGrid.GetGridSystem().SetGridObjectVisualSprites(playerGridSprites, opponentGridSprites, playerSettlementSprites, opponentSettlementSprites, playerVillageSprites, opponentVillageSprites);
    }

    public Sprite GetRandomPlayerVillageSprite() {
        return playerVillageSprites[Random.Range(0, playerVillageSprites.Count)];
    }

    public Sprite GetRandomOpponentVillageSprite() {
        return opponentVillageSprites[Random.Range(0, playerVillageSprites.Count)];
    }
}
