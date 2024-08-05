using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OpenClosePanelButton_Detail : OpenClosePanelButton, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Image panelOutline;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material hoveringMaterial;
    [SerializeField] private Animator panelAnimator;

    protected override void TaskOnClick() {

    }

    public void OnPointerEnter(PointerEventData eventData) {
        panel.gameObject.SetActive(true);
        panelOutline.material = hoveringMaterial;
        panelBackground.material = hoveringMaterial;
        //panelAnimator.SetTrigger("Open");
    }

    public void OnPointerExit(PointerEventData eventData) {
        panelAnimator.SetTrigger("Close");
        panelOutline.material = cleanMaterial;
        panelBackground.material = cleanMaterial;
        StartCoroutine(ClosePanel());
    }

    private IEnumerator ClosePanel() {
        yield return new WaitForSeconds(0.15f);
        panel.gameObject.SetActive(false);
    }

}
