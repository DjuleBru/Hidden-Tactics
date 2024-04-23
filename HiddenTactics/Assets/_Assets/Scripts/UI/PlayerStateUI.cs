using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : MonoBehaviour {

    public static PlayerStateUI Instance;

    [SerializeField] private bool isOpponentPanel;

    [SerializeField] private Image playerIconImage;

    [SerializeField] private TextMeshProUGUI playerGoldText;
    [SerializeField] private TextMeshProUGUI playerGoldChangingText;
    [SerializeField] private TextMeshProUGUI playerGoldChangingValueText;
    [SerializeField] private TextMeshProUGUI playerRevenueText;
    [SerializeField] private TextMeshProUGUI villagesRemainingText;

    [SerializeField] private GameObject playerReadyGameObject;
    [SerializeField] private TextMeshProUGUI playerNameText;

    private void Awake() {
        playerReadyGameObject.gameObject.SetActive(false);
        Instance = this;
    }

    private void Start() {
        //PlayerData localPlayerData = HiddenTacticsMultiplayer.Instance.GetPlayerData();

        PlayerReadyManager.Instance.OnReadyChanged += PlayerReadyManager_OnReadyChanged;
        PlayerReadyManager.Instance.OnPlayerWantsToSpeedUpChanged += PlayerReadyManager_OnPlayerWantsToSpeedUpChanged;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        VillageManager.Instance.OnPlayerVillageDestroyed += VillageManager_OnPlayerVillageDestroyed;
        VillageManager.Instance.OnOpponentVillageDestroyed += VillageManager_OnOpponentVillageDestroyed;
    }

    private void VillageManager_OnOpponentVillageDestroyed(object sender, System.EventArgs e) {
        if (isOpponentPanel) {
            villagesRemainingText.text = VillageManager.Instance.GetOpponentVillageNumber().ToString();
        }
    }

    private void VillageManager_OnPlayerVillageDestroyed(object sender, System.EventArgs e) {
        if(!isOpponentPanel) {
            villagesRemainingText.text = VillageManager.Instance.GetPlayerVillageNumber().ToString();
        }
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseStarting() | BattleManager.Instance.IsPreparationPhase()) {
            playerReadyGameObject.SetActive(false);
        } 
    }

    private void PlayerReadyManager_OnReadyChanged(object sender, System.EventArgs e) {

        PlayerData playerData = new PlayerData();

        if (!isOpponentPanel) {
            playerData = HiddenTacticsMultiplayer.Instance.GetPlayerData();
        } else {
            playerData = HiddenTacticsMultiplayer.Instance.GetOpponentData();
        }

        playerReadyGameObject.SetActive(PlayerReadyManager.Instance.IsPlayerReady(playerData.clientId));
    }

    private void PlayerReadyManager_OnPlayerWantsToSpeedUpChanged(object sender, System.EventArgs e) {

        PlayerData playerData = new PlayerData();

        if (!isOpponentPanel) {
            playerData = HiddenTacticsMultiplayer.Instance.GetPlayerData();
        }
        else {
            playerData = HiddenTacticsMultiplayer.Instance.GetOpponentData();
        }

        playerReadyGameObject.SetActive(PlayerReadyManager.Instance.PlayerWantingToSpeedUp(playerData.clientId));
    }


    public void RefreshPlayerGoldUI(int previousGold, int newGold) {
        playerGoldChangingText.gameObject.SetActive(false);
        playerGoldChangingValueText.gameObject.SetActive(false);

        playerGoldText.gameObject.SetActive(true);
        playerGoldText.text = newGold.ToString();
    }

    public void SetPlayerGoldChangingUI(int goldChangeAmount) {
        playerGoldText.gameObject.SetActive(false);
        playerGoldChangingText.gameObject.SetActive(true);
        playerGoldChangingValueText.gameObject.SetActive(true);

        playerGoldChangingText.text = playerGoldText.text;
        playerGoldChangingValueText.text = "-" + goldChangeAmount.ToString();
    }

    public void ResetPlayerGoldChangingUI() {
        playerGoldChangingText.gameObject.SetActive(false);
        playerGoldChangingValueText.gameObject.SetActive(false);

        playerGoldText.gameObject.SetActive(true);
    }

    public void ShowUnsufficientGoldUI() {

    }

}
