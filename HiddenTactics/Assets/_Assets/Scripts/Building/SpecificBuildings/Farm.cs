using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : Building
{
    [SerializeField] private float psDelayBetweenParticles;
    [SerializeField] private string buildingName;
    [SerializeField] private ParticleSystem goldPS;

    protected override void Start() {
        base.Start();
        if (GetBuildingIsOnlyVisual()) return;
        OnBuildingPlaced += Farm_OnBuildingPlaced;
        OnBuildingDestroyed += Farm_OnBuildingDestroyed;
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }


    protected override void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        base.BattleManager_OnStateChanged(sender, e);
        if (BattleManager.Instance.IsPreparationPhase()) {
            StartCoroutine(ShowGoldEarned());
        }
    }

    private IEnumerator ShowGoldEarned() {
        for (int i = 0; i < buildingSO.economicalBuildingRevenue; i++) {
            goldPS.Play();
            yield return new WaitForSeconds(psDelayBetweenParticles);
        }
    }

    private void Farm_OnBuildingDestroyed(object sender, System.EventArgs e) {
        if (isOwnedByPlayer) {
            RevenueDetailPanelUI.Instance.RemoveRevenueElement(buildingName, buildingSO.economicalBuildingRevenue);
            HiddenTacticsMultiplayer.Instance.ChangePlayerRevenueServerRpc(ownerClientId, -buildingSO.economicalBuildingRevenue);
        }
    }

    private void Farm_OnBuildingPlaced(object sender, System.EventArgs e) {
        if (isOwnedByPlayer) {
            RevenueDetailPanelUI.Instance.AddRevenueElement(buildingName, buildingSO.economicalBuildingRevenue);
            HiddenTacticsMultiplayer.Instance.ChangePlayerRevenueServerRpc(ownerClientId, buildingSO.economicalBuildingRevenue);
        }
    }

    public override void OnDestroy() {
        if(!GetBuildingIsOnlyVisual()) {
            BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
        }
        base.OnDestroy();
    }
}
