using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossfadeTransition : MonoBehaviour
{
    public static CrossfadeTransition Instance;
    private Animator animator;

    private void Awake() {
        Instance = this;
        animator = GetComponent<Animator>();
    }


    private void Start() {

        Debug.Log(" Crossfade Start");
        if (MainMenuUI.Instance == null) {
            FadeOut();
        }
    }

    public void FadeIn() {
        animator.SetTrigger("In");
    }

    public void FadeOut() {
        animator.SetTrigger("Out");
    }
}
