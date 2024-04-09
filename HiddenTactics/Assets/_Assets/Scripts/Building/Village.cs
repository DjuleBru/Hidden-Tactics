using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : Building
{
    private Vector3 villageOffset;

    public override void HandleIPlaceablePosition() {
        transform.position = battlefieldOwner.position + battlefieldOffset + villageOffset;
    }

    public void SetVillageOffset(Vector3 villageOffset) {
        this.villageOffset = villageOffset;
    }
}
