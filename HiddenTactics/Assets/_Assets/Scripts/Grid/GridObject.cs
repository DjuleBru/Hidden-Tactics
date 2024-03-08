using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridSystem gridSystem;
    private GridPosition gridPosition;
    private List<IPlaceable> iPlaceableList;
    private Troop troop;
    private Building building;
    private List<Unit> unitList;
    private GridObjectVisual gridObjectVisual;

    public GridObject(GridSystem gridSystem, GridPosition gridPosition) {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
        iPlaceableList = new List<IPlaceable>();
        unitList = new List<Unit>();
    }

    public override string ToString() {
        return gridPosition.ToString();
    }

    public void AddIPlaceable(IPlaceable iPlaceable) {
        iPlaceableList.Add(iPlaceable);

        if(iPlaceable is Troop) {
            troop = iPlaceable as Troop;
        }

        if (iPlaceable is Building) {
            building = iPlaceable as Building;
        }
    }

    public void RemoveIPlaceable(IPlaceable iPlaceable) {
        iPlaceableList.Remove(iPlaceable);

        if (iPlaceable is Troop) {
            troop = null;
        }

        if (iPlaceable is Building) {
            building = null;
        }
    }

    public Troop GetTroop() {
        return troop;
    }

    public Building GetBuilding() {
        return building;
    }

    public void AddUnit(Unit unit) {
        unitList.Add(unit);
    }

    public void RemoveUnit(Unit unit) {
        unitList.Remove(unit);
    }

    public List<Unit> GetUnitList() {
        return unitList;
    }

    public void SetGridObjectVisual(GridObjectVisual gridObjectVisual) {
        this.gridObjectVisual = gridObjectVisual;
    }

    public GridObjectVisual GetGridObjectVisual() {
        return gridObjectVisual;
    }
}
