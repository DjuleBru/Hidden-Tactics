using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileVisual : MonoBehaviour
{

    [SerializeField] private Transform bodyVisual;
    [SerializeField] private Transform shadowVisual;

    private Projectile projectile;
    private Vector3 trajectoryStartPoint;
    private Vector3 trajectoryEndPoint;
    private Vector3 oldPosition;

    private float newPositionXNormalized;

    private void Awake() {
        projectile = GetComponentInParent<Projectile>();
    }

    private void Start() {
        trajectoryStartPoint = transform.position;
        trajectoryEndPoint = projectile.GetTrajectoryEndPoint();
    }

    private void Update() {
        UpdateShadowPosition();
        UpdateProjectileRotation();
    }

    private void UpdateShadowPosition() {
        Vector3 newPosition = transform.position + new Vector3(projectile.GetProjectileMoveSpeed() * Time.deltaTime, 0, 0);
        Vector2 trajectoryRange = trajectoryEndPoint - trajectoryStartPoint;

        newPositionXNormalized = (newPosition.x - trajectoryStartPoint.x) / (trajectoryRange.x);
        float newPositionYNormalized = projectile.GetProjectileTrajectoryAnimationCurve().Evaluate(newPositionXNormalized);


        float yDifferentialTargetWithTimeRelative = projectile.GetProjectileYDifferentialWithTargetAnimationCurve().Evaluate(newPositionXNormalized);
        float yDifferentialTargetWithTime = yDifferentialTargetWithTimeRelative * trajectoryRange.y;

        newPosition.y = trajectoryStartPoint.y + (projectile.GetTrajectoryMaxRelativeHeight() * newPositionYNormalized) / 4 + + yDifferentialTargetWithTime;

        shadowVisual.position = newPosition;
    }

    private void UpdateProjectileRotation() {
        Vector3 projectileDir = projectile.GetProjectileMoveDir();

        if(newPositionXNormalized < 0.75) {
            bodyVisual.transform.rotation = LookAtTarget(projectileDir);
            shadowVisual.transform.rotation = LookAtTarget(projectileDir);
        }

    }

    private Quaternion LookAtTarget(Vector2 moveDir) {
        return Quaternion.Euler(0, 0, Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg);
    }

}
