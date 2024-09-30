using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileVisual : MonoBehaviour
{

    [SerializeField] private Transform bodyVisual;
    [SerializeField] private Transform shadowVisual;

    private Projectile projectile;
    [SerializeField] private TrailRenderer trailRenderer;
    private Vector3 trajectoryStartPoint;
    private Vector3 trajectoryEndPoint;

    private float newPositionXNormalized;

    private void Awake() {
        projectile = GetComponentInParent<Projectile>();
    }

    private void Start() {
        trajectoryStartPoint = transform.position;
        trajectoryEndPoint = projectile.GetTrajectoryEndPoint();
    }

    private void Update() {
        if (projectile.GetIsPooledProjectile()) return;

        UpdateShadowPosition();
        UpdateProjectileRotation();
    }

    private void UpdateShadowPosition() {
        Vector3 newPosition = transform.position;
        Vector2 trajectoryRange = trajectoryEndPoint - trajectoryStartPoint;

        if(Mathf.Abs(trajectoryRange.normalized.x) < Mathf.Abs(trajectoryRange.normalized.y)) {
            // Curved on X
            newPosition.x = trajectoryStartPoint.x + projectile.GetNextXTrajectoryPosition() / 4 + projectile.GetNextPositionXCorrectionAbsolute();
        } else {
            // Curved on Y
            newPosition.y = trajectoryStartPoint.y + projectile.GetNextYTrajectoryPosition() / 4 + projectile.GetNextPositionYCorrectionAbsolute();

        }

        shadowVisual.position = newPosition;
    }

    private void UpdateProjectileRotation() {
        Vector3 projectileDir = projectile.GetProjectileMoveDir();

        bodyVisual.transform.rotation = LookAtTarget(projectileDir);
        shadowVisual.transform.rotation = LookAtTarget(projectileDir);
    }

    private Quaternion LookAtTarget(Vector2 moveDir) {
        return Quaternion.Euler(0, 0, Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg);
    }

    public void SetShadowVisualInactive() {
        shadowVisual.gameObject.SetActive(false);
    }


    public void InitializeProjectileVisual(ITargetable target) {
        SetProjectileVisualActive();
        if (target is Building) {
            trailRenderer.endColor = new Color(1f, .1f, 0f, 1f);
            trailRenderer.startColor = new Color(1f, .8f, 0f, 1f);
        }
    }

    public void SetProjectileVisualInactive() {
        bodyVisual.gameObject.SetActive(false);
        shadowVisual.gameObject.SetActive(false);
    }

    public void SetProjectileVisualActive() {
        bodyVisual.gameObject.SetActive(true);
        shadowVisual.gameObject.SetActive(true);
    }
}
