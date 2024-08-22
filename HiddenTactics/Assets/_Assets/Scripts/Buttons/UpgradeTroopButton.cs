using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeTroopButton : TroopButtonUI, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] Troop troop;

    private Button upgradeTroopButton;

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
    }

    private void Awake() {
        upgradeTroopButton = GetComponent<Button>();

        upgradeTroopButton.onClick.AddListener(() => {
            troop.UpgradeTroop();
        });
    }
}
