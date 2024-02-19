using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem
{
    private int playerBattlefieldWidth = 6;

    private int width;
    private int height;
    private float cellSize;
    private float interBattlefieldSpacing;
    private Vector3 gridOrigin;

    private GridObject[,] gridObjectArray;
    private GridObjectVisual[,] gridObjectVisualArray;

    public GridSystem(int width, int height, float cellSize, Vector3 gridOrigin, float interBattlefieldSpacing) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.gridOrigin = gridOrigin;
        this.interBattlefieldSpacing = interBattlefieldSpacing;
        gridObjectArray = new GridObject[width, height];
        gridObjectVisualArray = new GridObjectVisual[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {

                GridPosition gridPosition = new GridPosition(x, y);
                gridObjectArray[x, y] = new GridObject(this, gridPosition);

            }
        }
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) {
        if(gridPosition.x >= playerBattlefieldWidth) {
            return (new Vector3(gridPosition.x * cellSize + interBattlefieldSpacing + gridOrigin.x, 
                                gridPosition.y * cellSize + gridOrigin.y, 
                                0)) ;
        }
        return (new Vector3(gridPosition.x, gridPosition.y, 0)) * cellSize + gridOrigin;
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) {
        int x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x) / cellSize);
        int y = Mathf.RoundToInt((worldPosition.y - gridOrigin.y) / cellSize);

        if (x >= playerBattlefieldWidth) {
            x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x - interBattlefieldSpacing) / cellSize);
        }

        return new GridPosition(x, y);
    }

    public void CreateGridObjectVisuals(Transform visualPrefab, Transform playerParentTransform, Transform opponentParentTransform) {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GridPosition gridPosition = new GridPosition(x, y);
                Transform visualTransform = GameObject.Instantiate(visualPrefab, GetWorldPosition(gridPosition) - new Vector3(cellSize / 2, cellSize / 2, 0), Quaternion.identity);
                if (x < width/2) {
                    visualTransform.SetParent(playerParentTransform);
                } else {
                    visualTransform.SetParent(opponentParentTransform);
                }
                GridObjectVisual gridObjectVisual = visualTransform.GetComponent<GridObjectVisual>();
                gridObjectVisual.SetGridObject(GetGridObject(gridPosition));

                gridObjectVisualArray[x, y] = gridObjectVisual;
            }
        }
    }

    public void SetGridObjectVisualSprites(List<Sprite> playerGridSprites, List<Sprite> opponentGridSprites, List<Sprite> playerSettlementSprites, List<Sprite> opponentSettlementSprites, List<Sprite> playerVillageSprites, List<Sprite> opponentVillageSprites) {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GridObjectVisual gridObjectVisual = gridObjectVisualArray[x, y];

                // first and last sprites = settlements
                if(x == 0) {
                    gridObjectVisual.SetGridSprite(playerSettlementSprites);
                    gridObjectVisual.SetVillageSprites(playerVillageSprites);
                    continue;
                }
                if(x == width -1) {
                    gridObjectVisual.SetGridSprite(opponentSettlementSprites);
                    gridObjectVisual.SetVillageSprites(opponentVillageSprites);
                    continue;
                }

                if (x < width / 2) {
                    gridObjectVisual.SetGridSprite(playerGridSprites);
                }
                else {
                    gridObjectVisual.SetGridSprite(opponentGridSprites);
                }
            }
        }
    }

    public GridObject GetGridObject(GridPosition gridPosition) {
        return gridObjectArray[gridPosition.x, gridPosition.y];
    }

    public GridObjectVisual GetGridObjectVisual(GridPosition gridPosition) {
        return gridObjectVisualArray[gridPosition.x, gridPosition.y];
    }

    public bool IsValidGridPosition(GridPosition gridPosition) {
        // Returns true if grid position is on battlefield
        if (gridPosition.x < 0 || gridPosition.y < 0 || gridPosition.x >= gridObjectArray.GetLength(0) || gridPosition.y >= gridObjectArray.GetLength(1)) {
            return false;
        }
        return true;
    }

    public bool IsValidPlayerGridPosition(GridPosition gridPosition) {
        // Returns true if grid position is on player side of the battlefield
        if (gridPosition.x < 0 || gridPosition.y < 0 || gridPosition.x >= playerBattlefieldWidth || gridPosition.y >= gridObjectArray.GetLength(1)) {
            return false;
        }
        return true;
    }

    public void SetGridOrigin(Vector3 gridOrigin) {
        this.gridOrigin = gridOrigin;
    }

    public void SetGridInterBattlefieldSpacing(float interBattlefieldSpacing) {
        this.interBattlefieldSpacing = interBattlefieldSpacing;
    }

}
