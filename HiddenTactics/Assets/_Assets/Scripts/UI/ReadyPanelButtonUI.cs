using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReadyPanelButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private Button button;
    [SerializeField] Image buttonOutlineImage;
    [SerializeField] Animator buttonAnimator;

    [SerializeField] Material readyMaterial;
    [SerializeField] Material unreadyMaterial;

    private void Awake() {
        button = GetComponent<Button>();
        buttonOutlineImage.material = unreadyMaterial;
    }


    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        buttonOutlineImage.material = unreadyMaterial;
    }

    public void HandleButtonClickVisual() {
        if (Player.LocalInstance.GetPlayerReady()) {
            buttonOutlineImage.material = readyMaterial;
        }
        else {
            buttonOutlineImage.material = unreadyMaterial;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        buttonAnimator.SetTrigger("Hovered");
    }

    public void OnPointerExit(PointerEventData eventData) {

    }
}
