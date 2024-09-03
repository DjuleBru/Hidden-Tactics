using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForServerUI : MonoBehaviour
{
    public static WaitingForServerUI Instance;
    private void Awake() {
        Instance = this;
        Hide();
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
