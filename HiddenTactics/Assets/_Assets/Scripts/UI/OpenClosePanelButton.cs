using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenClosePanelButton : MonoBehaviour
{
    [SerializeField] protected GameObject panel;
    [SerializeField] protected bool startClosed;
    protected bool panelOpen;
    protected Button button;

    protected void Awake() {
        button = GetComponent<Button>();
        if(startClosed) {
            panel.gameObject.SetActive(false);
        } else {
            panel.gameObject.SetActive(true);
        }
    }

    protected void Start() {
        button.onClick.AddListener(TaskOnClick);
    }

    protected virtual void TaskOnClick() {
        panelOpen = !panelOpen;
        panel.gameObject.SetActive(panelOpen);
    }
}
