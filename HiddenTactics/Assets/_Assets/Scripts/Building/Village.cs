using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Village : Building {
    private Vector3 villageOffset;

    protected override void Update() {
        if(battlefieldOwner != null && battlefieldOffset != null && villageOffset != null) {
            transform.position = battlefieldOwner.position + battlefieldOffset + villageOffset;
        }
    }

    public void SetVillageOffset(Vector3 villageOffset) {
        this.villageOffset = villageOffset;
    }

    public override void SetIPlaceableGridPosition(GridPosition iPlaceableGridPosition) {
        currentGridPosition = iPlaceableGridPosition;
        transform.position = BattleGrid.Instance.GetWorldPosition(currentGridPosition);
    }

    public override void PlaceIPlaceable() {

        isPlaced = true;
        BattleGrid.Instance.AddIPlaceableAtGridPosition(currentGridPosition, this);

        // Set placed troop on grid object
        BattleGrid.Instance.SetIPlaceableSpawnedAtGridPosition(this, currentGridPosition);
        SetIPlaceableGridPosition(currentGridPosition);
        battlefieldOffset = transform.position - battlefieldOwner.transform.position;
    }

    public override void Die() {
        isDestroyed = true;
        GetComponent<Collider2D>().enabled = false;
        VillageManager.Instance.SetVillageDestroyed(NetworkManager.Singleton.LocalClientId);
        BattleGrid.Instance.RemoveIPlaceableAtGridPosition(BattleGrid.Instance.GetGridPosition(transform.position), this);
    }
}
