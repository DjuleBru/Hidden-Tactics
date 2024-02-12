using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpawnTroopTestButton : MonoBehaviour
{
    [SerializeField] TroopSO troopToSpawnSO;

    private Button spawnTroopButton;

    private void Awake() {
        spawnTroopButton = GetComponent<Button>();

        spawnTroopButton.onClick.AddListener(() => {
            int troopIndex = BattleDataManager.Instance.GetTroopSOIndex(troopToSpawnSO);
            PlayerActionsManager.LocalInstance.SelectTroopToSpawn(troopIndex);
        });
    }

}
