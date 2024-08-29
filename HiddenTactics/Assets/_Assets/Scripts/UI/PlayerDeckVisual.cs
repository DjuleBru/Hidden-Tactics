using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeckVisual : MonoBehaviour
{

    public static PlayerDeckVisual Instance;

    private Vector3 mainMenuDeckVisualPosition;
    private float mainMenuDeckVisualScale;
    [SerializeField] private Vector3 editDeckMenuDeckVisualPosition;
    [SerializeField] private float editDeckDeckVisualScale;

    private void Awake() {
        Instance = this;
        mainMenuDeckVisualPosition = transform.position;
        mainMenuDeckVisualScale = transform.localScale.x;
    }
    public void SetMainMenuVisual() {
        transform.position = mainMenuDeckVisualPosition;
        transform.localScale = new Vector3(mainMenuDeckVisualScale, mainMenuDeckVisualScale, mainMenuDeckVisualScale);
    }

    public void SetEditDeckMenuVisual() {
        transform.position = editDeckMenuDeckVisualPosition;
        transform.localScale = new Vector3(editDeckDeckVisualScale, editDeckDeckVisualScale, editDeckDeckVisualScale);
    }
}
