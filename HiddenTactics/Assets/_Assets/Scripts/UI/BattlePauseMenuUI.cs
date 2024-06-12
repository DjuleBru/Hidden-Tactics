using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button surrenderButton;
    [SerializeField] private TextMeshProUGUI surrenderText;
    [SerializeField] private Button closeMenuButton;

    private bool menuActive;
    private bool tryingToSurrender;

    private void Awake() {
        pauseMenuPanel.SetActive(false);
        surrenderButton.onClick.AddListener(() => {
            TrySurrender();
        });
        closeMenuButton.onClick.AddListener(() => {
            ClosePauseMenu();
        });
    }

    private void Start() {
        BattleManager.Instance.OnGameEnded += BattleManager_OnGameEnded;
    }

    private void BattleManager_OnGameEnded(object sender, System.EventArgs e) {
        ClosePauseMenu();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menuActive = !menuActive;

            if(menuActive) {
                OpenPauseMenu();
            } else {
                ClosePauseMenu();
            }
        }

        if(menuActive) {
            if(Input.GetMouseButtonDown(1)) {
                CancelTryingToSurrender();
            }
        }

    }

    private void TrySurrender() {
        if(!tryingToSurrender) {
            // First click : confirm surrender needed
            surrenderText.text = "Are you sure ?";
            tryingToSurrender = true;
        } else {
            // Second click : surrender !
            Player.LocalInstance.SetPlayerSurrenders();
            ClosePauseMenu();
        }
    }

    private void CancelTryingToSurrender() {
        surrenderText.text = "Surrender";
        tryingToSurrender = false;
    }

    private void OpenPauseMenu() {
        pauseMenuPanel.SetActive(true);

    }

    private void ClosePauseMenu() {
        tryingToSurrender = false;
        pauseMenuPanel.SetActive(false);
    }
}
