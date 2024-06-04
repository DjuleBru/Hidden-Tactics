using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button exitGameButton;


    private void Awake() {
        exitGameButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }


}
