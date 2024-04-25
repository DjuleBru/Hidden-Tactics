using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VillageManager : NetworkBehaviour {
    public static VillageManager Instance { get; private set; }

    public event EventHandler OnPlayerVillageDestroyed;
    public event EventHandler OnOpponentVillageDestroyed;

    private int playerVillages = 20;
    private int opponentVillages = 20;

    [SerializeField] private Transform villagePrefab;
    [SerializeField] private Transform playerGridOrigin;
    [SerializeField] private Transform opponentGridOrigin;
    [SerializeField] private List<Vector2> villagePositionInGridObject;

    private void Awake() {
        Instance = this;
    }

    [ServerRpc(RequireOwnership =false)]
    public void GeneratePlayerVillagesServerRpc(ulong clientId) {
        GridSystem battleGrid = BattleGrid.Instance.GetGridSystem();

        for (int x = 0; x < battleGrid.width; x++) {
            for (int y = 0; y < battleGrid.height; y++) {
                Vector2Int villageGridPositionVector = new Vector2Int(x, y);
                GridPosition pos = new GridPosition(villageGridPositionVector.x, villageGridPositionVector.y);
                GridObjectVisual gridObjectVisual = battleGrid.GetGridObject(pos).GetGridObjectVisual();

                // first and last sprites = settlements
                if (x == 0) {
                    foreach(Vector2 villagePosition in villagePositionInGridObject) {

                        Vector3 villageOffset = new Vector3(villagePosition.x, villagePosition.y, 0);
                        Village village = Instantiate(villagePrefab).GetComponent<Village>();
                        
                        NetworkObject villageNetworkObject = village.gameObject.GetComponent<NetworkObject>();
                        villageNetworkObject.Spawn(true);

                        SetVillageDataClientRpc(villageNetworkObject, clientId, villageGridPositionVector, villageOffset);
                    }
                    continue;
                }
            }
        }
    }

    [ClientRpc]
    private void SetVillageDataClientRpc(NetworkObjectReference villageNetworkObjectReference, ulong clientID, Vector2Int villageGridPosition, Vector3 villageOffset) {

        GridPosition pos = new GridPosition(villageGridPosition.x, villageGridPosition.y);

        // Reverse X symmetry 
        if(clientID != NetworkManager.Singleton.LocalClientId) {
            pos = BattleGrid.Instance.TranslateOpponentGridPosition(pos);

            if(villageOffset.x == -4) {
                villageOffset.x = .25f;
            } else {
                villageOffset.x = -4;
            }
        }

        villageNetworkObjectReference.TryGet(out NetworkObject villageNetworkObject);
        Village village = villageNetworkObject.GetComponent<Village>();

        village.SetVillageOffset(villageOffset);
        village.SetIPlaceableOwnerClientId(clientID);
        village.SetIPlaceableBattlefieldOwner();
        village.SetIPlaceableGridPosition(pos);
        village.PlaceIPlaceable();
    }


    public void SetVillageDestroyed(ulong clientID) {
        if (!IsServer) return;
        SetVillageDestroyedServerRpc(clientID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetVillageDestroyedServerRpc(ulong clientID) {
        HiddenTacticsMultiplayer.Instance.RemoveOnePlayerVillage(clientID);

        SetVillageDestroyedClientRpc(clientID);
    }

    [ClientRpc]
    public void SetVillageDestroyedClientRpc(ulong clientID) {
        if(clientID == NetworkManager.Singleton.LocalClientId) {
            playerVillages--;
            OnPlayerVillageDestroyed?.Invoke(this, EventArgs.Empty);

        } else {
            opponentVillages--;
            OnOpponentVillageDestroyed.Invoke(this, EventArgs.Empty);
        }
    }

    public int GetPlayerVillageNumber() {
        return playerVillages;
    }

    public int GetOpponentVillageNumber() {
        return opponentVillages;
    }
}
