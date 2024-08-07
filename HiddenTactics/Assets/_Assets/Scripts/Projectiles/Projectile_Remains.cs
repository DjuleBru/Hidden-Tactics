using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile_Remains : Projectile
{
    [SerializeField] private float projectileMoveSpeedAfterHit;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator projectileAfterHitAnimator;

    [SerializeField] private float projectileRemainTimeAfterHit;
    [SerializeField] private bool projectileRotatesAfterHit;
    [SerializeField] private bool projectileAnimatedAfterHit;
    [SerializeField] private bool projectileSpawnsUnit;

    protected override void Update() {
        if (projectileHasHit) return;

        UpdateProjectilePosition();
        UpdateTrajectoryEndPoint();
    }

    protected override void ProjectileHasHit() {
        projectileHasHit = true;
        unitAttackOrigin.ProjectileHasHit(target, transform.position);
        StartCoroutine(ProjectileHitCoroutine());
    }

    private IEnumerator ProjectileHitCoroutine() {

        if (IsServer) {
            if (projectileHitVisualPrefab != null) {
                SpawnProjectileHitVisualGameObjectServerRpc();
            }
        }

        if (!keepProjectileVisualOnHit) {
            projectileVisual.gameObject.SetActive(false);
        }

        rb.drag = 5f;

        Vector2 force = projectileMoveDir.normalized * projectileMoveSpeedAfterHit;
        force.y = 0;
        rb.AddForce(force);

       
        if(projectileRotatesAfterHit) {
            float rotationForce = -700f;
            if (projectileMoveDir.x < 0) {
                // Target is located behind shooted
                rotationForce = -rotationForce;
            }
            rb.AddTorque(rotationForce);
        }

        if(projectileAnimatedAfterHit) {
            projectileAfterHitAnimator.SetTrigger("Hit");
        }

        projectileVisual.SetShadowVisualInactive();

        yield return new WaitForSeconds(projectileRemainTimeAfterHit);

        if(projectileSpawnsUnit) {
            unitAttackOrigin.GetComponent<Unit>().GetParentTroop().ActivateNextSpawnedUnit(transform.position);
        }

        if (unitAttackOrigin.IsServer) {
            Destroy(gameObject);
        }
    }
}
