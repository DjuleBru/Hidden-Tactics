using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCameraManager : MonoBehaviour
{
    public static MainMenuCameraManager Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera mainMenuCamera;
    [SerializeField] private CinemachineVirtualCamera editBattlefieldGridTilesCamera;
    [SerializeField] private CinemachineVirtualCamera editBattlefieldVillagesCamera;
    [SerializeField] private CinemachineVirtualCamera editBattlefieldBaseCamera;
    [SerializeField] private CinemachineVirtualCamera editBattlefieldCamera;
    [SerializeField] private CinemachineVirtualCamera editDeckCamera;
    [SerializeField] private CinemachineVirtualCamera fadeInCamera;

    [SerializeField] private CinemachineBrain brain;

    private void Awake() {
        Instance = this;

        SetBaseCamera();
    }

    public void SetEditBattlefieldCamera()
    {
        mainMenuCamera.enabled = false;
        editBattlefieldGridTilesCamera.enabled = false;
        editBattlefieldVillagesCamera.enabled = false;
        editBattlefieldBaseCamera.enabled = false;
        editDeckCamera.enabled = false;
        editBattlefieldCamera.enabled = true;
        fadeInCamera.enabled = false;
    }

    public void SetEditBattlefieldGridTilesCamera() {
        mainMenuCamera.enabled = false;
        editBattlefieldGridTilesCamera.enabled = true;
        editBattlefieldVillagesCamera.enabled = false;
        editBattlefieldBaseCamera.enabled = false;
        editDeckCamera.enabled = false;
        editBattlefieldCamera.enabled = false;
        fadeInCamera.enabled = false;
    }

    public void SetEditBattlefieldVillagesCamera() {
        mainMenuCamera.enabled = false;
        editBattlefieldGridTilesCamera.enabled = false;
        editBattlefieldVillagesCamera.enabled = true;
        editBattlefieldBaseCamera.enabled = false;
        editDeckCamera.enabled = false;
        editBattlefieldCamera.enabled = false;
        fadeInCamera.enabled = false;
    }

    public void SetEditBattlefieldBaseCamera() {
        mainMenuCamera.enabled = false;
        editBattlefieldGridTilesCamera.enabled = false;
        editBattlefieldVillagesCamera.enabled = false;
        editBattlefieldBaseCamera.enabled = true;
        editDeckCamera.enabled = false;
        editBattlefieldCamera.enabled = false;
        fadeInCamera.enabled = false;
    }

    public void SetBaseCamera() {
        mainMenuCamera.enabled = true;
        editBattlefieldGridTilesCamera.enabled = false;
        editBattlefieldVillagesCamera.enabled = false;
        editBattlefieldBaseCamera.enabled = false;
        editDeckCamera.enabled = false;
        editBattlefieldCamera.enabled = false;
        fadeInCamera.enabled = false;
    }


    public void SetEditDeckCamera()
    {
        mainMenuCamera.enabled = false;
        editBattlefieldGridTilesCamera.enabled = false;
        editBattlefieldVillagesCamera.enabled = false;
        editBattlefieldBaseCamera.enabled = false;
        editDeckCamera.enabled = true;
        editBattlefieldCamera.enabled = false;
        fadeInCamera.enabled = false;
    }

    public void SetFadeInCamera() {
        mainMenuCamera.enabled = false;
        editBattlefieldGridTilesCamera.enabled = false;
        editBattlefieldVillagesCamera.enabled = false;
        editBattlefieldBaseCamera.enabled = false;
        editDeckCamera.enabled = false;
        editBattlefieldCamera.enabled = false;
        fadeInCamera.enabled = true;
    }
}
