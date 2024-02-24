using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    [SerializeField] private AnimationCurve projectileTrajectoryAnimationCurve;
    [SerializeField] private AnimationCurve projectileSpeedAnimationCurve;
    [SerializeField] private AnimationCurve projectileYDifferentialWithTargetAnimationCurve;

    [SerializeField] private GameObject projectileVisual;

    public event EventHandler OnProjectileHit;

    private float trajectoryMaxRelativeHeight;
    [SerializeField] private float projectileMaxMoveSpeed;
    [SerializeField] private float projectileTrajectoryYCurve = .2f;

    private float projectileMoveSpeed;

    private Unit targetUnit;

    private Transform targetTransform;
    private UnitAttack unitAttackOrigin;
    private Vector2 trajectoryStartPoint;
    private Vector2 trajectoryEndPointRandomized;
    private Vector3 trajectoryEndPointRandomOffset;
    private float trajectoryEndPointRandomOffsetValue = .5f;
    private Vector3 newPosition;

    private Vector3 projectileMoveDir;

    private float targetPositionUpdateTime = .2f;
    private float targetPositionUpdateTimer;
    private float newPositionXNormalized;

    private bool projectileHasHit;

    private void Start() {
        trajectoryStartPoint = transform.position;

        projectileMoveSpeed = projectileMaxMoveSpeed;
    }

    private void Update() {
        if (projectileHasHit) return;

        if (newPositionXNormalized < 1) {
            UpdateNewPosition();
        } else {
            projectileHasHit = true;
            StartCoroutine(DestroyProjectile());
            unitAttackOrigin.ProjectileHasHit(targetUnit);
        }

        UpdateTrajectoryEndPoint();
    }

    private void UpdateTrajectoryEndPoint() {
        targetPositionUpdateTimer -= Time.deltaTime;
        if (targetPositionUpdateTimer < 0) {
            targetPositionUpdateTimer = targetPositionUpdateTime;
            trajectoryEndPointRandomized = targetTransform.position + new Vector3 (trajectoryEndPointRandomOffset.x, trajectoryEndPointRandomOffset.y, 0);
        }
    }

    public void Initialize(UnitAttack unitAttackOrigin, Unit targetUnit) {
        this.targetUnit = targetUnit;
        this.unitAttackOrigin = unitAttackOrigin;
        targetTransform = targetUnit.GetProjectileTarget();

        transform.position = unitAttackOrigin.GetProjectileSpawnPointPosition();

        trajectoryEndPointRandomOffset = new Vector3(UnityEngine.Random.Range(-trajectoryEndPointRandomOffsetValue, trajectoryEndPointRandomOffsetValue), UnityEngine.Random.Range(-trajectoryEndPointRandomOffsetValue, trajectoryEndPointRandomOffsetValue), 0);
        trajectoryEndPointRandomized = targetTransform.position + trajectoryEndPointRandomOffset;

        float distanceToTarget = Mathf.Abs(trajectoryEndPointRandomized.x - transform.position.x);
        trajectoryMaxRelativeHeight = distanceToTarget * projectileTrajectoryYCurve;
    }

    private void CalculateNewProjectileMoveSpeed(float newPositionXNormalized) {
        float projectileMoveSpeedNormalized = projectileSpeedAnimationCurve.Evaluate(newPositionXNormalized);
        projectileMoveSpeed = projectileMaxMoveSpeed * projectileMoveSpeedNormalized;
    }

    private void UpdateNewPosition() {
        Vector2 trajectoryRange = trajectoryEndPointRandomized - trajectoryStartPoint;
        if(trajectoryRange.x < 0) {
            // Target is located behind shooted
            projectileMoveSpeed = -projectileMoveSpeed;
        }
        newPosition = transform.position + new Vector3(projectileMoveSpeed * Time.deltaTime, 0, 0);

        newPositionXNormalized = (newPosition.x - trajectoryStartPoint.x) / (trajectoryRange.x);
        float newPositionYNormalized = projectileTrajectoryAnimationCurve.Evaluate(newPositionXNormalized);

        float trajectoryYWithTime = trajectoryMaxRelativeHeight * newPositionYNormalized;

        float yDifferentialTargetWithTimeRelative = projectileYDifferentialWithTargetAnimationCurve.Evaluate(newPositionXNormalized);
        float yDifferentialTargetWithTime = yDifferentialTargetWithTimeRelative * trajectoryRange.y;

        newPosition.y = trajectoryStartPoint.y + trajectoryYWithTime + yDifferentialTargetWithTime;

        projectileMoveDir = newPosition - transform.position;

        CalculateNewProjectileMoveSpeed(newPositionXNormalized);
        transform.position = newPosition;
    }

    private IEnumerator DestroyProjectile() {
        projectileVisual.SetActive(false);
        yield return new WaitForSeconds(.5f);

        if(unitAttackOrigin.IsServer) {
            Destroy(gameObject);
        }
    }

    public Vector2 GetTrajectoryEndPoint() {
        return trajectoryEndPointRandomized;
    }

    public float GetProjectileMoveSpeed() { 
        return projectileMoveSpeed; 
    }

    public float GetTrajectoryMaxRelativeHeight() {
        return trajectoryMaxRelativeHeight;
    }

    public AnimationCurve GetProjectileTrajectoryAnimationCurve() { 
        return projectileTrajectoryAnimationCurve; 
    }

    public AnimationCurve GetProjectileYDifferentialWithTargetAnimationCurve() {
        return projectileYDifferentialWithTargetAnimationCurve;
    }

    public Vector3 GetProjectileMoveDir() {
        return projectileMoveDir;
    }

}
