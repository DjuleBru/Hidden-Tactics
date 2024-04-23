using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerGoldManager : NetworkBehaviour {

    public static PlayerGoldManager Instance;

    [SerializeField] private const int playerInitialGold = 20;
    [SerializeField] private int playerBaseIncome = 10;
    [SerializeField] private int playerVillageDestroyedBonusIncome = 1;
    [SerializeField] private float playerSavingsRevenue = .1f;

    [SerializeField] private int opponentVillageDestroyedBoostGold = 2;
    [SerializeField] private int playerUnitJumpedBonusGold = 1;

    public event EventHandler OnPlayerGoldChanged;
    
    private void Awake() {
        Instance = this;
        //PlayerStateUI.Instance.RefreshPlayerGoldUI(0, playerGold.Value);
    }

    private void Start() {
        if (!IsServer) return;
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsPreparationPhase()) {
            EarnRevenueServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void EarnRevenueServerRpc(ulong clientID) {

        int revenue = playerBaseIncome;
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromClientId(clientID);
        playerData.playerGold += revenue;

        EarnRevenueClientRpc(revenue, clientID);
    }

    [ClientRpc]
    private void EarnRevenueClientRpc(int revenue, ulong clientID) {
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
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromClientId(clientID);
        playerData.playerGold -= goldAmount;

        SpendGoldClientRpc(goldAmount, clientID);
    }

    [ClientRpc]
    private void SpendGoldClientRpc(int goldAmount, ulong clientID) {
    }

    public void AddGold(int goldAmount) {
    } 

    private void PlayerGold_OnValueChanged() {
        //PlayerStateUI.Instance.RefreshPlayerGoldUI(previous, current);
    }

    public int GetPlayerGold(ulong clientID) {
        PlayerData playerData = HiddenTacticsMultiplayer.Instance.GetPlayerDataFromClientId(clientID);
        return playerData.playerGold;
    }

}
