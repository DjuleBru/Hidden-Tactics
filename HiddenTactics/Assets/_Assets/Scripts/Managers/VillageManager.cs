using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VillageManager : NetworkBehaviour
{
    [SerializeField] private Transform villagePrefab;

    [SerializeField] private List<Vector2> villagePositionInGridObject;

    private void Start() {
        GenerateVillages();
    }

    private void GenerateVillages() {
        GridSystem battleGrid = BattleGrid.Instance.GetGridSystem();

        for (int x = 0; x < battleGrid.width; x++) {
            for (int y = 0; y < battleGrid.height; y++) {

                GridPosition pos = new GridPosition(x, y);
                GridObjectVisual gridObjectVisual = battleGrid.GetGridObject(pos).GetGridObjectVisual();

                // first and last sprites = settlements
                if (x == 0) {
                    foreach(Vector2 villagePosition in villagePositionInGridObject) {

                        Vector3 villageOffset = new Vector3(villagePosition.x, villagePosition.y, 0);
                        Vector3 villagePositionWorld = gridObjectVisual.transform.position + villageOffset;
                        Village village = Instantiate(villagePrefab, villagePositionWorld, Quaternion.identity).GetComponent<Village>();

                        village.SetVillageOffset(villageOffset);
                        village.SetIPlaceableOwnerClientId(NetworkManager.Singleton.LocalClientId);
                        village.SetIPlaceableBattlefieldOwner();
                        village.SetIPlaceableGridPosition(pos);
                        village.PlaceIPlaceable();
                    }
                    continue;
                }

                if (x == battleGrid.width - 1) {
                    foreach (Vector2 villagePosition in villagePositionInGridObject) {
                        Vector3 villagePositionWorld = gridObjectVisual.transform.position + new Vector3(villagePosition.x, villagePosition.y, 0);
                        Village village = Instantiate(villagePrefab, villagePositionWorld, Quaternion.identity).GetComponent<Village>();

                        village.SetIPlaceableOwnerClientId(NetworkManager.Singleton.LocalClientId);
                        village.SetIPlaceableBattlefieldOwner();
                        village.SetIPlaceableGridPosition(pos);
                        village.PlaceIPlaceable();
                    }
                    continue;
                }
            }
        }
    }
}
