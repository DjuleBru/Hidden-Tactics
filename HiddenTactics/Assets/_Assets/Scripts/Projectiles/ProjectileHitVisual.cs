using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileHitVisual : NetworkBehaviour
{
    [SerializeField] private float projectileVisualLifetime;
    private float projectileVisualTimer;

    private void Awake() {
        projectileVisualTimer = projectileVisualLifetime;
    }

    private void Update() {
        projectileVisualTimer -= Time.deltaTime;
        if(projectileVisualTimer < 0 ) {
            if (IsServer) {
                Destroy(gameObject);
            }
        }
    }

    public void Initialize(Vector3 position) {
        Debug.Log(position);
        transform.position = position;
    }
}
