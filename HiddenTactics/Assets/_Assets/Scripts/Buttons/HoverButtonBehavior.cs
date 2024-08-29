using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverButtonBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private List<Image> imagesToColorTransition;
    [SerializeField] private Color unhoveredColor;
    [SerializeField] private Color hoveredColor;
     private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        animator.SetTrigger("Hover");
        foreach(Image image in imagesToColorTransition) {
            image.color = hoveredColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        animator.SetTrigger("Unhover");
        foreach (Image image in imagesToColorTransition) {
            image.color = unhoveredColor;
        }
    }
}
