using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    private Vector3 moveInput;
    [SerializeField] Vector3 followOffset;

    private float zoom;
    private float velocity = 0f;
    private bool tacticalView;

    [SerializeField] float zoomMultiplier;
    [SerializeField] float cameraMoveSpeed;
    [SerializeField] float cameraZoomSpeed;
    [SerializeField] float minOrtho;
    [SerializeField] float maxOrtho;
    [SerializeField] float smoothTime;
    [SerializeField] CinemachineVirtualCamera mainVirtualCamera;
    [SerializeField] CinemachineVirtualCamera tacticalViewVirtualCamera;

    [SerializeField] float cameraMaxX;
    [SerializeField] float cameraMaxY;
    [SerializeField] float cameraMinX;
    [SerializeField] float cameraMinY;

    private float worldIconScaleMultiplier = 1f;
    private float worldIconScaleTickPerZoom = .01f;

    public event EventHandler OnCameraZoomedChanged;
    public static CameraManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        zoom = mainVirtualCamera.m_Lens.OrthographicSize;
        SettingsManager.Instance.OnTacticalViewDisabled += SettingsManager_OnTacticalViewDisabled;
        SettingsManager.Instance.OnTacticalViewEnabled += SettingsManager_OnTacticalViewEnabled;
        BattleManager.Instance.OnAllPlayersLoaded += BattleManager_OnAllPlayersLoaded;
    }

    private void Update() {
        HandleMovement();
        HandleZoom();
    }

    private void BattleManager_OnAllPlayersLoaded(object sender, EventArgs e) {
        StartCoroutine(SetBattleCameraOnAllPlayersLoaded(.5f));
    }

    private IEnumerator SetBattleCameraOnAllPlayersLoaded(float delay) {
        yield return new WaitForSeconds(delay);
        tacticalViewVirtualCamera.enabled = false;
    }

    private void SettingsManager_OnTacticalViewEnabled(object sender, EventArgs e) {
        tacticalView = true;
        tacticalViewVirtualCamera.enabled = tacticalView;
    }

    private void SettingsManager_OnTacticalViewDisabled(object sender, EventArgs e) {
        tacticalView = false;
        tacticalViewVirtualCamera.enabled = tacticalView;
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

        worldIconScaleMultiplier = mainVirtualCamera.m_Lens.OrthographicSize/25f;
        OnCameraZoomedChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetCameraOrthoSize() {
        return mainVirtualCamera.m_Lens.OrthographicSize;
    }

    public float GetWorldIconScaleMultiplier() {
        return worldIconScaleMultiplier;
    }
}
