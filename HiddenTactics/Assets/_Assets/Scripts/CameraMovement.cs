using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMovement : MonoBehaviour
{
    Vector3 moveInput;
    [SerializeField] Vector3 followOffset;

    float zoom;
    float velocity = 0f;

    [SerializeField] float zoomMultiplier;
    [SerializeField] float cameraMoveSpeed;
    [SerializeField] float cameraZoomSpeed;
    [SerializeField] float minOrtho;
    [SerializeField] float maxOrtho;
    [SerializeField] float smoothTime;
    [SerializeField] CinemachineVirtualCamera mainVirtualCamera;

    [SerializeField] float cameraMaxX;
    [SerializeField] float cameraMaxY;
    [SerializeField] float cameraMinX;
    [SerializeField] float cameraMinY;

    public event EventHandler OnCameraZoomedChanged;
    public static CameraMovement Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        zoom = mainVirtualCamera.m_Lens.OrthographicSize;
    }

    private void Update() {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement() {
        moveInput = GameInput.Instance.GetMovementVector();
        if (moveInput != Vector3.zero) {
            Vector3 newPosition = transform.position + moveInput * cameraMoveSpeed * Time.deltaTime;
            if (newPosition.x < cameraMaxX & newPosition.x > cameraMinX & newPosition.y < cameraMaxY & newPosition.y > cameraMinY) {
                transform.position += moveInput * cameraMoveSpeed * Time.deltaTime;
            }
        }
    }

    private void HandleZoom() {
        float scroll = GameInput.Instance.GetZoomVector().y;

        zoom -= scroll * cameraZoomSpeed * zoomMultiplier;
        zoom = Mathf.Clamp(zoom, minOrtho, maxOrtho);
        mainVirtualCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(mainVirtualCamera.m_Lens.OrthographicSize, zoom, ref velocity, smoothTime);

        OnCameraZoomedChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetCameraOrthoSize() {
        return mainVirtualCamera.m_Lens.OrthographicSize;
    }
}
