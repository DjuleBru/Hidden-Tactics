using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TroopButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected bool buttonEnabled = true;
    [SerializeField] protected Troop troop;
    [SerializeField] protected Building building;
    protected bool pointerEntered;

    protected virtual void Awake() {
        if(!buttonEnabled) {
            GetComponent<Button>().interactable = false;
        }
    }

    protected void Start() {
        GameInput.Instance.OnRightClickPerformed += GameInput_OnRightClickPerformed;
        if(troop != null) {
            troop.OnTroopSelled += Troop_OnTroopSelled;
        }
        if(building != null) {
            building.OnBuildingSelled += Building_OnBuildingSelled;
        }
    }

    private void Building_OnBuildingSelled(object sender, System.EventArgs e) {
        buttonEnabled = true;
        GetComponent<Button>().interactable = true;
    }

    private void Troop_OnTroopSelled(object sender, System.EventArgs e) {
        buttonEnabled = true;
        GetComponent<Button>().interactable = true;
    }

    protected virtual void GameInput_OnRightClickPerformed(object sender, System.EventArgs e) {
        if (!buttonEnabled) return;
        if (!BattleManager.Instance.IsPreparationPhase()) return;
        CancelButtonShowVisuals();
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        if (!buttonEnabled) return;
        pointerEntered = true;
        animator.SetTrigger("Grow");
        animator.ResetTrigger("Shrink");
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        if (!buttonEnabled) return;
        pointerEntered = false;
        animator.SetTrigger("Shrink");
        animator.ResetTrigger("Grow");
    }

    protected virtual void CancelButtonShowVisuals() {

    }

    protected void OnDestroy() {
        GameInput.Instance.OnRightClickPerformed -= GameInput_OnRightClickPerformed;
    }
} 
