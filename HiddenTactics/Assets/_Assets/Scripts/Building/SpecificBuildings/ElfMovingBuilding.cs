using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ElfMovingBuilding : Building
{
    private bool movingTreeWall;
    [SerializeField] private GameObject movingTreeWallVisual;
    [SerializeField] private Button movingTreeWallButton;
    private GridPosition movingTreeWallVisualGridPosition;

    [SerializeField] private List<Vector2> validRelativeMovementGridPositionVectors;
    private List<GridPosition> validRelativeMovementGridPosition = new List<GridPosition>();
    private List<GridPosition> unValidRelativeMovementGridPosition = new List<GridPosition>();

    protected override void Awake() {
        base.Awake();
        movingTreeWallVisual.SetActive(false);
    }

    public override void OnNetworkSpawn() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        OnAnyBuildingPlaced += Building_OnAnyBuildingPlaced;
        OnAnyBuildingDestroyed += Building_OnAnyBuildingDestroyed;
        Troop.OnAnyTroopPlaced += Troop_OnAnyTroopPlaced;
        Troop.OnAnyTroopSelled += Troop_OnAnyTroopSelled;
    }

    protected override void Update() {
        base.Update();
        if(movingTreeWall) {
            HandleTreeWallMovementOnGrid();
            HandleTreeWallMovementDuringPlacement();

            if(Input.GetMouseButtonDown(0)) {
                if (!BattleGrid.Instance.IsValidPlayerGridPosition(movingTreeWallVisualGridPosition)) return;
                if (!PlayerAction_SpawnIPlaceable.LocalInstance.IsValidIPlaceableSpawningTarget(movingTreeWallVisualGridPosition)) return;
                MoveTreeWall();
            }
            
            if(Input.GetMouseButtonDown(1)) {
                CancelMovingTreeWall();
            }
        }
    }

    protected override void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        base.BattleManager_OnStateChanged((object)sender, e);

        if(BattleManager.Instance.IsBattlePhaseStarting()) {
            movingTreeWallButton.interactable = true;
            FillValidMovementGridPositionsList();

            if (!isOwnedByPlayer) return;
            SynchronizeTreeWallPositionServerRpc(new Vector2(currentGridPosition.x, currentGridPosition.y));
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void SynchronizeTreeWallPositionServerRpc(Vector2 gridPosition) {
        SynchronizeTreeWallPositionClientRpc(gridPosition);
    }

    [ClientRpc]
    private void SynchronizeTreeWallPositionClientRpc(Vector2 gridPosition) {

        if (isOwnedByPlayer) return;

        GridPosition currentOpponentGridPosition = currentGridPosition;
        GridPosition newGridPositon = BattleGrid.Instance.TranslateOpponentGridPosition(new GridPosition((int)gridPosition.x, (int)gridPosition.y));

        SetTreeWallNewPosition(currentOpponentGridPosition, newGridPositon);
        this.currentGridPosition = newGridPositon;
    }

    private void MoveTreeWall() {
        movingTreeWall = false;
        movingTreeWallVisual.SetActive(false);

        PlayerActionsManager.LocalInstance.ChangeAction(PlayerActionsManager.Action.Idle);
        GridHoverManager.Instance.ShowGridPositionsValid(validRelativeMovementGridPosition, false);
        GridHoverManager.Instance.ShowGridPositionsUnvalid(unValidRelativeMovementGridPosition, false);

        SetTreeWallNewPosition(currentGridPosition, movingTreeWallVisualGridPosition);
    }

    private void SetTreeWallNewPosition(GridPosition previousGridPosition, GridPosition newGridPosition) {
        BattleGrid.Instance.IPlaceableMovedGridPosition(this, previousGridPosition, newGridPosition);
        currentGridPosition = newGridPosition;
        transform.position = BattleGrid.Instance.GetWorldPosition(currentGridPosition) - buildingCenterPoint.localPosition;
        battlefieldOffset = transform.position - battlefieldOwner.transform.position;
        InvokeOnBuildingPlaced();
        movingTreeWallButton.interactable = false;
    }

    public void StartMovingTreeWall() {
        PlayerActionsManager.LocalInstance.ChangeAction(PlayerActionsManager.Action.MovingIPlaceable);
        GridHoverManager.Instance.ShowGridPositionsValid(validRelativeMovementGridPosition, true);
        GridHoverManager.Instance.ShowGridPositionsUnvalid(unValidRelativeMovementGridPosition, true);

        movingTreeWallVisual.SetActive(true);
        movingTreeWall = true;
    }

    public void CancelMovingTreeWall() {
        PlayerActionsManager.LocalInstance.ChangeAction(PlayerActionsManager.Action.Idle);
        GridHoverManager.Instance.ShowGridPositionsValid(validRelativeMovementGridPosition, false);
        GridHoverManager.Instance.ShowGridPositionsUnvalid(unValidRelativeMovementGridPosition, false);
        movingTreeWallVisual.SetActive(false);
        movingTreeWall = false;
    }

    private void HandleTreeWallMovementOnGrid() {
        if (!isOwnedByPlayer) return;

        GridPosition newGridPosition = MousePositionManager.Instance.GetMouseGridPosition();

        // Grid position is not a valid position
        if (!validRelativeMovementGridPosition.Contains(newGridPosition)) return;
        // building was not set at a grid position yet
        if (movingTreeWallVisualGridPosition == null) {
            movingTreeWallVisualGridPosition = MousePositionManager.Instance.GetMouseGridPosition();
        }

        // Troop changed grid position
        if (newGridPosition != movingTreeWallVisualGridPosition) {
            movingTreeWallVisualGridPosition = newGridPosition;
        }
    }

    private void HandleTreeWallMovementDuringPlacement() {
        if (movingTreeWallVisualGridPosition == null) {

            if (!isOwnedByPlayer) return;
            movingTreeWallVisual.transform.position = MousePositionManager.Instance.GetMousePositionWorldPoint() - buildingCenterPoint.localPosition;
        }
        else {
            movingTreeWallVisual.transform.position = BattleGrid.Instance.GetWorldPosition(movingTreeWallVisualGridPosition) - buildingCenterPoint.localPosition;
        }
    }

    private void Troop_OnAnyTroopSelled(object sender, System.EventArgs e) {
        FillValidMovementGridPositionsList();
    }

    private void Troop_OnAnyTroopPlaced(object sender, System.EventArgs e) {
        FillValidMovementGridPositionsList();
    }

    private void Building_OnAnyBuildingDestroyed(object sender, System.EventArgs e) {
        FillValidMovementGridPositionsList();
    }

    private void Building_OnAnyBuildingPlaced(object sender, System.EventArgs e) {
        FillValidMovementGridPositionsList();
    }

    private void FillValidMovementGridPositionsList() {
        validRelativeMovementGridPosition.Clear();
        unValidRelativeMovementGridPosition.Clear();

        foreach (Vector2 vector2 in validRelativeMovementGridPositionVectors) {
            GridPosition absoluteGridPosition = new GridPosition(currentGridPosition.x + (int)vector2.x, currentGridPosition.y + (int)vector2.y);
            if (!BattleGrid.Instance.IsValidPlayerGridPosition(absoluteGridPosition)) return;
            if (!PlayerAction_SpawnIPlaceable.LocalInstance.IsValidIPlaceableSpawningTarget(absoluteGridPosition)) {
                unValidRelativeMovementGridPosition.Add(absoluteGridPosition);
                continue;
            };
            validRelativeMovementGridPosition.Add(absoluteGridPosition);
        }
    }

}
