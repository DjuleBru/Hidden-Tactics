using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerGoldManager : NetworkBehaviour {

    public static PlayerGoldManager Instance;

    [SerializeField] private const int playerInitialGold = 20;
    [SerializeField] private const int playerBaseIncome = 10;
    [SerializeField] private const int playerVillageDestroyedBonusIncome = 1;
    [SerializeField] private const float playerSavingsRevenue = .1f;

    [SerializeField] private const int opponentVillageDestroyedBoostGold = 2;
    [SerializeField] private const int playerUnitJumpedBonusGold = 1;
    
    private void Awake() {
        Instance = this;
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        PlayerStateUI.Instance.RefreshPlayerGoldUI(0, playerInitialGold);
        PlayerStateUI.Instance.RefreshPlayerRevenueUI(playerBaseIncome);

        VillageManager.Instance.OnPlayerVillageDestroyed += VillageManager_OnPlayerVillageDestroyed;
        VillageManager.Instance.OnOpponentVillageDestroyed += VillageManager_OnOpponentVillageDestroyed;

        HiddenTacticsMultiplayer.Instance.OnPlayerGoldChanged += HiddenTacticsMultiplayer_OnPlayerGoldChanged;
        HiddenTacticsMultiplayer.Instance.OnPlayerRevenueChanged += HiddenTacticsMultiplayer_OnPlayerRevenueChanged;
    }

    private void HiddenTacticsMultiplayer_OnPlayerGoldChanged(object sender, HiddenTacticsMultiplayer.OnPlayerGoldChangedEventArgs e) {
        if (e.clientId == NetworkManager.Singleton.LocalClientId) {
            PlayerStateUI.Instance.RefreshPlayerGoldUI(e.previousGold, e.newGold);
        }
    }

    private void HiddenTacticsMultiplayer_OnPlayerRevenueChanged(object sender, HiddenTacticsMultiplayer.OnPlayerRevenueChangedEventArgs e) {
        if (e.clientId == NetworkManager.Singleton.LocalClientId) {
            PlayerStateUI.Instance.RefreshPlayerRevenueUI(e.newRevenue);
        }
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (BattleManager.Instance.IsFirstPreparationPhase()) return;

        if(BattleManager.Instance.IsPreparationPhase()) {
            EarnRevenueServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void EarnRevenueServerRpc(ulong clientID) {
        int revenue = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromClientId(clientID).playerRevenue;
        int playerGold = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromClientId(clientID).playerGold;

        HiddenTacticsMultiplayer.Instance.DistributePlayerRevenue();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EarnGoldServerRpc(ulong clientID, int goldAmount) {
        HiddenTacticsMultiplayer.Instance.ChangePlayerGoldServerRpc(clientID, goldAmount);
    }

    public bool CanSpendGold(int goldAmount, ulong clientID) {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromClientId(clientID);

        return playerData.playerGold >= goldAmount;
    }

    public void SpendGold(int goldAmount, ulong clientID) {
        SpendGoldServerRpc(goldAmount, clientID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpendGoldServerRpc(int goldAmount, ulong clientID) {
        HiddenTacticsMultiplayer.Instance.ChangePlayerGoldServerRpc(clientID, -goldAmount);
    }


    private void VillageManager_OnOpponentVillageDestroyed(object sender, EventArgs e) {
        EarnGoldServerRpc(NetworkManager.Singleton.LocalClientId, opponentVillageDestroyedBoostGold);
    }

    private void VillageManager_OnPlayerVillageDestroyed(object sender, EventArgs e) {
    }

    public int GetPlayerGold(ulong clientID) {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromClientId(clientID);
        return playerData.playerGold;
    }

    public int GetPlayerBaseIncome() {
        return playerBaseIncome;
    }

    public int GetPlayerInitialgGold() {
        return playerInitialGold;
    }

    public int GetPlayerVillageDestroyedBonusIncome() {
        return playerVillageDestroyedBonusIncome;
    }

    public int GetOpponentVillageDestroyedBoostGold() {
        return opponentVillageDestroyedBoostGold;
    }

    public int GetPlayerUnitJumpedBonusGold() {
        return playerUnitJumpedBonusGold;
    }

}