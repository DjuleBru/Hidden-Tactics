using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTroopButton : MonoBehaviour
{
    [SerializeField] Troop troop;

    private Button upgradeTroopButton;

    private void Awake() {
        upgradeTroopButton = GetComponent<Button>();

        upgradeTroopButton.onClick.AddListener(() => {
            troop.UpgradeTroop();
        });
    }
}
