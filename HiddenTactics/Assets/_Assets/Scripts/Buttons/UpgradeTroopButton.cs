using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeTroopButton : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] Troop troop;

    private Button upgradeTroopButton;

    public void OnPointerEnter(PointerEventData eventData) {
    }

    public void OnPointerExit(PointerEventData eventData) {
    }

    private void Awake() {
        upgradeTroopButton = GetComponent<Button>();

        upgradeTroopButton.onClick.AddListener(() => {
            troop.UpgradeTroop();
        });
    }
}
