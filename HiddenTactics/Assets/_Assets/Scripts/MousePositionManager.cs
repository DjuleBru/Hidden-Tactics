using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MousePositionManager : MonoBehaviour {

    public static MousePositionManager Instance;
    private Camera mainCamera;

    private GridObjectVisual hoveredGridObjectVisual;

    private void Awake() {
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Update() {
        //Debug.Log(GetMouseGridPosition());

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

        if (hit.collider != null)
        {
            hit.collider.gameObject.TryGetComponent<GridObjectVisual>(out GridObjectVisual gridObjectVisual);

            if(hoveredGridObjectVisual != null) {
                if(hoveredGridObjectVisual == gridObjectVisual) {
                    return;
                } else {
                    hoveredGridObjectVisual.SetUnSelected();

                    hoveredGridObjectVisual = gridObjectVisual;
                    hoveredGridObjectVisual.SetSelected();
                }
            } else {
                hoveredGridObjectVisual = gridObjectVisual;
                hoveredGridObjectVisual.SetSelected();
            }

        }
    }

    public Vector3 GetMousePositionWorldPoint() {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
        return mouseWorldPosition;
    }

    public GridPosition GetMouseGridPosition() {
        return BattleGrid.Instance.GetGridPosition(GetMousePositionWorldPoint());
    }

}
