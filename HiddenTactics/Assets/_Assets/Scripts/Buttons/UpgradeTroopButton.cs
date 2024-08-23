using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeTroopButton : TroopButtonUI, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] Troop troop;
    [SerializeField] Building building;

    private Button upgradeTroopButton;

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
    }

    protected override void Awake() {
        base.Awake();
        upgradeTroopButton = GetComponent<Button>();

        upgradeTroopButton.onClick.AddListener(() => {
            if(troop != null) {
                troop.UpgradeTroop();
            } else {

            }
        });
    }

}
