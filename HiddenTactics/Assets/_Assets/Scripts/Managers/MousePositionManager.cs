using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class MousePositionManager : MonoBehaviour {

    public static MousePositionManager Instance;
    private Camera mainCamera;

    private EventSystem eventSys;

    private void Awake() {
        Instance = this;
        mainCamera = Camera.main;
        eventSys = EventSystem.current;
    }

    public Vector3 GetMousePositionWorldPoint() {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
        return mouseWorldPosition;
    }

    public GridPosition GetMouseGridPosition() {
        return BattleGrid.Instance.GetGridPosition(GetMousePositionWorldPoint());
    }

    public bool IsPointerOverUIElement() {
        return eventSys.IsPointerOverGameObject();
    }

}
