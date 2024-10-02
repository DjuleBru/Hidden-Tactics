using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitMovement : NetworkBehaviour {

    #region PATH COMPONENTS

    protected Seeker seeker;
    protected Path path;
    protected Vector3 velocity;

    #endregion

    #region PATH PARAMETERS

    // Length of the path
    protected int theGScoreToStopAt = 6000;

    [SerializeField] protected float nextWaypointDistance = .5f;

    protected int currentWaypoint = 0;
    protected bool reachedEndOfPath;

    [SerializeField] protected float pathCalculationRate = .2f;
    protected float pathCalculationTimer = 0;
    #endregion

    private Unit unit;
    private Rigidbody2D rb;
    private CircleCollider2D unitCollider;
    private Vector3 moveDir2D;
    private int moveDirMultiplier;

    private bool dazed;
    private bool canMove;
    private bool inFormation;
    private bool parentTroopSet;

    private float moveSpeed;
    private float moveSpeedMultiplier = 1f;

    private Vector3 destinationPoint;
    private Vector3 moveDir;
    private Vector3 watchDir;
    private Vector3 moveForwardsPoint;

    public event EventHandler OnMoveSpeedBuffed;
    public event EventHandler OnMoveSpeedDebuffed;

    protected float syncMoveDirTimer;
    protected float syncMoveDirRate = .3f;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        unit = GetComponent<Unit>();
        seeker = GetComponent<Seeker>();
        unitCollider = GetComponent<CircleCollider2D>();

        moveSpeed = unit.GetUnitSO().unitMoveSpeed;
    }

    public override void OnNetworkSpawn() {
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        unit.OnUnitReset += Unit_OnUnitReset;
        unit.OnUnitDynamicallySpawned += Unit_OnUnitDynamicallySpawned;
        unit.OnAdditionalUnitActivated += Unit_OnAdditionalUnitActivated;
        unit.OnParentTroopSet += Unit_OnParentTroopSet;
    }

    private void Update() {
        if (!parentTroopSet) return;

        pathCalculationTimer -= Time.deltaTime;
        if (pathCalculationTimer < 0) {
            CheckUnitsInFormation();
        }

        if (!canMove) return;

        if (pathCalculationTimer < 0) {
            Vector3 direction = (destinationPoint - unit.transform.position).normalized;
            Vector3 pathStartPoint = unit.transform.position + direction * unitCollider.radius;

            if(!unit.GetScared()) {
                CalculatePath(pathStartPoint, destinationPoint);
            } else {
                CalculateFleePath(destinationPoint);
            }
        }

        if (path != null) {
            FollowPath(path);
        }
    }

    private void FixedUpdate() {
        if (!canMove) return;

        if (IsServer) {
            if (!dazed) {
                rb.velocity = moveDir * moveSpeed * moveSpeedMultiplier * Time.fixedDeltaTime;
                syncMoveDirTimer += Time.deltaTime;

                if(syncMoveDirTimer >= syncMoveDirRate) {
                    SetMoveDirServerRpc(moveDir);
                    syncMoveDirTimer = 0;
                }

            }
        }
    }

    private void CheckUnitsInFormation() {
        if (Vector3.Distance(transform.position, moveForwardsPoint) < 1.5f) {

            if (!inFormation) {
                inFormation = true;
                SetMoveForwardsPoint();
            }
        }
        else {
            inFormation = false;
        }
    }

    protected void FollowPath(Path path) {
        reachedEndOfPath = false;
        float distanceToWaypoint;

        if (currentWaypoint < path.vectorPath.Count) {
            while (true) {
                distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);

                if (distanceToWaypoint < nextWaypointDistance) {

                    if (currentWaypoint + 1 < path.vectorPath.Count) {
                        currentWaypoint++;
                    }
                    else {
                        reachedEndOfPath = true;
                        break;
                    }
                }
                else {
                    break;
                }
            }
            var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

            moveDir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
            velocity = moveDir * moveSpeed * speedFactor;
        }
    }

    public void CalculatePath(Vector3 startPoint, Vector3 destinationPoint) {
        //Update graph
        Bounds bounds = GetComponent<Collider2D>().bounds;
        bounds.Expand(2f);
        AstarPath.active.UpdateGraphs(bounds);

        if (pathCalculationTimer <= 0) {
            seeker.StartPath(startPoint, destinationPoint, PathComplete);
            pathCalculationTimer = pathCalculationRate;
        }
    }

    public void CalculateFleePath(Vector3 destinationPoint) {
        // Create a path object
        FleePath path = FleePath.Construct(transform.position, destinationPoint, theGScoreToStopAt);
        // This is how strongly it will try to flee, if you set it to 0 it will behave like a RandomPath
        path.aimStrength = 1;
        // Determines the variation in path length that is allowed
        path.spread = 4000;
        // Get the Seeker component which must be attached to this GameObject
        Seeker seeker = GetComponent<Seeker>();
        // Start the path and return the result to MyCompleteFunction (which is a function you have to define, the name can of course be changed)
        seeker.StartPath(path, PathComplete);
    }

    protected void PathComplete(Path p) {
        path = p;
        currentWaypoint = 0;
    }

    public void MoveForwards() {
        canMove = true;
        destinationPoint = moveForwardsPoint;
    }

    public void MoveToInitialPosition() {
        canMove = true;
        destinationPoint = unit.GetInitialUnitPosition();
    }

    public void MoveToTarget(Vector3 targetPosition) {
        canMove = true;
        destinationPoint = targetPosition;
    }

    public void MoveBehindTarget(Vector3 targetPosition, float distanceBehind) {
        canMove = true;
        Vector3 destinationPointBehindTarget = targetPosition;

        if(unit.IsOwnedByPlayer()) {
            destinationPointBehindTarget += new Vector3(distanceBehind, 0, 0);
        } else {
            destinationPointBehindTarget -= new Vector3(distanceBehind, 0, 0);
        }

        destinationPoint = destinationPointBehindTarget;
    }

    private void SetMoveForwardsPoint() {
        moveForwardsPoint = BattleGrid.Instance.GetMoveForwardsNextGridPosition(unit);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMoveDirServerRpc(Vector2 moveDirNormalized) {

        short moveDirShortX = (short)(moveDirNormalized.x * 100);  // Multiplier par 100 pour convertir en entier
        short moveDirShortY = (short)(moveDirNormalized.y * 100);

        SetMoveDirClientRpc(moveDirShortX, moveDirShortY);
    }

    [ClientRpc]
    private void SetMoveDirClientRpc(short moveDirShortX, short moveDirShortY) {
        float moveDirX = moveDirShortX / 100.0f;
        float moveDirY = moveDirShortY / 100.0f;

        Vector2 moveDirShort = new Vector2(moveDirX, moveDirY);

        if (!IsServer) {
            // Mirror movedir 
            moveDirShort.x = -moveDirShort.x;
        }

        moveDir2D = new Vector2(moveDirShort.x, moveDirShort.y);
    }

    public void StopMoving() {
        canMove = false;
        moveDir = Vector3.zero;
    }

    public void ResetSpeed() {
        rb.velocity = Vector3.zero;
    }

    public void SetDazed(bool dazed) {
        this.dazed = dazed;
    }

    #region BUFFS

    public void SetMoveSpeedMultiplier(float moveSpeedMultiplier) {
        this.moveSpeedMultiplier = moveSpeedMultiplier;
    }
    public float GetMoveSpeedMultiplier() {
        return moveSpeedMultiplier;
    }
    public void BuffMoveSpeed(float moveSpeedBuff) {
        BuffMoveSpeedServerRpc(moveSpeedBuff);

    }

    public void DebuffMoveSpeed(float moveSpeedDebuff) {
        moveSpeed -= moveSpeedDebuff;
    }

    public void ResetMoveSpeed() {
        ResetMoveSpeedServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    private void BuffMoveSpeedServerRpc(float moveSpeedBuff) {
        moveSpeedMultiplier += moveSpeedBuff;
        BuffMoveSpeedClientRpc();
    }

    [ClientRpc]
    private void BuffMoveSpeedClientRpc() {
        Debug.Log("BuffAttackDamageClientRpc");
        OnMoveSpeedBuffed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetMoveSpeedServerRpc() {
        moveSpeedMultiplier = 1f;
        Debug.Log("attack damage Reset");
        ResetMoveSpeedClientRpc();
    }

    [ClientRpc]
    private void ResetMoveSpeedClientRpc() {
        Debug.Log("ResetAttackDamageClientRpc");
        OnMoveSpeedDebuffed?.Invoke(this, EventArgs.Empty);
    }




    #endregion

    public void SetWatchDir(Transform targetTransform) {
        Vector2 watchDir = (targetTransform.position - transform.position).normalized;

        short watchDirShortX = (short)(watchDir.x * 100);  // Multiplier par 100 pour convertir en entier
        short watchDirShortY = (short)(watchDir.y * 100);

        SetWatchDirServerRpc(watchDirShortX, watchDirShortY);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetWatchDirServerRpc(short watchDirShortX, short watchDirShortY) {
        SetWatchDirClientRpc(watchDirShortX, watchDirShortY);
    }

    [ClientRpc]
    private void SetWatchDirClientRpc(short watchDirShortX, short watchDirShortY) {

        float watchDirX = watchDirShortX / 100.0f;
        float watchDirY = watchDirShortY / 100.0f;

        Vector2 watchDirNormalized = new Vector2(watchDirX, watchDirY);

        if (!IsServer) {
            // Mirror watchdir 
            watchDirNormalized.x = -watchDirNormalized.x;
        }
        watchDir = new Vector2(watchDirNormalized.x, watchDirNormalized.y);
    }

    public Vector2 GetWatchDir2D() {
        return watchDir;
    }

    public Vector2 GetMoveDir2D() {
        return moveDir2D;
    }

    public float GetMoveSpeed() {
        return moveSpeed;
    }

    public Vector3 GetDestinationPoint() {
        return destinationPoint;
    }

    private void Unit_OnUnitPlaced(object sender, EventArgs e) {

        if (unit.GetParentTroop().IsOwnedByPlayer()) {
            moveDirMultiplier = 1;
        }
        else {
            moveDirMultiplier = -1;
        }
        SetMoveForwardsPoint();
    }

    private void Unit_OnUnitReset(object sender, EventArgs e) {
        ResetSpeed();

        moveForwardsPoint = BattleGrid.Instance.GetMoveForwardsCustomGridPosition(this.unit, unit.GetInitialUnitGridPosition());
    }

    private void Unit_OnUnitDynamicallySpawned(object sender, EventArgs e) {
        SetMoveForwardsPoint();
    }

    private void Unit_OnAdditionalUnitActivated(object sender, EventArgs e) {
        SetMoveForwardsPoint();
    }

    private void Unit_OnParentTroopSet(object sender, EventArgs e) {
        parentTroopSet = true;
    }

}
