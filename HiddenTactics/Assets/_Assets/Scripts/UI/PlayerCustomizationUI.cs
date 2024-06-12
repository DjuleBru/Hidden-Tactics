using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCustomizationUI : MonoBehaviour
{
    public static PlayerCustomizationUI Instance { get; private set; }

    [SerializeField] private TMP_InputField playerNameInputField;
    private PlayerIconSelectSingleUI[] playerIcons;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        playerIcons = GetComponentsInChildren<PlayerIconSelectSingleUI>();

        playerNameInputField.onValueChanged.AddListener((string newText) => {
            HiddenTacticsMultiplayer.Instance.SetPlayerName(newText);
        });

        string playerName = SavingManager.Instance.LoadPlayerName();

        playerNameInputField.text = playerName;
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
