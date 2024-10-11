using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitPositionsSyncManager : NetworkBehaviour
{
    private Dictionary<ulong, Unit> unitsPlacedOnBattlefield = new Dictionary<ulong, Unit>();
    private float unitSyncRate = .2f;
    private float unitSyncTimer;


    private void Start() {
        Unit.OnAnyUnitPlaced += Unit_OnAnyUnitPlaced;
    }

    private void Update() {
        if (!IsServer) return;
        if (!BattleManager.Instance.IsBattlePhase()) return;

        unitSyncTimer -= Time.deltaTime;
        if (unitSyncTimer < 0) {
            unitSyncTimer = unitSyncRate;
            HandleAllUnitSync();
        }

    }

    private void HandleAllUnitSync() {
        int remainingUnitsToSync = unitsPlacedOnBattlefield.Count;
        int packetSize = 10;

        Vector2[] allUnitPositions = new Vector2[0];
        ulong[] allUnitUlongs = new ulong[0];

        if (remainingUnitsToSync >= packetSize) {
            allUnitPositions = new Vector2[packetSize];
            allUnitUlongs = new ulong[packetSize];
        } else {
            allUnitPositions = new Vector2[unitsPlacedOnBattlefield.Count];
            allUnitUlongs = new ulong[unitsPlacedOnBattlefield.Count];
        }

        int i = 0;
        int j = 1;
        int k = 1;

        foreach(Unit unit in unitsPlacedOnBattlefield.Values) {

            allUnitPositions[i] = unit.transform.position;
            allUnitUlongs[i] = unit.GetComponent<NetworkObject>().NetworkObjectId;

            i++;
            k++;
            if (k > packetSize*j) {
                j++;
                i = 0;
                SyncAllUnitsServerRpc(allUnitPositions, allUnitUlongs);

                remainingUnitsToSync -= packetSize;
                Debug.Log("remainingUnitsToSync " + remainingUnitsToSync);

                if (remainingUnitsToSync >= packetSize) {
                    allUnitPositions = new Vector2[packetSize];
                    allUnitUlongs = new ulong[packetSize];
                }

                else {
                    allUnitPositions = new Vector2[remainingUnitsToSync];
                    allUnitUlongs = new ulong[remainingUnitsToSync];
                }
            }

        }

        SyncAllUnitsServerRpc(allUnitPositions, allUnitUlongs);
    }


    [ServerRpc]
    private void SyncAllUnitsServerRpc(Vector2[] allUnitPositions, ulong[] allUnitsUlongs) {
        SyncAllUnitsClientRpc(allUnitPositions, allUnitsUlongs);
        Debug.Log("SyncAllUnitsServerRpc " + allUnitPositions.Length);
    }

    [ClientRpc]
    private void SyncAllUnitsClientRpc(Vector2[] allUnitPositions, ulong[] allUnitsUlongs) {
        if (IsServer) return;

        for (int i = 0;  i < allUnitsUlongs.Length; i++) {
            Unit unit = GetUnitFromKey(allUnitsUlongs[i]);
            unit.SetPosition(allUnitPositions[i]);
        }
    }

    public ulong GetUnitKey(Unit unit) {

        foreach (ulong keyVar in unitsPlacedOnBattlefield.Keys) {
            if (unitsPlacedOnBattlefield[keyVar] == unit) {
                return keyVar;
            }
        }

        return 0;
    }

    public Unit GetUnitFromKey(ulong unitKey) {
        unitsPlacedOnBattlefield.TryGetValue(unitKey, out Unit unit);

        return unit;
    }

    private void Unit_OnAnyUnitPlaced(object sender, System.EventArgs e) {
        Unit unit = sender as Unit;
        unitsPlacedOnBattlefield.Add(unit.GetComponent<NetworkObject>().NetworkObjectId, unit);
        Debug.Log(unitsPlacedOnBattlefield.Count);
    }
}
