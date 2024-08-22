using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TroopButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Animator animator;

    public virtual void OnPointerEnter(PointerEventData eventData) {
        animator.SetTrigger("Grow");
        animator.ResetTrigger("Shrink");
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        animator.SetTrigger("Shrink");
        animator.ResetTrigger("Grow");
    }
}
