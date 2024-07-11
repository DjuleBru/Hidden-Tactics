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
    [SerializeField] private Image playerIconShadowImage;

    [SerializeField] private TextMeshProUGUI playerGoldText;
    [SerializeField] private TextMeshProUGUI playerGoldChangingText;
    [SerializeField] private TextMeshProUGUI playerGoldChangingValueText;
    [SerializeField] private TextMeshProUGUI playerRevenueText;
    [SerializeField] private TextMeshProUGUI villagesRemainingText;

    [SerializeField] private GameObject playerReadyGameObject;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [SerializeField] private ParticleSystem spendGoldPS;
    [SerializeField] private ParticleSystem earnGoldPS;

    [SerializeField] private Color goldChangingNegativelyColor;
    [SerializeField] private Color goldChangingPositivelyColor;

    private int goldToEarn;
    private float goldEarningPSRate = .15f;
    private float goldEarningPSTimer;

    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image backgroundBorderImage;
    [SerializeField] private Image backgroundBorderShadowImage;

    [SerializeField] private Image playerIconBackgroundImage;
    [SerializeField] private Image playerIconBackgroundShadowImage;
    [SerializeField] private Image playerIconBorderImage;
    [SerializeField] private Image heroIconBackgroundImage;
    [SerializeField] private Image heroIconBackgroundShadowImage;
    [SerializeField] private Image heroIconBorderImage;

    [SerializeField] private Image goldbackgroundBorderImage;
    [SerializeField] private Image villagesbackgroundBorderImage;

    private void Awake() {
        playerReadyGameObject.gameObject.SetActive(false);
        Instance = this;
    }

    private void Update() {
        if (goldToEarn > 0) {

            goldEarningPSTimer += Time.deltaTime;

            if (goldEarningPSTimer > goldEarningPSRate) {
                earnGoldPS.Play();
                goldEarningPSTimer = 0f; 
                goldToEarn--;
            }
        }
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        VillageManager.Instance.OnPlayerVillageDestroyed += VillageManager_OnPlayerVillageDestroyed;
        VillageManager.Instance.OnOpponentVillageDestroyed += VillageManager_OnOpponentVillageDestroyed;

        SetPanelVisuals();
    }


    private void SetPanelVisuals() {
        Deck playerDeck = DeckManager.LocalInstance.GetDeckSelected();

        if (isOpponentPanel) {
            //Get opponent deck
            playerDeck = DeckManager.LocalInstance.GetDeckSelected();
        }

        backgroundImage.sprite = playerDeck.deckFactionSO.panelBackground;
        backgroundBorderImage.sprite = playerDeck.deckFactionSO.panelBackgroundBorder;
        backgroundBorderShadowImage.sprite = playerDeck.deckFactionSO.panelBackgroundBorder;


        playerIconBackgroundImage.sprite = playerDeck.deckFactionSO.slotBackground;
        playerIconBackgroundShadowImage.sprite = playerDeck.deckFactionSO.slotBorder;
        playerIconBorderImage.sprite = playerDeck.deckFactionSO.slotBorder;
        heroIconBackgroundImage.sprite = playerDeck.deckFactionSO.slotBackground;
        heroIconBackgroundShadowImage.sprite = playerDeck.deckFactionSO.slotBorder;
        heroIconBorderImage.sprite = playerDeck.deckFactionSO.panelBackgroundBorder;

        villagesbackgroundBorderImage.sprite = playerDeck.deckFactionSO.panelBackgroundBorder;

        if (!isOpponentPanel) {
            goldbackgroundBorderImage.sprite = playerDeck.deckFactionSO.panelBackgroundBorder;
        }
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

    public void RefreshPlayerGoldUI(int previousGold, int newGold) {
        playerGoldChangingText.gameObject.SetActive(false);
        playerGoldChangingValueText.gameObject.SetActive(false);

        playerGoldText.gameObject.SetActive(true);
        playerGoldText.text = newGold.ToString();

        if (previousGold < newGold) {
            //Player earned gold
            goldToEarn = newGold - previousGold;
        } else {
            //Player spent gold
            spendGoldPS.Stop();
            int goldSpent = previousGold - newGold;
            ParticleSystem.Burst burst = new ParticleSystem.Burst(0, goldSpent);
            spendGoldPS.emission.SetBurst(0, burst);
            spendGoldPS.Play();
        }
    }

    public void RefreshPlayerRevenueUI(int newRevenue) {
        playerRevenueText.text = newRevenue.ToString();
    }

    public void SetPlayerGoldChangingUI(int goldChangeAmount) {

        playerGoldText.gameObject.SetActive(false);
        playerGoldChangingText.gameObject.SetActive(true);
        playerGoldChangingValueText.gameObject.SetActive(true);
        playerGoldChangingText.text = playerGoldText.text;

        if (goldChangeAmount > 0) {
            playerGoldChangingValueText.text = "+" + goldChangeAmount.ToString();
            playerGoldChangingValueText.color = goldChangingPositivelyColor;
        } else {
            playerGoldChangingValueText.text = goldChangeAmount.ToString();
            playerGoldChangingValueText.color = goldChangingNegativelyColor;
        }

    }

    public void ResetPlayerGoldChangingUI() {
        playerGoldChangingText.gameObject.SetActive(false);
        playerGoldChangingValueText.gameObject.SetActive(false);

        playerGoldText.gameObject.SetActive(true);
    }

    public void ShowUnsufficientGoldUI() {

    }

}
