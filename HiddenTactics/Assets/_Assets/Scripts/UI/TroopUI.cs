using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopUI : MonoBehaviour
{
    [SerializeField] GameObject troopSelectedGameObject;
    private Troop troop;

    private void Awake() {
        troopSelectedGameObject.SetActive(false);
        troop = GetComponentInParent<Troop>();
    }

    private void Start() {
        troop.OnTroopSelled += Troop_OnTroopSelled;
    }

    private void Troop_OnTroopSelled(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }

    public void ShowTroopSelectedUI() {
        troopSelectedGameObject.SetActive(true);
    }

    public void HideTroopSelectedUI() {
        troopSelectedGameObject.SetActive(false);
    }
}
