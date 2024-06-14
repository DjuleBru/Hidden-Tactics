using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    public event EventHandler OnShowIPlaceableIconPerformed;
    public bool showIPlaceableIcon;

    private void Awake() {
        Instance = this;
        showIPlaceableIcon = true;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.ShowIPlaceableIcons.performed += ShowIPlaceableIcons_performed;
    }

    private void ShowIPlaceableIcons_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        showIPlaceableIcon = !showIPlaceableIcon;
        OnShowIPlaceableIconPerformed?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVector() {
        Vector2 moveInput = playerInputActions.Player.MoveCamera.ReadValue<Vector2>();

        return moveInput;
    }

    public Vector2 GetZoomVector() {
        Vector2 zoomInput = playerInputActions.Player.Zoom.ReadValue<Vector2>();

        return zoomInput;
    }

    public bool GetShowIPlaceableIconSetting() {
        return showIPlaceableIcon;
    }
}
