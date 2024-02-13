using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGrid : MonoBehaviour
{
    public static BattleGrid Instance { get; private set;}

    [SerializeField] Transform gridDebugObjectPrefab;

    [SerializeField] Transform gridOrigin;
    [SerializeField] float interBattlefieldSpacing;
    private GridSystem gridSystem;

    private void Awake() {
        Instance = this;

        gridSystem = new GridSystem(10, 5, 9, gridOrigin.position, interBattlefieldSpacing);
        gridSystem.CreateDebugObjects(gridDebugObjectPrefab, gridOrigin);
    }

    private void Update() {
        gridSystem.SetGridOrigin(gridOrigin.position);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) {
            return gridSystem.GetGridPosition(worldPosition);
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) {
        return gridSystem.GetWorldPosition(gridPosition);
    }

    public bool IsValidGridPosition(GridPosition gridPosition) {
        return gridSystem.IsValidGridPosition(gridPosition);
    }

    public bool IsValidTroopGridPositioning(GridPosition gridPosition) {
        return gridSystem.IsValidTroopGridPositioning(gridPosition);
    }

    #region TROOP
    public void AddTroopAtGridPosition(GridPosition gridPosition, Troop troop) {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            gridObject.AddTroop(troop);
    }

    public List<Troop> GetTroopListAtGridPosition(GridPosition gridPosition) {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            return gridObject.GetTroopList();
    }

    public void RemoveTroopAtGridPosition(GridPosition gridPosition, Troop troop) {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            gridObject.RemoveTroop(troop);
    }
    public void TroopMovedGridPosition(Troop troop, GridPosition fromGridPosition, GridPosition toGridPosition) {
        RemoveTroopAtGridPosition(fromGridPosition, troop);
        AddTroopAtGridPosition(toGridPosition, troop);
    }
    #endregion

    #region UNIT

    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit) {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            gridObject.AddUnit(unit);
    }

    public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition) {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            return gridObject.GetUnitList();
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit) {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            gridObject.RemoveUnit(unit);
    }

    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition) {
            RemoveUnitAtGridPosition(fromGridPosition, unit);
            AddUnitAtGridPosition(toGridPosition, unit);
    }
    #endregion

}
