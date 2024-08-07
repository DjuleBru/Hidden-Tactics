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
    [SerializeField] protected bool keepProjectileVisualOnHit;

    protected float projectileMoveSpeed;

    protected ITargetable target;

    protected Transform targetTransform;
    protected UnitAttack unitAttackOrigin;
    protected Vector2 trajectoryRange;
    protected Vector2 trajectoryStartPoint;
    protected Vector2 trajectoryEndPointRandomized;
    protected Vector3 trajectoryEndPointRandomOffset;
    protected float trajectoryEndPointRandomOffsetValue = .5f;
    protected Vector3 nextPosition;

    protected Vector3 projectileMoveDir;

    protected float targetPositionUpdateTime = .2f;
    protected float targetPositionUpdateTimer;
    protected float nextPositionXNormalized;
    protected float nextPositionYNormalized;

    protected float nextYTrajectoryPosition;
    protected float nextXTrajectoryPosition;
    protected float nextPositionYCorrectionAbsolute;
    protected float nextPositionXCorrectionAbsolute;

    protected bool projectileHasHit;

    protected void Start() {
        trajectoryStartPoint = transform.position;
        projectileMoveSpeed = projectileMaxMoveSpeed;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseEnding()) {
            projectileHasHit = true;
            StartCoroutine(DestroyProjectile());
        }
    }

    protected virtual void Update() {
        if (projectileHasHit) return;

        UpdateProjectilePosition();
        UpdateTrajectoryEndPoint();
    }

    protected void UpdateTrajectoryEndPoint() {
        if (targetTransform == null) return;
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

    protected void UpdateProjectilePosition() {
        trajectoryRange = trajectoryEndPointRandomized - trajectoryStartPoint;

        if(Mathf.Abs(trajectoryRange.normalized.x) < Mathf.Abs(trajectoryRange.normalized.y)) {
            // Projectile will be curved on the X axis

            if (trajectoryRange.y < 0) {
                // Target is located behind shooted
                projectileMoveSpeed = -projectileMoveSpeed;
            }

            UpdatePositionXCurve();

        } else {
            // Projectile will be curved on the Y axis

            if (trajectoryRange.x < 0) {
                // Target is located behind shooted
                projectileMoveSpeed = -projectileMoveSpeed;
            }
            UpdatePositionYCurve();
        }
    }

    protected void UpdatePositionYCurve() {
        float nextPositionX = transform.position.x + projectileMoveSpeed * Time.deltaTime;

        nextPositionXNormalized = (nextPositionX - trajectoryStartPoint.x) / (trajectoryRange.x);
        float nextPositionYNormalized = projectileTrajectoryAnimationCurve.Evaluate(nextPositionXNormalized);

        if(nextPositionXNormalized >= .99f) {
            ProjectileHasHit();
            return;
        }

        nextYTrajectoryPosition = nextPositionYNormalized * trajectoryMaxRelativeHeight;

        float nextPositionYCorrectionNormalized = projectileYDifferentialWithTargetAnimationCurve.Evaluate(nextPositionXNormalized);
        nextPositionYCorrectionAbsolute = nextPositionYCorrectionNormalized * trajectoryRange.y;

        float nextPositionY = trajectoryStartPoint.y + nextYTrajectoryPosition + nextPositionYCorrectionAbsolute;

        Vector3 nextPosition = new Vector3(nextPositionX, nextPositionY, 0);

        CalculateNewProjectileMoveSpeed(nextPositionXNormalized);
        projectileMoveDir = nextPosition - transform.position;

        transform.position = nextPosition;
    }

    protected void UpdatePositionXCurve() {

        float nextPositionY = transform.position.y + projectileMoveSpeed * Time.deltaTime;

        nextPositionYNormalized = (nextPositionY - trajectoryStartPoint.y) / (trajectoryRange.y);
        float nextPositionXNormalized = projectileTrajectoryAnimationCurve.Evaluate(nextPositionYNormalized);

        if (nextPositionYNormalized >= .99f) {
            ProjectileHasHit();
            return;
        }

        nextXTrajectoryPosition = nextPositionXNormalized * trajectoryMaxRelativeHeight;

        float nextPositionXCorrectionNormalized = projectileYDifferentialWithTargetAnimationCurve.Evaluate(nextPositionYNormalized);
        nextPositionXCorrectionAbsolute = nextPositionXCorrectionNormalized * trajectoryRange.x;

        if (trajectoryRange.x > 0 && trajectoryRange.y > 0) {
            nextXTrajectoryPosition = -nextXTrajectoryPosition;
        }
        if (trajectoryRange.x < 0 && trajectoryRange.y < 0) {
            nextXTrajectoryPosition = -nextXTrajectoryPosition;
        }

        float nextPositionX = trajectoryStartPoint.x + nextXTrajectoryPosition + nextPositionXCorrectionAbsolute;

        Vector3 nextPosition = new Vector3(nextPositionX, nextPositionY, 0);

        CalculateNewProjectileMoveSpeed(nextPositionYNormalized);
        projectileMoveDir = nextPosition - transform.position;

        transform.position = nextPosition;
    }

    protected virtual void ProjectileHasHit() {
        projectileHasHit = true;
        StartCoroutine(DestroyProjectile());
        unitAttackOrigin.ProjectileHasHit(target, transform.position);
    }

    protected IEnumerator DestroyProjectile() {

        if(IsServer) {
            if(projectileHitVisualPrefab != null) {
                SpawnProjectileHitVisualGameObjectServerRpc();
            }
        }

        if(!keepProjectileVisualOnHit) {
            projectileVisual.SetProjectileVisualInactive();
        }

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

    public float GetNextYTrajectoryPosition() {
        return nextYTrajectoryPosition;
    }
    public float GetNextXTrajectoryPosition() {
        return nextXTrajectoryPosition;
    }
    public float GetNextPositionYCorrectionAbsolute() {
        return nextPositionYCorrectionAbsolute;
    }
    public float GetNextPositionXCorrectionAbsolute() {
        return nextPositionXCorrectionAbsolute;
    }

    public override void OnDestroy() {
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }
}
