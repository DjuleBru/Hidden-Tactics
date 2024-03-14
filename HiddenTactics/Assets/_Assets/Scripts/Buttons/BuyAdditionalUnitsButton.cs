using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BuyAdditionalUnitsButton : MonoBehaviour {

    [SerializeField] Troop troop;

    private Button buyAdditionalUnitsButton;

    private void Awake() {
        buyAdditionalUnitsButton = GetComponent<Button>();

        buyAdditionalUnitsButton.onClick.AddListener(() => {
            troop.BuyAdditionalUnit();
        });
    }
}