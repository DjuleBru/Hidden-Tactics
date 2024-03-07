using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{

    [SerializeField] protected AnimationCurve projectileTrajectoryAnimationCurve;
    [SerializeField] protected AnimationCurve projectileSpeedAnimationCurve;
    [SerializeField] protected AnimationCurve projectileYDifferentialWithTargetAnimationCurve;

    [SerializeField] protected ProjectileVisual projectileVisual;
    [SerializeField] protected GameObject projectileHitVisualPrefab;

    protected float trajectoryMaxRelativeHeight;
    [SerializeField] protected float projectileMaxMoveSpeed;
    [SerializeField] protected float projectileTrajectoryYCurve = .2f;

    protected float projectileMoveSpeed;

    protected ITargetable target;

    protected Transform targetTransform;
    protected UnitAttack unitAttackOrigin;
    protected Vector2 trajectoryStartPoint;
    protected Vector2 trajectoryEndPointRandomized;
    protected Vector3 trajectoryEndPointRandomOffset;
    protected float trajectoryEndPointRandomOffsetValue = .5f;
    protected Vector3 newPosition;

    protected Vector3 projectileMoveDir;

    protected float targetPositionUpdateTime = .2f;
    protected float targetPositionUpdateTimer;
    protected float newPositionXNormalized;

    protected bool projectileHasHit;

    protected void Start() {
        trajectoryStartPoint = transform.position;

        projectileMoveSpeed = projectileMaxMoveSpeed;
    }

    protected virtual void Update() {
        if (projectileHasHit) return;

        if (newPositionXNormalized < 1) {
            UpdateNewPosition();
        } else {
            projectileHasHit = true;
            StartCoroutine(DestroyProjectile());
            unitAttackOrigin.ProjectileHasHit(target, transform.position);
        }

        UpdateTrajectoryEndPoint();
    }

    protected void UpdateTrajectoryEndPoint() {
        targetPositionUpdateTimer -= Time.deltaTime;
        if (targetPositionUpdateTimer < 0) {
            targetPositionUpdateTimer = targetPositionUpdateTime;
            trajectoryEndPointRandomized = targetTransform.position + new Vector3 (trajectoryEndPointRandomOffset.x, trajectoryEndPointRandomOffset.y, 0);
        }
    }

    public void Initialize(UnitAttack unitAttackOrigin, ITargetable target) {
        this.target = target;
        this.unitAttackOrigin = unitAttackOrigin;
        targetTransform = target.GetProjectileTarget();

        transform.position = unitAttackOrigin.GetProjectileSpawnPointPosition();

        trajectoryEndPointRandomOffset = new Vector3(UnityEngine.Random.Range(-trajectoryEndPointRandomOffsetValue, trajectoryEndPointRandomOffsetValue), UnityEngine.Random.Range(-trajectoryEndPointRandomOffsetValue, trajectoryEndPointRandomOffsetValue), 0);
        trajectoryEndPointRandomized = targetTransform.position + trajectoryEndPointRandomOffset;

        float distanceToTarget = Mathf.Abs(trajectoryEndPointRandomized.x - transform.position.x);
        trajectoryMaxRelativeHeight = distanceToTarget * projectileTrajectoryYCurve;

        projectileVisual.InitializeProjectileVisual(target);
    }

    protected void CalculateNewProjectileMoveSpeed(float newPositionXNormalized) {
        float projectileMoveSpeedNormalized = projectileSpeedAnimationCurve.Evaluate(newPositionXNormalized);
        projectileMoveSpeed = projectileMaxMoveSpeed * projectileMoveSpeedNormalized;
    }

    protected void UpdateNewPosition() {
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

    protected IEnumerator DestroyProjectile() {
        if(IsServer) {
            if(projectileHitVisualPrefab != null) {
                SpawnProjectileHitVisualGameObjectServerRpc();
            }
        }
        projectileVisual.gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);

        if(IsServer) {
            Destroy(gameObject);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    protected void SpawnProjectileHitVisualGameObjectServerRpc() {

        GameObject projectileHitVisualGameObject = Instantiate(projectileHitVisualPrefab);
        NetworkObject projectileHitVisualGameObjectNetworkObject = projectileHitVisualGameObject.GetComponent<NetworkObject>();

        projectileHitVisualGameObjectNetworkObject.Spawn(true);

        SetProjectileHitVisualGameObjectPositionClientRpc(projectileHitVisualGameObjectNetworkObject);
    }


    [ClientRpc]
    protected void SetProjectileHitVisualGameObjectPositionClientRpc(NetworkObjectReference projectileHitVisualGameObjectNetworkObjectReference) {
        projectileHitVisualGameObjectNetworkObjectReference.TryGet(out NetworkObject projectileHitVisualGameObjectNetworkObject);
        ProjectileHitVisual projectileHitVisual = projectileHitVisualGameObjectNetworkObject.GetComponent<ProjectileHitVisual>();

        projectileHitVisual.Initialize(transform.position);
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

    public GameObject GetProjectileHitVisualGameObject() {
        return projectileHitVisualPrefab;
    }

}
