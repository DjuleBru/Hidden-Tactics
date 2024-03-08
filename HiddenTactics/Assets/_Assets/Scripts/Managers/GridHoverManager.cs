using Sirenix.OdinInspector.Editor.Validation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHoverManager : MonoBehaviour
{
    private GridPosition currentGridPosition;
    private GridPosition previousGridPosition;

    private List<GridObjectVisual> attackTargetTilesGridObjectVisuals = new List<GridObjectVisual>();

    private void Update() {
        if (!BattleGrid.Instance.IsValidGridPosition(MousePositionManager.Instance.GetMouseGridPosition())) return;

        HandleHoveredGridPositionChange();
    }

    private void HandleHoveredGridPositionChange() {
        currentGridPosition = MousePositionManager.Instance.GetMouseGridPosition();

        if(previousGridPosition == null) {
            previousGridPosition = currentGridPosition;
        }

        if(currentGridPosition != previousGridPosition) {
            HoveredGridPositionChanged();
            previousGridPosition = currentGridPosition;
        }
    }

    private void HoveredGridPositionChanged() {
        HandleGridObjectHover();

        // Do not show hovered units if not idle
        if (PlayerActionsManager.LocalInstance.GetCurrentAction() != PlayerActionsManager.Action.Idle) return;
        HandleUnitHover();
        ShowRangeAttackTroopTiles();
    }

    private void HandleGridObjectHover() {
        GridObjectVisual previousGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(previousGridPosition);
        previousGridObjectVisual.ResetVisual();

        GridObjectVisual newGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(currentGridPosition);
        newGridObjectVisual.SetSelected();
    }

    private void HandleUnitHover() {
        List<Unit> previousHoveredUnitList = BattleGrid.Instance.GetUnitListAtGridPosition(previousGridPosition);

        if(previousHoveredUnitList.Count > 0) {
            foreach(Unit unit in previousHoveredUnitList) {
                unit.GetUnitVisual().SetUnitHovered(false);
            }
        }

        List<Unit> newHoveredUnitList = BattleGrid.Instance.GetUnitListAtGridPosition(currentGridPosition);
        if (newHoveredUnitList.Count > 0) {
            foreach (Unit unit in newHoveredUnitList) {
                unit.GetUnitVisual().SetUnitHovered(true);
            }
        }
    }

    private void ShowRangeAttackTroopTiles() {
        Troop newHoveredTroop = BattleGrid.Instance.GetTroopAtGridPosition(currentGridPosition);

        if(attackTargetTilesGridObjectVisuals.Count > 0) {
            foreach(GridObjectVisual gridObjectVisual in attackTargetTilesGridObjectVisuals) {
                gridObjectVisual.ResetVisual();
            }
            attackTargetTilesGridObjectVisuals.Clear();
        }

        if (newHoveredTroop != null) {
            AttackSO mainTroopAttackSO = newHoveredTroop.GetTroopSO().mainTroopAttackSO;

            if (mainTroopAttackSO.attackType != AttackSO.AttackType.ranged) return;
            // Only show this if it is a ranged attack

            foreach(Vector2 gridPositionVector in mainTroopAttackSO.attackTargetTiles) {

                GridPosition rangedAttackGridPosition = new GridPosition((int)gridPositionVector.x, (int)gridPositionVector.y);
                rangedAttackGridPosition = rangedAttackGridPosition + currentGridPosition;

                GridObjectVisual targetGridPositionGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(rangedAttackGridPosition);
                attackTargetTilesGridObjectVisuals.Add(targetGridPositionGridObjectVisual);

                targetGridPositionGridObjectVisual.SetAsAttackTargetTile();
            }
        }


    }
}
