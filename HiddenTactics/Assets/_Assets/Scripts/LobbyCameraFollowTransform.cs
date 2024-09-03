using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCameraFollowTransform : MonoBehaviour
{
    [SerializeField] private List<Vector3> cameraPositionList;
    [SerializeField] private List<float> cameraZoomList;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float cameraMoveSpeed;
    [SerializeField] private float cameraOrthographicSizeSpeed;
    private Vector3 nextCameraPosition;
    private float nextCameraOrthographicSize;

    private void Awake() {
        nextCameraPosition = transform.position;
        virtualCamera.m_Lens.OrthographicSize = 35f;
        nextCameraOrthographicSize = cameraZoomList[Random.Range(0, cameraZoomList.Count)];
    }

    private void Update() {
        if (Vector3.Distance(transform.position, nextCameraPosition) > 2f) {
            Vector3 moveDirNormalized = (nextCameraPosition - transform.position).normalized;
            transform.position += moveDirNormalized * cameraMoveSpeed * Time.deltaTime;
        } else {
            nextCameraPosition = cameraPositionList[Random.Range(0, cameraPositionList.Count)];
        }

        if (virtualCamera.m_Lens.OrthographicSize - nextCameraOrthographicSize > 0) {
            virtualCamera.m_Lens.OrthographicSize -= cameraOrthographicSizeSpeed  * Time.deltaTime;
        } else {
            virtualCamera.m_Lens.OrthographicSize += cameraOrthographicSizeSpeed * Time.deltaTime;
        }

        if(Mathf.Abs(virtualCamera.m_Lens.OrthographicSize - nextCameraOrthographicSize) <.5f) {
            nextCameraOrthographicSize = cameraZoomList[Random.Range(0, cameraZoomList.Count)];
        }

    }
}
