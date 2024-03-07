using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridSystem gridSystem;
    private GridPosition gridPosition;
    private List<IPlaceable> iPlaceableList;
    private List<Troop> troopList;
    private Building building;
    private List<Unit> unitList;

    public GridObject(GridSystem gridSystem, GridPosition gridPosition) {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
        iPlaceableList = new List<IPlaceable>();
        troopList = new List<Troop>();
        unitList = new List<Unit>();
    }

    public override string ToString() {
        return gridPosition.ToString();
    }

    public void AddIPlaceable(IPlaceable iPlaceable) {
        iPlaceableList.Add(iPlaceable);

        if(iPlaceable is Troop) {
            troopList.Add(iPlaceable as Troop);
        }

        if (iPlaceable is Building) {
            building = iPlaceable as Building;
        }
    }

    public void RemoveIPlaceable(IPlaceable iPlaceable) {
        iPlaceableList.Remove(iPlaceable);

        if (iPlaceable is Troop) {
            troopList.Remove(iPlaceable as Troop);
        }

        if (iPlaceable is Building) {
            building = null;
        }
    }

    public List<Troop> GetTroopList() {
        return troopList;
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

}
