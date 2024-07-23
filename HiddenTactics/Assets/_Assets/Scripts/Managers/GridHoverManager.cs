using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridHoverManager : MonoBehaviour
{
    public static GridHoverManager Instance;

    private GridPosition currentHoveredGridPosition;
    private GridPosition previousHoveredGridPosition;
    private GridPosition previousValidHoveredGridPosition;

    private List<GridObjectVisual> targetTilesGridObjectVisuals = new List<GridObjectVisual>();
    private List<Unit> unitSetAsTargetList = new List<Unit>();
    private List<Unit> supportUnits = new List<Unit>();

    private void Awake() {
        Instance = this;
    }

    public void SubsribeToPlayerActions() {
        Debug.Log("suubbed to player actions");
        PlayerActionsManager.LocalInstance.OnActionChanged += PlayerActionsManager_OnActionChanged;
    }

    private void PlayerActionsManager_OnActionChanged(object sender, System.EventArgs e) {
        if(PlayerActionsManager.LocalInstance.GetCurrentAction() != PlayerActionsManager.Action.SelectingIPlaceableToSpawn) {

            if (targetTilesGridObjectVisuals.Count > 0) {
                foreach (GridObjectVisual gridObjectVisual in targetTilesGridObjectVisuals) {
                    gridObjectVisual.ResetVisual();
                }
                targetTilesGridObjectVisuals.Clear();
            }

            foreach (Unit unit in supportUnits) {
                unit.GetComponent<SupportUnit>().HideBuffedUnitBuffs();
            }

        }

        if(PlayerActionsManager.LocalInstance.GetCurrentAction() == PlayerActionsManager.Action.SelectingIPlaceableToSpawn) {
            foreach(Unit unit in supportUnits) {
                ShowPlacedSupportTroopBuffedTiles(unit);
                unit.GetComponent<SupportUnit>().ShowBuffedUnitBuffs();
            }
        }
    }

    private void Update() {
        if (!BattleManager.Instance.IsPreparationPhase()) return;
        // Only allow grid hovering on preparation phase

        if (!BattleGrid.Instance.IsValidGridPosition(MousePositionManager.Instance.GetMouseGridPosition())) {
            // Player is not hovering a valid gridposition
            if (currentHoveredGridPosition != null) {
                GridObjectVisual currentHoveredGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(currentHoveredGridPosition);
                currentHoveredGridObjectVisual.ResetVisual();
            }
            return;
        }

        if (MousePositionManager.Instance.IsPointerOverUIElement()) {
            // Player is hovering UI
            if (currentHoveredGridPosition != null) {
                GridObjectVisual currentHoveredGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(currentHoveredGridPosition);
                currentHoveredGridObjectVisual.ResetVisual();
            }
            return;
        }


        HandleHoveredGridPositionChange();
    }

    private void HandleHoveredGridPositionChange() {
        currentHoveredGridPosition = MousePositionManager.Instance.GetMouseGridPosition();

        if(previousHoveredGridPosition == null) {
            previousHoveredGridPosition = currentHoveredGridPosition;
        }

        if (previousValidHoveredGridPosition == null) {
            previousValidHoveredGridPosition = currentHoveredGridPosition;
        }

        if (currentHoveredGridPosition != previousHoveredGridPosition) {
            HoveredGridPositionChanged();
            previousHoveredGridPosition = currentHoveredGridPosition;

            if(PlayerAction_SpawnTroop.LocalInstance.IsValidIPlaceableSpawningTarget()) {
                previousValidHoveredGridPosition = currentHoveredGridPosition;
            }

        }
    }

    private void HoveredGridPositionChanged() {

        HandleGridObjectHover();
        CancelPreviousUnitsSetAsTarget();

        if(PlayerActionsManager.LocalInstance.GetCurrentAction() == PlayerActionsManager.Action.SelectingIPlaceableToSpawn) {
            TroopSO troopSOBeingPlaced =  BattleDataManager.Instance.GetTroopSOFromIndex(PlayerActionsManager.LocalInstance.GetTroopSOIndexBeingSpawned());
            ShowTroopToPlaceRangedAttackTiles(troopSOBeingPlaced);

            foreach (Unit unit in supportUnits) {
                ShowPlacedSupportTroopBuffedTiles(unit);
            }

            return;
        }

        // Do not show hovered units if not idle
        if (PlayerActionsManager.LocalInstance.GetCurrentAction() != PlayerActionsManager.Action.Idle) return;

        HandleUnitHover();
        ShowHoveredTroopRangedAttackTiles();
    }

    private void HandleGridObjectHover() {


        if (PlayerActionsManager.LocalInstance.GetCurrentAction() == PlayerActionsManager.Action.SelectingIPlaceableToSpawn) {
            //Player is placing troop : hover new grid position only if it is valid

            if (!PlayerAction_SpawnTroop.LocalInstance.IsValidIPlaceableSpawningTarget()) return;

            GridObjectVisual previousValidGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(previousValidHoveredGridPosition);
            previousValidGridObjectVisual.ResetVisual();

            GridObjectVisual newGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(currentHoveredGridPosition);
            newGridObjectVisual.SetHovered();

        } else {

            //Player is not placing troop

            GridObjectVisual previousGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(previousHoveredGridPosition);
            previousGridObjectVisual.ResetVisual();

            GridObjectVisual newGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(currentHoveredGridPosition);
            newGridObjectVisual.SetHovered();
        }

       
    }

    private void HandleUnitHover() {
        List<Unit> previousHoveredUnitList = BattleGrid.Instance.GetUnitListAtGridPosition(previousHoveredGridPosition);

        if (previousHoveredUnitList.Count > 0) {
            foreach (Unit unit in previousHoveredUnitList) {
                // Do not change hovered visual on selected troop
                if (PlayerAction_SelectTroop.LocalInstance.IsTroopSelected(unit.GetParentTroop())) continue;
                
                unit.GetUnitVisual().SetUnitHovered(false);

                if(unit.GetComponent<SupportUnit>() != null) {
                    unit.GetComponent<SupportUnit>().HideBuffedUnitBuffs();
                }

            }
        }

        List<Unit> newHoveredUnitList = BattleGrid.Instance.GetUnitListAtGridPosition(currentHoveredGridPosition);
        if (newHoveredUnitList.Count > 0) {
            foreach (Unit unit in newHoveredUnitList) {
                // Do not change hovered visual on selected troop
                if (PlayerAction_SelectTroop.LocalInstance.IsTroopSelected(unit.GetParentTroop())) continue;
                unit.GetUnitVisual().SetUnitHovered(true);

                if (unit.GetComponent<SupportUnit>() != null) {
                    unit.GetComponent<SupportUnit>().ShowBuffedUnitBuffs();
                }
            }
        }
    }

    private void ShowHoveredTroopRangedAttackTiles() {
        Troop newHoveredTroop = BattleGrid.Instance.GetTroopAtGridPosition(currentHoveredGridPosition);

        if(targetTilesGridObjectVisuals.Count > 0) {
            foreach(GridObjectVisual gridObjectVisual in targetTilesGridObjectVisuals) {
                gridObjectVisual.ResetVisual();
            }
            targetTilesGridObjectVisuals.Clear();
        }

        if (newHoveredTroop != null) {
            AttackSO troopMainAttackSO = newHoveredTroop.GetTroopSO().unitPrefab.GetComponent<Unit>().GetUnitSO().mainAttackSO;
            AttackSO troopSideAttackSO = newHoveredTroop.GetTroopSO().unitPrefab.GetComponent<Unit>().GetUnitSO().sideAttackSO;
            SupportUnit.SupportType supportType = newHoveredTroop.GetTroopSO().supportType;

            if (troopMainAttackSO != null) {
                if (troopMainAttackSO.attackType == AttackSO.AttackType.ranged) {
                    ShowRangedAttackSOAttackTiles(troopMainAttackSO, newHoveredTroop.IsOwnedByPlayer());
                }
            }

            if (troopSideAttackSO != null) {
                if(troopSideAttackSO.attackType == AttackSO.AttackType.ranged) {
                    ShowRangedAttackSOAttackTiles(troopSideAttackSO, newHoveredTroop.IsOwnedByPlayer());
                }
            }

            if (supportType != SupportUnit.SupportType.none) {
                ShowPlacingSupportTroopBuffedTiles(newHoveredTroop.GetTroopSO());
            }
        }
    }

    private void ShowTroopToPlaceRangedAttackTiles(TroopSO troopSO) {
        if (!PlayerAction_SpawnTroop.LocalInstance.IsValidIPlaceableSpawningTarget()) return;

        if (targetTilesGridObjectVisuals.Count > 0) {
            foreach (GridObjectVisual gridObjectVisual in targetTilesGridObjectVisuals) {
                gridObjectVisual.ResetVisual();
            }
            targetTilesGridObjectVisuals.Clear();
        }

        AttackSO troopMainAttackSO = troopSO.unitPrefab.GetComponent<Unit>().GetUnitSO().mainAttackSO;
        AttackSO troopSideAttackSO = troopSO.unitPrefab.GetComponent<Unit>().GetUnitSO().sideAttackSO;
        SupportUnit.SupportType supportType = troopSO.supportType;

        if (troopMainAttackSO != null) {
            if (troopMainAttackSO.attackType == AttackSO.AttackType.ranged) {
                ShowRangedAttackSOAttackTiles(troopMainAttackSO, true);
            }
        }

        if (troopSideAttackSO != null) {
            if (troopSideAttackSO.attackType == AttackSO.AttackType.ranged) {
                ShowRangedAttackSOAttackTiles(troopSideAttackSO, true);
            }
        }

        if(supportType != SupportUnit.SupportType.none) {
            ShowPlacingSupportTroopBuffedTiles(troopSO);
        }
    }

    private void ShowRangedAttackSOAttackTiles(AttackSO attackSO, bool isPlayerTroop) {
        // Only show this if it is a ranged attack

        foreach (Vector2 gridPositionVector in attackSO.attackTargetTiles) {
            GridPosition rangedAttackGridPosition = new GridPosition((int)gridPositionVector.x, (int)gridPositionVector.y);

            if(!isPlayerTroop) {
                rangedAttackGridPosition.x = -rangedAttackGridPosition.x;
            }

            rangedAttackGridPosition = rangedAttackGridPosition + currentHoveredGridPosition;

            if (rangedAttackGridPosition == currentHoveredGridPosition) continue;
            if (!BattleGrid.Instance.IsValidGridPosition(rangedAttackGridPosition)) continue;

            SetGridObjectVisualAsAttackTarget(rangedAttackGridPosition);
            HandleSettingUnitsAsRangedAttackTarget(rangedAttackGridPosition);
        }
    }

    private void ShowPlacingSupportTroopBuffedTiles(TroopSO troopSO) {
        foreach (Vector2 gridPositionVector in troopSO.buffedGridPositions) {
            GridPosition supportGridPosition = new GridPosition((int)gridPositionVector.x, (int)gridPositionVector.y);

            supportGridPosition = supportGridPosition + currentHoveredGridPosition;

            if (!BattleGrid.Instance.IsValidGridPosition(supportGridPosition)) continue;
            if (!BattleGrid.Instance.IsValidPlayerGridPosition(supportGridPosition)) continue;

            SetGridObjectVisualAsBuffTarget(supportGridPosition, troopSO.supportType);
        }
    }

    private void ShowPlacedSupportTroopBuffedTiles(Unit unit) {
        Troop troop = unit.GetParentTroop();

        foreach (Vector2 gridPositionVector in troop.GetTroopSO().buffedGridPositions) {
            GridPosition supportGridPosition = new GridPosition((int)gridPositionVector.x, (int)gridPositionVector.y);

            supportGridPosition = supportGridPosition + troop.GetIPlaceableGridPosition();

            if (!BattleGrid.Instance.IsValidGridPosition(supportGridPosition)) continue;
            if (!BattleGrid.Instance.IsValidPlayerGridPosition(supportGridPosition)) continue;

            SetGridObjectVisualAsBuffTarget(supportGridPosition, troop.GetTroopSO().supportType);
        }
    }

    private void SetGridObjectVisualAsAttackTarget(GridPosition rangedAttackGridPosition) {
        GridObjectVisual targetGridPositionGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(rangedAttackGridPosition);
        targetTilesGridObjectVisuals.Add(targetGridPositionGridObjectVisual);

        targetGridPositionGridObjectVisual.SetAsAttackTargetTile();
    }

    private void SetGridObjectVisualAsBuffTarget(GridPosition gridPosition, SupportUnit.SupportType supportType) {
        GridObjectVisual targetGridPositionGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(gridPosition);
        targetTilesGridObjectVisuals.Add(targetGridPositionGridObjectVisual);

        if(supportType == SupportUnit.SupportType.attackSpeed) {
            targetGridPositionGridObjectVisual.SetAsAttackSpeedBuffTile();
        }
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
                if(unit != null) {
                    unit.GetUnitUI().HideUnitTargetUI();
                }
            }
        }
        unitSetAsTargetList.Clear();
    }

    public void AddSupportUnit(Unit unit) {
        supportUnits.Add(unit);
    }

    public void RemoveSupportUnit(Unit unit) {
        supportUnits.Remove(unit);
    }
}
