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

    [SerializeField] protected float nextWaypointDistance = 1.5f;

    protected int currentWaypoint = 0;
    protected bool reachedEndOfPath;

    protected float pathCalculationRate = .2f;
    protected float pathCalculationTimer = 0;
    #endregion

    private Unit unit;
    private Rigidbody2D rb;
    private CircleCollider2D unitCollider;
    private Vector3 moveDir2D;
    private int moveDirMultiplier;

    private bool dazed;
    private bool canMove;
    private float moveSpeed;

    private Vector3 destinationPoint;
    private Vector3 moveDir;
    private Vector3 watchDir;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        unit = GetComponent<Unit>();
        seeker = GetComponent<Seeker>();
        unitCollider = GetComponent<CircleCollider2D>();

        moveSpeed = unit.GetUnitSO().unitMoveSpeed;
    }


    public override void OnNetworkSpawn() {
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
    }

    private void Update() {
        if (!canMove) return;

        pathCalculationTimer -= Time.deltaTime;

        if (pathCalculationTimer < 0) {
            Vector3 direction = (destinationPoint - unit.transform.position).normalized;
            Vector3 pathStartPoint = unit.transform.position + direction * unitCollider.radius;

            CalculatePath(pathStartPoint, destinationPoint);
        }

        if (path != null) {
            FollowPath(path);
        }
    }

    private void FixedUpdate() {
        if (!canMove) return;

        if (IsServer) {
            if (!dazed) {
                MoveServerRpc(moveDir);
            }
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

    protected void PathComplete(Path p) {
        path = p;
        currentWaypoint = 0;
    }

    public void MoveForwards() {
        canMove = true;
        destinationPoint = transform.position + new Vector3(moveDirMultiplier * 50, 0, 0);
    }

    public void MoveToTarget(Vector3 targetPosition) {
        canMove = true;

        CalculatePath(transform.position, targetPosition);
        destinationPoint = targetPosition;
    }


    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(Vector3 moveDir3DNormalized) {
        rb.velocity = moveDir3DNormalized * moveSpeed * Time.fixedDeltaTime;
        MoveClientRpc(moveDir3DNormalized);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector3 moveDir3DNormalized) {
        if (!IsServer) {
            // Mirror movedir 
            moveDir3DNormalized.x = -moveDir3DNormalized.x;
        }

        moveDir2D = new Vector2(moveDir3DNormalized.x, moveDir3DNormalized.y);
    }

    public void StopMoving() {
        canMove = false;
        moveDir = Vector3.zero;
    }

    public void SetDazed(bool dazed) {
        this.dazed = dazed;
    }

    public void BuffMoveSpeed(float moveSpeedBuff) {
        moveSpeed += moveSpeedBuff;

    }
    public void DebuffMoveSpeed(float moveSpeedDebuff) {
        moveSpeed -= moveSpeedDebuff;
    }


    public void SetWatchDir(Transform targetTransform) {
        Vector3 watchDir = (targetTransform.position - transform.position).normalized;
        SetWatchDirServerRpc(watchDir);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetWatchDirServerRpc(Vector3 watchDir3DNormalized) {
        SetWatchDirClientRpc(watchDir3DNormalized);
    }

    [ClientRpc]
    private void SetWatchDirClientRpc(Vector3 watchDir3DNormalized) {
        if (!IsServer) {
            // Mirror watchdir 
            watchDir3DNormalized.x = -watchDir3DNormalized.x;
        }

        watchDir = new Vector2(watchDir3DNormalized.x, watchDir3DNormalized.y);
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

    private void Unit_OnUnitPlaced(object sender, EventArgs e) {

        if (unit.GetParentTroop().IsOwnedByPlayer()) {
            moveDirMultiplier = 1;
        }
        else {
            moveDirMultiplier = -1;
        }
    }

}
