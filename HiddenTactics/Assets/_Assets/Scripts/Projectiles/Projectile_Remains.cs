using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Remains : Projectile
{
    [SerializeField] private float projectileMoveSpeedAfterHit;
    [SerializeField] private Rigidbody2D rb;

    protected override void Update() {
        if (projectileHasHit) return;

        if (nextPositionXNormalized < 1) {
            UpdateProjectilePosition();
        } else {
            StartCoroutine(ProjectileHitCoroutine());
            unitAttackOrigin.ProjectileHasHit(target, transform.position);
        }

        UpdateTrajectoryEndPoint();
    }

    private IEnumerator ProjectileHitCoroutine() {
        projectileHasHit = true;

        if (IsServer) {
            if (projectileHitVisualPrefab != null) {
                SpawnProjectileHitVisualGameObjectServerRpc();
            }
        }

        rb.drag = 2f;
        rb.AddForce(projectileMoveDir.normalized * .15f);

        float rotationForce = -700f;
        if (projectileMoveDir.x < 0) {
            // Target is located behind shooted
            rotationForce = -rotationForce;
        }

        rb.AddTorque(rotationForce);
        
        projectileVisual.SetShadowVisualInactive();

        yield return new WaitForSeconds(2.5f);

        if (unitAttackOrigin.IsServer) {
            Destroy(gameObject);
        }
    }
}
