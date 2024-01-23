using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    public Vector2 GetMovementVector() {
        Vector2 moveInput = playerInputActions.Player.MoveCamera.ReadValue<Vector2>();

        return moveInput;
    }

    public Vector2 GetZoomVector() {
        Vector2 zoomInput = playerInputActions.Player.Zoom.ReadValue<Vector2>();

        return zoomInput;
    }
}
