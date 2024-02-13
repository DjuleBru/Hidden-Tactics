using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class GridSystem
{
    private int width;
    private int height;
    private float cellSize;
    private float interBattlefieldSpacing;
    private Vector3 gridOrigin;

    private GridObject[,] gridObjectArray;

    public GridSystem(int width, int height, float cellSize, Vector3 gridOrigin, float interBattlefieldSpacing) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.gridOrigin = gridOrigin;
        this.interBattlefieldSpacing = interBattlefieldSpacing;
        gridObjectArray = new GridObject[width, height];

        for(int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {

                GridPosition gridPosition = new GridPosition(x, y);
                gridObjectArray[x, y] = new GridObject(this, gridPosition);

            }
        }
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) {
        if(gridPosition.x >= 5) {
            return (new Vector3(gridPosition.x * cellSize + interBattlefieldSpacing + gridOrigin.x, 
                                gridPosition.y * cellSize + gridOrigin.y, 
                                0)) ;
        }
        return (new Vector3(gridPosition.x, gridPosition.y, 0)) * cellSize + gridOrigin;
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) {
        int x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x) / cellSize);
        int y = Mathf.RoundToInt((worldPosition.y - gridOrigin.y) / cellSize);

        if (x >= 5) {
            x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x - interBattlefieldSpacing /2) / cellSize);
        }

        return new GridPosition(x, y);
    }

    public void CreateDebugObjects(Transform debugPrefab, Transform parentTransform) {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GridPosition gridPosition = new GridPosition(x, y);
                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition) - new Vector3(cellSize / 2, cellSize / 2, 0), Quaternion.identity, parentTransform);
                debugTransform.GetComponent<GridDebugObject>().SetGridObject(GetGridObject(gridPosition));
            }
        }
    }

    public GridObject GetGridObject(GridPosition gridPosition) {
        return gridObjectArray[gridPosition.x, gridPosition.y];
    }

    public void SetGridOrigin(Vector3 gridOrigin) {
        this.gridOrigin = gridOrigin;
    }

}
