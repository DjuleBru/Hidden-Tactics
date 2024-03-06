using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildingHP : MonoBehaviour, IDamageable {

    private Building building;
    private float buildingHP;

    public event EventHandler<OnHealthChangedEventArgs> OnHealthChanged;

    public class OnHealthChangedEventArgs : EventArgs {
        public float previousHealth;
        public float newHealth;
    }

    private void Awake() {
        building = GetComponent<Building>();

        buildingHP = building.GetBuildingSO().buildingHP;
    }

    public float GetHP() {
        return buildingHP;
    }

    public void TakeDamage(float damage) {
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void TakeDamageServerRpc(float damage) {
        TakeDamageClientRpc(damage);
    }

    [ClientRpc]
    protected virtual void TakeDamageClientRpc(float damage) {
        buildingHP -= damage;

        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs {
            previousHealth = buildingHP + damage,
            newHealth = buildingHP
        });

        if (buildingHP <= 0) {
            building.Die();
        }
    }

    public float GetMaxHP() {
        return building.GetBuildingSO().buildingHP;
    }



}
