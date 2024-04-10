using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridSystem gridSystem;
    private GridPosition gridPosition;
    private List<IPlaceable> iPlaceableList;

    private Troop troopAtGridPosition;
    private List<Building> buildingListAtGridPosition = new List<Building>();

    private Troop troopSpawnedAtGridPosition;
    private Building buildingSpawnedAtGridPosition;
    private IPlaceable iPlaceableSpawnedAtGridPosition;

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
            if (troopAtGridPosition != null) return;
            troopAtGridPosition = iPlaceable as Troop;
        }

        if (iPlaceable is Building) {
            buildingListAtGridPosition.Add(iPlaceable as Building);
        }
    }

    public void RemoveIPlaceable(IPlaceable iPlaceable) {
        iPlaceableList.Remove(iPlaceable);

        if (iPlaceable is Troop) {
            if(iPlaceable as Troop == troopAtGridPosition) {
                troopAtGridPosition = null;
            }
        }

        if (iPlaceable is Building) {
            buildingListAtGridPosition.Remove(iPlaceable as Building);
        }
    }

    public void SetIPlaceableAsSpawned(IPlaceable iPlaceable) {
        if (iPlaceable is Troop) {
            troopSpawnedAtGridPosition = iPlaceable as Troop;
            iPlaceableSpawnedAtGridPosition = troopSpawnedAtGridPosition;
        }

        if (iPlaceable is Building) {
            buildingSpawnedAtGridPosition = iPlaceable as Building;
            iPlaceableSpawnedAtGridPosition = buildingSpawnedAtGridPosition;
        }
    }

    public IPlaceable GetIPlaceableSpawned() {
        return iPlaceableSpawnedAtGridPosition;
    }

    public Troop GetTroop() {
        return troopAtGridPosition;
    }

    public List<Building> GetBuildingList() {
        return buildingListAtGridPosition;
    }

    public List<IPlaceable> GetIPlaceableList() {
        return iPlaceableList;
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
