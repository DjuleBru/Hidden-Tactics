using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : Building
{
    [SerializeField] private int farmGoldRevenue;
    [SerializeField] private float psDelayBetweenParticles;
    [SerializeField] private string buildingName;
    [SerializeField] private ParticleSystem goldPS;

    protected override void Start() {
        base.Start();
        OnBuildingPlaced += Farm_OnBuildingPlaced;
        OnBuildingDestroyed += Farm_OnBuildingDestroyed;
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }


    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if (BattleManager.Instance.IsPreparationPhase()) {
            StartCoroutine(ShowGoldEarned());
        }
    }

    private IEnumerator ShowGoldEarned() {
        for (int i = 0; i < farmGoldRevenue; i++) {
            goldPS.Play();
            yield return new WaitForSeconds(psDelayBetweenParticles);
        }
    }

    private void Farm_OnBuildingDestroyed(object sender, System.EventArgs e) {
        if (isOwnedByPlayer) {
            RevenueDetailPanelUI.Instance.RemoveRevenueElement(buildingName, farmGoldRevenue);
            HiddenTacticsMultiplayer.Instance.ChangePlayerRevenueServerRpc(ownerClientId, -farmGoldRevenue);
        }
    }

    private void Farm_OnBuildingPlaced(object sender, System.EventArgs e) {
        if (isOwnedByPlayer) {
            RevenueDetailPanelUI.Instance.AddRevenueElement(buildingName, farmGoldRevenue);
            HiddenTacticsMultiplayer.Instance.ChangePlayerRevenueServerRpc(ownerClientId, farmGoldRevenue);
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }
}
