using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGrid : MonoBehaviour
{
    public static BattleGrid Instance { get; private set;}

    [SerializeField] Transform gridObjectVisualPrefab;

    [SerializeField] Transform playerGridOrigin;
    [SerializeField] Transform opponentGridOrigin;

    [SerializeField] float interBattlefieldSpacing_Battle;
    [SerializeField] float interBattlefieldSpacing_Preparation;

    private GridSystem gridSystem;

    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private int gridCellSize;

    private void Awake() {
        Instance = this;

        gridSystem = new GridSystem(gridWidth, gridHeight, gridCellSize, playerGridOrigin.position, interBattlefieldSpacing_Preparation);
        gridSystem.CreateGridObjectVisuals(gridObjectVisualPrefab, playerGridOrigin, opponentGridOrigin);
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsBattlePhase()) {
            gridSystem.SetGridInterBattlefieldSpacing(interBattlefieldSpacing_Battle);
        } else {
            gridSystem.SetGridInterBattlefieldSpacing(interBattlefieldSpacing_Preparation);
        }
    }

    private void Update() {
        gridSystem.SetGridOrigin(playerGridOrigin.position);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) {
            return gridSystem.GetGridPosition(worldPosition);
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) {
        return gridSystem.GetWorldPosition(gridPosition);
    }

    public GridPosition TranslateOpponentGridPosition(GridPosition gridPosition) {
        GridPosition translatedGridPosition = gridPosition;

        translatedGridPosition.x = 11 - gridPosition.x;

        return translatedGridPosition;

    }

    public bool IsValidGridPosition(GridPosition gridPosition) {
        return gridSystem.IsValidGridPosition(gridPosition);
    }

    public bool IsValidPlayerGridPosition(GridPosition gridPosition) {
        return gridSystem.IsValidPlayerGridPosition(gridPosition);
    }

    public List<IPlaceable> GetIPlaceableListAtGridPosition(GridPosition gridPosition) {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetIPlaceableList();
    }

    public IPlaceable GetIPlaceableSpawnedAtGridPosition(GridPosition gridPosition) {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetIPlaceableSpawned();
    }

    public void SetIPlaceableSpawnedAtGridPosition(IPlaceable iPlaceable, GridPosition gridPosition) {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetIPlaceableAsSpawned(iPlaceable);
    }


    #region TROOP
    public void AddIPlaceableAtGridPosition(GridPosition gridPosition, IPlaceable iPlaceable) {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);

            gridObject.AddIPlaceable(iPlaceable);
    }

    public Troop GetTroopAtGridPosition(GridPosition gridPosition) {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);
            return gridObject.GetTroop();
    }

    public void RemoveIPlaceableAtGridPosition(GridPosition gridPosition, IPlaceable iPlaceable) {
            GridObject gridObject = gridSystem.GetGridObject(gridPosition);

            gridObject.RemoveIPlaceable(iPlaceable);
    }

    public void IPlaceableMovedGridPosition(IPlaceable iPlaceable, GridPosition fromGridPosition, GridPosition toGridPosition) {
        RemoveIPlaceableAtGridPosition(fromGridPosition, iPlaceable);
        AddIPlaceableAtGridPosition(toGridPosition, iPlaceable);
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

    #region BUILDING
    public void AddBuildingAtGridPosition(GridPosition gridPosition, Building building) {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.AddIPlaceable(building);
    }

    public Building GetBuildingAtGridPosition(GridPosition gridPosition) {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetBuilding();
    }

    #endregion
    public GridSystem GetGridSystem() {
        return gridSystem;
    }

    public GridObjectVisual GetGridObjectVisual(GridPosition gridPosition) {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetGridObjectVisual();
    }

    public int GetGridWidth() {
        return gridWidth;
    }

    public float GetBattlefieldMiddlePoint() {
        return playerGridOrigin.position.x + (opponentGridOrigin.position.x - playerGridOrigin.position.x) / 2;
    }

    public Transform GetPlayerGridOrigin() { return playerGridOrigin; }
    public Transform GetOpponentGridOrigin() { return opponentGridOrigin; }
}
