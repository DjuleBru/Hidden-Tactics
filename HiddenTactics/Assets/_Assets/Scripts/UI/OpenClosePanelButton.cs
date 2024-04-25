using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenClosePanelButton : MonoBehaviour
{
    [SerializeField] protected GameObject panel;
    protected bool panelOpen;
    protected Button button;

    protected void Awake() {
        button = GetComponent<Button>();
    }

    protected void Start() {
        button.onClick.AddListener(TaskOnClick);
    }

    protected virtual void TaskOnClick() {
        panelOpen = !panelOpen;
        panel.gameObject.SetActive(panelOpen);
    }
}
