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

            if(PlayerAction_SpawnIPlaceable.LocalInstance.IsValidIPlaceableSpawningTarget(currentHoveredGridPosition)) {
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

        HandleTroopHover();
        HandleBuildingHover();
        ShowHoveredTroopRangedAttackTiles();
    }

    private void HandleGridObjectHover() {

        if (PlayerActionsManager.LocalInstance.GetCurrentAction() == PlayerActionsManager.Action.SelectingIPlaceableToSpawn) {
            //Player is placing troop : hover new grid position only if it is valid

            if (!PlayerAction_SpawnIPlaceable.LocalInstance.IsMousePositionValidIPlaceableSpawningTarget()) return;

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

    private void HandleTroopHover() {
        Troop previousTroopHovered = BattleGrid.Instance.GetTroopAtGridPosition(previousHoveredGridPosition);

        if (!PlayerAction_SelectIPlaceable.LocalInstance.IsTroopSelected(previousTroopHovered)) {
            // Previous troop was not selected : unhover
            if (previousTroopHovered != null) {
                previousTroopHovered.SetTroopHovered(false);
            }

        }

        Troop newTroopHovered = BattleGrid.Instance.GetTroopAtGridPosition(currentHoveredGridPosition);
        if (!PlayerAction_SelectIPlaceable.LocalInstance.IsTroopSelected(newTroopHovered)) {
            // New troop was not selected : hover

            if (newTroopHovered != null) {
                newTroopHovered.SetTroopHovered(true);
            }
        }
    }

    private void HandleBuildingHover() {
        Building previousBuildingHovered = BattleGrid.Instance.GetBuildingAtGridPosition(previousHoveredGridPosition);

        if (!PlayerAction_SelectIPlaceable.LocalInstance.IsBuildingSelected(previousBuildingHovered)) {
            // Previous building was not selected : unhover
            if (previousBuildingHovered != null) {
                previousBuildingHovered.SetBuildingHovered(false);
            }

        }

        Building newBuildingHovered = BattleGrid.Instance.GetBuildingAtGridPosition(currentHoveredGridPosition);
        if (!PlayerAction_SelectIPlaceable.LocalInstance.IsBuildingSelected(newBuildingHovered)) {
            // New building was not selected : hover

            if (newBuildingHovered != null) {
                newBuildingHovered.SetBuildingHovered(true);
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
                ShowRangedAttackSOAttackTiles(troopMainAttackSO, troopMainAttackSO.attackType, newHoveredTroop.IsOwnedByPlayer());
            }

            if (troopSideAttackSO != null) {
                ShowRangedAttackSOAttackTiles(troopSideAttackSO, troopSideAttackSO.attackType, newHoveredTroop.IsOwnedByPlayer());
            }

            if (supportType != SupportUnit.SupportType.none) {
                ShowPlacingSupportTroopBuffedTiles(newHoveredTroop.GetTroopSO());
            }
        }
    }

    private void ShowTroopToPlaceRangedAttackTiles(TroopSO troopSO) {
        if (!PlayerAction_SpawnIPlaceable.LocalInstance.IsValidIPlaceableSpawningTarget(currentHoveredGridPosition)) return;

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
            ShowRangedAttackSOAttackTiles(troopMainAttackSO, troopMainAttackSO.attackType, true);
        }

        if (troopSideAttackSO != null) {
            ShowRangedAttackSOAttackTiles(troopSideAttackSO, troopSideAttackSO.attackType, true);
        }

        if(supportType != SupportUnit.SupportType.none) {
            ShowPlacingSupportTroopBuffedTiles(troopSO);
        }
    }

    private void ShowRangedAttackSOAttackTiles(AttackSO attackSO, AttackSO.AttackType attackType, bool isPlayerTroop) {
        // Only show this if it is a ranged attack

        foreach (Vector2 gridPositionVector in attackSO.attackTargetTiles) {
            GridPosition rangedAttackGridPosition = new GridPosition((int)gridPositionVector.x, (int)gridPositionVector.y);

            if(!isPlayerTroop) {
                rangedAttackGridPosition.x = -rangedAttackGridPosition.x;
            }

            rangedAttackGridPosition = rangedAttackGridPosition + currentHoveredGridPosition;

            if (rangedAttackGridPosition == currentHoveredGridPosition) continue;
            if (!BattleGrid.Instance.IsValidGridPosition(rangedAttackGridPosition)) continue;

            if (attackType == AttackSO.AttackType.ranged) {
                SetGridObjectVisualAsAttackTarget(rangedAttackGridPosition);
                HandleSettingUnitsAsRangedAttackTarget(rangedAttackGridPosition);
            }

            if(attackType == AttackSO.AttackType.healAllyRangedTargeting) {
                SetGridObjectVisualAsHealTarget(rangedAttackGridPosition);
            }
            if (attackType == AttackSO.AttackType.healAllyMeleeTargeting) {
                SetGridObjectVisualAsHealTarget(rangedAttackGridPosition);
            }
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

    private void SetGridObjectVisualAsHealTarget(GridPosition rangedAttackGridPosition) {
        GridObjectVisual targetGridPositionGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(rangedAttackGridPosition);
        targetTilesGridObjectVisuals.Add(targetGridPositionGridObjectVisual);

        targetGridPositionGridObjectVisual.SetAsHealTargetTile();
    }

    private void SetGridObjectVisualAsBuffTarget(GridPosition gridPosition, SupportUnit.SupportType supportType) {
        GridObjectVisual targetGridPositionGridObjectVisual = BattleGrid.Instance.GetGridObjectVisual(gridPosition);
        targetTilesGridObjectVisuals.Add(targetGridPositionGridObjectVisual);

        if(supportType == SupportUnit.SupportType.attackSpeed) {
            targetGridPositionGridObjectVisual.SetAsAttackSpeedBuffTile();
        }
        if (supportType == SupportUnit.SupportType.attackDamage) {
            targetGridPositionGridObjectVisual.SetAsAttackDamageBuffTile();
        }
        if (supportType == SupportUnit.SupportType.moveSpeed) {
            targetGridPositionGridObjectVisual.SetAsMoveSpeedBuffTile();
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
