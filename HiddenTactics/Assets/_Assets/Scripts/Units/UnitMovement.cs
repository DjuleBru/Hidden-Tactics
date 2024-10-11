using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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
    private UnitAI unitAI;
    private Rigidbody2D rb;
    private CircleCollider2D unitCollider;

    private bool dazed;
    private NetworkVariable<bool> canMove = new NetworkVariable<bool>();
    private bool inFormation;
    private bool parentTroopSet;

    private float moveSpeed;
    private float moveSpeedMultiplier = 1f;

    private Vector3 destinationPoint;

    private NetworkVariable<Vector2> moveDirSynced = new NetworkVariable<Vector2>();
    private Vector2 moveDir;
    private float moveDirSyncTimer;
    private float moveDirSyncRate = .2f;
    private Vector2 previousMoveDir;

    private Vector3 watchDir;
    private Vector3 moveForwardsPoint;

    public event EventHandler OnMoveSpeedBuffed;
    public event EventHandler OnMoveSpeedDebuffed;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        unit = GetComponent<Unit>();
        seeker = GetComponent<Seeker>();
        unitCollider = GetComponent<CircleCollider2D>();
        unitAI = GetComponent<UnitAI>();

        moveSpeed = unit.GetUnitSO().unitMoveSpeed;
    }

    public override void OnNetworkSpawn() {
        unit.OnUnitPlaced += Unit_OnUnitPlaced;
        unit.OnUnitReset += Unit_OnUnitReset;
        unit.OnUnitDynamicallySpawned += Unit_OnUnitDynamicallySpawned;
        unit.OnAdditionalUnitActivated += Unit_OnAdditionalUnitActivated;
        unit.OnParentTroopSet += Unit_OnParentTroopSet;

        //canMove.OnValueChanged += canMove_OnValueChanged;
        moveDirSynced.OnValueChanged += moveDirSynced_OnValueChanged;
    }


    private void Update() {
        if (!parentTroopSet) return;
        if (!IsServer) return;

        pathCalculationTimer -= Time.deltaTime;
        if (pathCalculationTimer < 0) {
            CheckUnitsInFormation();
        }

        if (!canMove.Value) return;

        //HandleSyncMoveDir();

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
        if (!canMove.Value) return;

        if (IsServer) {
            if (!dazed) {
                rb.velocity = moveDir * moveSpeed * moveSpeedMultiplier * Time.fixedDeltaTime;
            }
        }
        else {
            rb.velocity = GetMoveDir2D() * moveSpeed * moveSpeedMultiplier * Time.fixedDeltaTime;
        }
    }

    private void HandleSyncMoveDir() {
        moveDirSyncTimer -= Time.deltaTime;

        if(previousMoveDir == Vector2.zero) {
            previousMoveDir = moveDir;
        }

        if (moveDirSyncTimer < 0) {
            moveDirSyncTimer = moveDirSyncRate;


            if (moveDir.x * previousMoveDir.x < 0 || moveDir.y * previousMoveDir.y < 0) {
                Debug.Log("synching move dir");
                previousMoveDir = moveDir;
                moveDirSynced.Value = moveDir;
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
        canMove.Value = true;
        destinationPoint = moveForwardsPoint;
    }

    public void MoveToInitialPosition() {
        canMove.Value = true;
        destinationPoint = unit.GetInitialUnitPosition();
    }

    public void MoveToTarget(Vector3 targetPosition) {
        //Debug.Log("MoveToTarget");
        canMove.Value = true;
        destinationPoint = targetPosition;
    }

    public void MoveBehindTarget(Vector3 targetPosition, float distanceBehind) {
        canMove.Value = true;
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

    private void canMove_OnValueChanged(bool previousValue, bool newValue) {
        //Debug.Log(previousValue + " canMove_OnValueChanged " + newValue);
    } 

    private void moveDirSynced_OnValueChanged(Vector2 previousValue, Vector2 newValue) {
        moveDir = newValue;
    }

    public void StopMoving() {
        if(IsServer) {
            if (canMove.Value == true) {
                //Debug.Log(unit + " StopMoving");
                canMove.Value = false;
            }
        }

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
        watchDir = (targetTransform.position - transform.position).normalized;
    }

    public void SetMoveForwardsMoveDirClient() {

        if(unit.IsOwnedByPlayer()) {
            moveDir = new Vector2(-1, 0);
        } else {
            moveDir = new Vector2(1, 0);
        }
        
    }

    public Vector2 GetWatchDir2D() {
        return watchDir;
    }

    public Vector2 GetMoveDir2D() {
        Vector2 moveDirClient = moveDir;

        if (!IsServer) {
            moveDirClient.x = -moveDirClient.x;
        }
        return moveDirClient;
    }

    public float GetMoveSpeed() {
        return moveSpeed;
    }

    public Vector3 GetDestinationPoint() {
        return destinationPoint;
    }

    private void Unit_OnUnitPlaced(object sender, EventArgs e) {
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
