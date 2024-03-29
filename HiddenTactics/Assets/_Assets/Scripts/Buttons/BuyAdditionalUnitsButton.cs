using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyAdditionalUnitsButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] Troop troop;

    private Button buyAdditionalUnitsButton;

    public void OnPointerEnter(PointerEventData eventData) {
        foreach (Unit unit in troop.GetUnitsInAdditionalUnitsInTroopList()) {
            unit.GetUnitVisual().gameObject.SetActive(true);
        }

    }

    public void OnPointerExit(PointerEventData eventData) {
        foreach (Unit unit in troop.GetUnitsInAdditionalUnitsInTroopList()) {
            unit.GetUnitVisual().gameObject.SetActive(false);
        }
    }

    private void Awake() {
        buyAdditionalUnitsButton = GetComponent<Button>();

        buyAdditionalUnitsButton.onClick.AddListener(() => {
            troop.BuyAdditionalUnitsLocally();
        });
    }
}