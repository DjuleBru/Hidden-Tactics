using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OpenClosePanelButton_Detail : OpenClosePanelButton {

    [SerializeField] private TextMeshProUGUI plusText;
    protected override void TaskOnClick() {
        panelOpen = !panelOpen;
        panel.gameObject.SetActive(panelOpen);

        if(panelOpen) {
            plusText.text = "-";
        } else {
            plusText.text = "+";
        }
    }
}
