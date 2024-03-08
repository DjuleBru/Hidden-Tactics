using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopUI : MonoBehaviour
{
    [SerializeField] GameObject troopSelectedGameObject;

    private void Awake() {
        troopSelectedGameObject.SetActive(false);
    }

    public void ShowTroopSelectedUI() {
        troopSelectedGameObject.SetActive(true);
    }

    public void HideTroopSelectedUI() {
        troopSelectedGameObject.SetActive(false);
    }
}
