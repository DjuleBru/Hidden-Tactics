using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHoverManager : MonoBehaviour
{
    private GridPosition currentHoveredGridPosition;
    private GridPosition previousHoveredGridPosition;

    private List<GridObjectVisual> attackTargetTilesGridObjectVisuals = new List<GridObjectVisual>();
    private List<Unit> unitSetAsTargetList = new List<Unit>();

    private void Update() {
        if (!BattleManager.Instance.IsPreparationPhase()) return;
        // Only allow grid hovering on preparation phase

        if (!BattleGrid.Instance.IsValidGridPosition(MousePositionManager.Instance.GetMouseGridPosition())) return;
        // Player is not hovering a valid gridposition

        HandleHoveredGridPositionChange();
    }

    private void HandleHoveredGridPositionChange() {
        currentHoveredGridPosition = MousePositionManager.Instance.GetMouseGridPosition();

        if(previousHoveredGridPosition == null) {
            previousHoveredGridPosition = currentHoveredGridPosition;
        }

        if(currentHoveredGridPosition != previousHoveredGridPosition) {
            HoveredGridPositionChanged();
            previousHoveredGridPosition = currentHoveredGridPosition;
        }
    }

    private void HoveredGridPositionChanged() {
        HandleGridObjectHover();
        CancelPreviousUnitsSetAsTarget();

        // Do not show hovered units if not idle
        if (PlayerActionsManager.LocalInstance.GetCurrentAction() != PlayerActionsManager.Action.Idle) return;

        HandleUnitHover();
        ShowTroopRangedAttackTiles();
    }

    private void HandleGridObjectHover() {
        GridObjectVisual previousGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(previousHoveredGridPosition);
        previousGridObjectVisual.ResetVisual();

        GridObjectVisual newGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(currentHoveredGridPosition);
        newGridObjectVisual.SetHovered();
    }

    private void HandleUnitHover() {
        List<Unit> previousHoveredUnitList = BattleGrid.Instance.GetUnitListAtGridPosition(previousHoveredGridPosition);

        if(previousHoveredUnitList.Count > 0) {
            foreach(Unit unit in previousHoveredUnitList) {
                unit.GetUnitVisual().SetUnitHovered(false);
            }
        }

        List<Unit> newHoveredUnitList = BattleGrid.Instance.GetUnitListAtGridPosition(currentHoveredGridPosition);
        if (newHoveredUnitList.Count > 0) {
            foreach (Unit unit in newHoveredUnitList) {
                unit.GetUnitVisual().SetUnitHovered(true);
            }
        }
    }

    private void ShowTroopRangedAttackTiles() {
        Troop newHoveredTroop = BattleGrid.Instance.GetTroopAtGridPosition(currentHoveredGridPosition);

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
                rangedAttackGridPosition = rangedAttackGridPosition + currentHoveredGridPosition;

                SetGridObjectVisualAsAttackTarget(rangedAttackGridPosition);
                HandleSettingUnitsAsRangedAttackTarget(rangedAttackGridPosition);
            }
        }
    }

    private void SetGridObjectVisualAsAttackTarget(GridPosition rangedAttackGridPosition) {
        GridObjectVisual targetGridPositionGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(rangedAttackGridPosition);
        attackTargetTilesGridObjectVisuals.Add(targetGridPositionGridObjectVisual);

        targetGridPositionGridObjectVisual.SetAsAttackTargetTile();
    }

    private void HandleSettingUnitsAsRangedAttackTarget(GridPosition rangedAttackGridPosition) {
        List<Unit> unitsAtRangedAttackGridPosition = BattleGrid.Instance.GetUnitListAtGridPosition(rangedAttackGridPosition);

        if (unitsAtRangedAttackGridPosition.Count > 0) {
            foreach (Unit unit in unitsAtRangedAttackGridPosition) {
                unit.GetUnitUI().ShowUnitUIAsRangedTarget();
                unitSetAsTargetList.Add(unit);
            }
        }
    }

    private void CancelPreviousUnitsSetAsTarget() {
        if (unitSetAsTargetList.Count > 0) {
            foreach (Unit unit in unitSetAsTargetList) {
                unit.GetUnitUI().HideUnitTargetUI();
            }
        }
        unitSetAsTargetList.Clear();
    }
}
