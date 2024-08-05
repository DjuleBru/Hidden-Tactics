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

        if(BattleManager.Instance != null) {
            // Battle scene : create grid for both player AND opponent
            gridSystem.CreateGridObjectVisuals(gridObjectVisualPrefab, playerGridOrigin, opponentGridOrigin);

        } else {
            // Lobby scene : create grid for only player
            gridSystem.CreateGridObjectVisuals(gridObjectVisualPrefab, playerGridOrigin);
        }
    }

    private void Start() {
        if(BattleManager.Instance != null) {
            BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        }
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsBattlePhase()) {
            gridSystem.SetGridInterBattlefieldSpacing(interBattlefieldSpacing_Battle);
        } else {
            gridSystem.SetGridInterBattlefieldSpacing(interBattlefieldSpacing_Preparation);
        }
    }

    private void Update() {
        if (playerGridOrigin == null) return;
        //Fix required because on scene change, player grid origin would be destroyed thus leading to nullrefexception ?

        gridSystem.SetGridOrigin(playerGridOrigin.position);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) {
            return gridSystem.GetGridPosition(worldPosition);
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) {
        return gridSystem.GetWorldPosition(gridPosition);
    }

    public Vector3 GetMoveForwardsNextGridPosition(Unit unit) {
        // Recompose where the unit should be : current grid position for X, initial grid position for Y
        GridPosition unitGridPosition = new GridPosition (unit.GetCurrentGridPosition().x, unit.GetInitialUnitGridPosition().y);
        GridPosition nextGridPosition = new GridPosition(0, 0);
        Vector3 nextWorldPosition = Vector3.zero;

        if (unit.IsOwnedByPlayer()) {
            nextGridPosition = new GridPosition(unitGridPosition.x + 2, unitGridPosition.y);

            //Unit reached end on battlefield
            if (nextGridPosition.x == 12) {
                nextGridPosition = new GridPosition(11, nextGridPosition.y);
                nextWorldPosition = gridSystem.GetWorldPosition(nextGridPosition);
                nextWorldPosition.x += gridCellSize * 3;
                nextWorldPosition.y += unit.GetUnitPositionInTroop().y - gridCellSize / 2;
            } else {
                nextWorldPosition = gridSystem.GetWorldPosition(nextGridPosition);
                nextWorldPosition.y += unit.GetUnitPositionInTroop().y - gridCellSize / 2;
                nextWorldPosition.x += unit.GetUnitPositionInTroop().x - gridCellSize / 2;
            }
        } else {

            nextGridPosition = new GridPosition(unitGridPosition.x - 2, unitGridPosition.y);

            //Unit reached end on battlefield
            if (nextGridPosition.x == -1) {
                nextGridPosition = new GridPosition(0, nextGridPosition.y);
                nextWorldPosition = gridSystem.GetWorldPosition(nextGridPosition);
                nextWorldPosition.x -= gridCellSize * 3;
                nextWorldPosition.y += unit.GetUnitPositionInTroop().y - gridCellSize / 2;
            } else {
                nextWorldPosition = gridSystem.GetWorldPosition(nextGridPosition);
                nextWorldPosition.y += unit.GetUnitPositionInTroop().y - gridCellSize / 2;
                nextWorldPosition.x += unit.GetUnitPositionInTroop().x - gridCellSize / 2;
            }
        }

        return nextWorldPosition;
    }

    public Vector3 GetMoveForwardsCustomGridPosition(Unit unit, GridPosition gridPosition) {

        GridPosition nextGridPosition = new GridPosition(0, 0);
        Vector3 nextWorldPosition = Vector3.zero;

        if (unit.IsOwnedByPlayer()) {
            nextGridPosition = new GridPosition(gridPosition.x + 1, gridPosition.y);

            nextWorldPosition = gridSystem.GetWorldPosition(nextGridPosition);
            nextWorldPosition.y += unit.GetUnitPositionInTroop().y - gridCellSize / 2;
            nextWorldPosition.x += unit.GetUnitPositionInTroop().x - gridCellSize / 2;
        }
        else {

            nextGridPosition = new GridPosition(gridPosition.x - 1, gridPosition.y);

            nextWorldPosition = gridSystem.GetWorldPosition(nextGridPosition);
            nextWorldPosition.y += unit.GetUnitPositionInTroop().y - gridCellSize / 2;
            nextWorldPosition.x += unit.GetUnitPositionInTroop().x - gridCellSize / 2;
        }

        return nextWorldPosition;
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

    public void ResetIPlaceableSpawnedAtGridPosition(GridPosition gridPosition) {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.ResetIPlaceableSpawned();
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

    public bool ValidGridPositionLeft() {

        for (int x = 0; x < gridSystem.width; x++) {
            for (int y = 0; y < gridSystem.height; y++) {

                GridPosition gridPosition = new GridPosition(x, y);
                if (PlayerAction_SpawnIPlaceable.LocalInstance.IsValidIPlaceableSpawningTarget(gridPosition) && IsValidPlayerGridPosition(gridPosition)) {
                    return true;
                }
            }
        }

        return false;
    }

    public GridPosition GetFirstValidGridPosition() {
        GridPosition gridPosition = new GridPosition(0, 0);

        for (int x = 0; x < gridSystem.width; x++) {
            for (int y = 0; y < gridSystem.height; y++) {

                gridPosition = new GridPosition(x, y);
                if(PlayerAction_SpawnIPlaceable.LocalInstance.IsValidIPlaceableSpawningTarget(gridPosition) && IsValidPlayerGridPosition(gridPosition)) {
                    return gridPosition;
                }
            }
        }
        return gridPosition;
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
