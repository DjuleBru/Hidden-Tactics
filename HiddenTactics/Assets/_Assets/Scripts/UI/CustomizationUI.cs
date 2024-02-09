using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizationUI : MonoBehaviour
{
    [SerializeField] Button closeButton;

    public static CustomizationUI Instance { get; private set; }

    [SerializeField] private TMP_InputField playerNameInputField;
    private PlayerIconSelectSingleUI[] playerIcons;

    private void Awake() {
        Instance = this;

        closeButton.onClick.AddListener(() => {
            Hide();
        });
    }

    private void Start() {
        playerIcons = GetComponentsInChildren<PlayerIconSelectSingleUI>();

        playerNameInputField.onValueChanged.AddListener((string newText) => {
            HiddenTacticsMultiplayer.Instance.SetPlayerName(newText);
        });

        Hide();

    }

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    public PlayerIconSelectSingleUI[] GetPlayerIconsArray() {
        return playerIcons;
    }
}
