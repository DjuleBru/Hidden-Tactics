using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridSystem gridSystem;
    private GridPosition gridPosition;
    private List<Troop> troopList;
    private List<Unit> unitList;

    public GridObject(GridSystem gridSystem, GridPosition gridPosition) {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
        troopList = new List<Troop>();
        unitList = new List<Unit>();
    }

    public override string ToString() {
        return gridPosition.ToString();
    }

    public void AddTroop(Troop troop) {
        troopList.Add(troop);
    }

    public void RemoveTroop(Troop troop) {
        troopList.Remove(troop);
    }

    public List<Troop> GetTroopList() {
        return troopList;
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
