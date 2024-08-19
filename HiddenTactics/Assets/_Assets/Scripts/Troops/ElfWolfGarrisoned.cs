using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElfWolfGarrisoned : Troop
{
    protected override void BattleManager_OnStateChanged(object sender, EventArgs e) {
        base.BattleManager_OnStateChanged(sender, e);

        if(BattleManager.Instance.IsPreparationPhase()) {
            ActivateNextSpawnedUnit(Vector3.zero);
            ActivateNextSpawnedUnit(Vector3.zero);
        }
    }

    public override void ActivateNextSpawnedUnit(Vector3 spawnPosition) {
        Debug.Log("activating next spawned unit");

        if (spawnedUnitActivatedIndex < spawnedUnitsInTroop.Count) {
            if (spawnPosition != Vector3.zero) {
                spawnedUnitsInTroop[spawnedUnitActivatedIndex].transform.position = spawnPosition;
            }
            spawnedUnitsInTroop[spawnedUnitActivatedIndex].ActivateSpawnedUnit();
            spawnedUnitsInTroop[spawnedUnitActivatedIndex].SetUnitAsSpawnedUnit(false);
            spawnedUnitActivatedIndex++;
        }
        else {
            Debug.LogWarning("no more units to spawn");
        }
    }

    public override void DeactivateAllDynamicallySpawnedUnits() {

    }
}
