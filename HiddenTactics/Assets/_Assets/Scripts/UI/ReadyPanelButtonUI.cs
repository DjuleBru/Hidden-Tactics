using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReadyPanelButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private Button button;
    [SerializeField] Image buttonOutlineImage;

    [SerializeField] Material readyMaterial;
    [SerializeField] Material unreadyMaterial;
    [SerializeField] Color readyColor;
    [SerializeField] Color unreadyColor;

    private void Awake() {
        button = GetComponent<Button>();
        button.GetComponent<Image>().color = unreadyColor;
        buttonOutlineImage.material = unreadyMaterial;
    }


    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        button.GetComponent<Image>().color = unreadyColor;
        buttonOutlineImage.material = unreadyMaterial;
    }

    public void HandleButtonClickVisual() {
        if (Player.LocalInstance.GetPlayerReady()) {
            button.GetComponent<Image>().color = readyColor;
            buttonOutlineImage.material = readyMaterial;
        }
        else {
            button.GetComponent<Image>().color = unreadyColor;
            buttonOutlineImage.material = unreadyMaterial;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {

    }

    public void OnPointerExit(PointerEventData eventData) {

    }
}
