using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SupportUnit : NetworkBehaviour {

    public enum SupportType {
        none,
        attackSpeed,

    }

    private List<Vector2> buffTargetTiles = new List<Vector2>();
    private List<Unit> unitsBuffedList = new List<Unit>();
    private List<Unit> unitsBuffedVisualsList = new List<Unit>();
    List<GridPosition> surroundingGridPositions = new List<GridPosition>();
    private Unit unit;

    private void Awake() {
        unit = GetComponent<Unit>();
    }

    public override void OnNetworkSpawn() {
        unit.OnUnitChangedGridPosition += Unit_OnUnitChangedGridPosition;
        unit.OnUnitDied += Unit_OnUnitDied;
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Start() {
        buffTargetTiles = GetComponentInParent<Troop>().GetTroopSO().buffedGridPositions;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        Debug.Log("phase changed");

        // Buffs activate at the beginning of the battle phase
        if (BattleManager.Instance.IsBattlePhase()) {
            RefreshSurroundingGridPositions();
            HandleBuffsInSurroundingGridPositions();

            foreach (Unit unit in unitsBuffedList) {
                unit.GetComponentInChildren<UnitStatusEffectVisuals>().ActivateBuffVisuals();
            }
        }

        // Buffs DeActivate at the beginning of the Preparation phase
        if (BattleManager.Instance.IsPreparationPhase()) {
            DebuffUnitsInRange();
        }
    }

    private void Unit_OnUnitPlaced(object sender, System.EventArgs e) {
        Debug.Log(unit.IsOwnedByPlayer());
        if(unit.IsOwnedByPlayer()) {
            HideBuffedUnitBuffs();
        }

        RefreshSurroundingGridPositions();
        GridHoverManager.Instance.AddSupportUnit(unit);
    }

    private void Unit_OnUnitDied(object sender, System.EventArgs e) {
        foreach(Unit unit in unitsBuffedList) {
            unit.GetComponent<UnitAttack>().ResetAttackRate();
        }
    }

    private void Unit_OnUnitChangedGridPosition(object sender, System.EventArgs e) {
        if (unit.GetUnitIsPlaced()) return;

        RefreshSurroundingGridPositions();
        HandleBuffsVisualsInSurroundingGridPositions();
    }

    private void RefreshSurroundingGridPositions() {
        GridPosition gridPosition = unit.GetCurrentGridPosition();

        surroundingGridPositions.Clear();

        foreach (Vector2 vector2 in buffTargetTiles) {
            GridPosition targetGridPosition = new GridPosition(gridPosition.x + (int)vector2.x, gridPosition.y + (int)vector2.y);

            if(BattleGrid.Instance.IsValidGridPosition(targetGridPosition)) {
                surroundingGridPositions.Add(targetGridPosition);
            }
        }

    }

    private void HandleBuffsInSurroundingGridPositions() {
        List<Unit> unitListInSurroundingGridPositions = new List<Unit>();

        // Fetch units in surrounding grid positions
        foreach (GridPosition gridPosition in surroundingGridPositions) {

            List<Unit> unitListAtGridPosition = BattleGrid.Instance.GetUnitListAtGridPosition(gridPosition);

            foreach(Unit unit in unitListAtGridPosition) {
                bool targetUnitIsInSameTeam = (unit.IsOwnedByPlayer() == this.unit.IsOwnedByPlayer());
                if (unit.GetUnitIsBought() && targetUnitIsInSameTeam && (unit != this.unit)) {
                    unitListInSurroundingGridPositions.Add(unit);
                }
            }
        }

        // Debuff units not in surrounding grid positions anymore
        List<Unit> unitsToDebuff = new List<Unit>();

        foreach (Unit unit in unitsBuffedList) {
            if(!unitListInSurroundingGridPositions.Contains(unit)) {
                bool targetUnitIsInSameTeam = (unit.IsOwnedByPlayer() == this.unit.IsOwnedByPlayer());
                if (unit.GetUnitIsBought() && targetUnitIsInSameTeam && (unit != this.unit)) {
                    unitsToDebuff.Add(unit);
                }
            }
        }

        foreach(Unit unit in unitsToDebuff) {
            unit.GetComponent<UnitAttack>().ResetAttackRate();
            unitsBuffedList.Remove(unit);
        }

        // Buff new units not in surrounding grid positions 
        foreach(Unit unit in unitListInSurroundingGridPositions) {

            if (!unitsBuffedList.Contains(unit)) {
                unitsBuffedList.Add(unit);
                unit.GetComponent<UnitAttack>().BuffAttackRate(.5f);
            }

        }
    }

    private void HandleBuffsVisualsInSurroundingGridPositions() {
        List<Unit> unitListInSurroundingGridPositions = new List<Unit>();

        // Fetch units in surrounding grid positions
        foreach (GridPosition gridPosition in surroundingGridPositions) {

            List<Unit> unitListAtGridPosition = BattleGrid.Instance.GetUnitListAtGridPosition(gridPosition);

            foreach (Unit unit in unitListAtGridPosition) {
                bool targetUnitIsInSameTeam = (unit.IsOwnedByPlayer() == this.unit.IsOwnedByPlayer());
                if (unit.GetUnitIsBought() && targetUnitIsInSameTeam && (unit != this.unit)) {
                    unitListInSurroundingGridPositions.Add(unit);
                }
            }
        }

        // Debuff units not in surrounding grid positions anymore
        List<Unit> unitsToDebuff = new List<Unit>();

        foreach (Unit unit in unitsBuffedVisualsList) {
            if (!unitListInSurroundingGridPositions.Contains(unit)) {
                bool targetUnitIsInSameTeam = (unit.IsOwnedByPlayer() == this.unit.IsOwnedByPlayer());
                if (unit.GetUnitIsBought() && targetUnitIsInSameTeam && (unit != this.unit)) {
                    unitsToDebuff.Add(unit);
                }
            }
        }

        foreach (Unit unit in unitsToDebuff) {
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().HideBuffEffects();
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().HideBuffBase();
            unitsBuffedVisualsList.Remove(unit);
        }

        // Buff new units not in surrounding grid positions 
        foreach (Unit unit in unitListInSurroundingGridPositions) {
            if (!unitsBuffedVisualsList.Contains(unit)) {
                unitsBuffedVisualsList.Add(unit);
                unit.GetComponentInChildren<UnitStatusEffectVisuals>().ShowBuffEffects();
                unit.GetComponentInChildren<UnitStatusEffectVisuals>().ShowBuffBase();
            }

        }
    }

    private void DebuffUnitsInRange() {
        List<Unit> unitsToDebuff = new List<Unit>();

        foreach (Unit unit in unitsBuffedList) {
            unitsToDebuff.Add(unit);
        }

        foreach (Unit unit in unitsToDebuff) {
            unit.GetComponent<UnitAttack>().ResetAttackRate();
            unitsBuffedList.Remove(unit);
        }
    }

    public void ShowBuffedUnitBuffs() {
        Debug.Log("trying to shows");
        HandleBuffsVisualsInSurroundingGridPositions();

        foreach (Unit unit in unitsBuffedList) {
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().ShowBuffEffects();
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().ShowBuffBase();
        }

        foreach (Unit unit in unitsBuffedVisualsList) {
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().ShowBuffEffects();
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().ShowBuffBase();
        }
    }

    public void HideBuffedUnitBuffs() {
        Debug.Log("trying to hide");
        HandleBuffsVisualsInSurroundingGridPositions();

        foreach (Unit unit in unitsBuffedList) {
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().HideBuffEffects();
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().HideBuffBase();
        }

        foreach (Unit unit in unitsBuffedVisualsList) {
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().HideBuffEffects();
            unit.GetComponentInChildren<UnitStatusEffectVisuals>().HideBuffBase();
        }
    }

    public override void OnDestroy() {
        HideBuffedUnitBuffs();
        GridHoverManager.Instance.RemoveSupportUnit(unit);
    }

}
