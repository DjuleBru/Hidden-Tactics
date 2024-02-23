using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAttack : NetworkBehaviour
{
    public event EventHandler OnUnitAttack;
    public event EventHandler OnUnitAttackStarted;
    public event EventHandler OnUnitAttackEnded;


    [SerializeField] private Transform projectileSpawnPoint;

    private Unit unit;
    private UnitAI unitAI;

    private Unit attackTarget;
    private int attackDamage;
    private float attackTimer;

    private float attackStartTimer;
    private float attackEndTimer;
    private bool attackStarted;

    private float attackRate;
    private float attackAOE;
    private float attackKnockback;
    private float attackDazedTime;
    private float attackAnimationHitDelay;

    private bool attacking;

    private void Awake() {
        unitAI = GetComponent<UnitAI>();
        unit = GetComponent<Unit>();
    }

    private void Start() {
        UpdateActiveAttackParameters();
    }

    public override void OnNetworkSpawn() {
        unitAI.OnStateChanged += UnitAI_OnStateChanged;
        unit.OnUnitDazed += Unit_OnUnitDazed;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {

        if (attacking) {
            if(!unit.GetUnitSO().mainAttackSO.attackDecomposition) {
                HandleStandardAttack();
            } else {
                HandleDecomposedAttack();
            }
        }

    }

    private void HandleStandardAttack() {
        attackTimer -= Time.deltaTime;
        if (attackTimer < 0) {
            attackTimer = attackRate;
            StartCoroutine(MeleeAttack(attackTarget));
        }
    }

    private void HandleDecomposedAttack() {
        if (!attackStarted) {
            attackStartTimer -= Time.deltaTime;

            if (attackStartTimer < 0) {
                attackStarted = true;
                OnUnitAttackStarted?.Invoke(this, EventArgs.Empty);
            }
        } else {
            attackEndTimer -= Time.deltaTime;

            if (attackEndTimer < 0) {
                attackStarted = false;
                attackStartTimer = attackRate/3;
                attackEndTimer = attackRate;

                Shoot(attackTarget);
                OnUnitAttackEnded?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private IEnumerator MeleeAttack(Unit targetUnit) {
        OnUnitAttack?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(attackAnimationHitDelay);

        if (!unit.GetUnitIsDead() && !targetUnit.GetUnitIsDead()) {
            // Unit is still alive on attack animation hit and target unit is still alive
            InflictDamage(targetUnit);
        }
    }

    private void Shoot(Unit targetUnit) {
        NetworkObject targetUnitNetworkObject = targetUnit.GetComponent<NetworkObject>();
        SpawnProjectileServerRpc(targetUnitNetworkObject);
    }

    private void InflictDamage(Unit targetUnit) {
        if (!IsServer) return;

        PerformAllDamageActions(targetUnit);

        if (attackAOE != 0) {
            foreach (Unit unitAOETarget in FindAOEAttackTargets(transform.position)) {
                PerformAllDamageActions(unitAOETarget);
            }
        }
    }

    private void PerformAllDamageActions(Unit targetUnit) {
        targetUnit.TakeDamage(attackDamage);
        if (attackKnockback != 0) {

            Vector2 incomingDamageDirection = new Vector2(targetUnit.transform.position.x - transform.position.x, targetUnit.transform.position.y - transform.position.y);
            Vector2 force = incomingDamageDirection * attackKnockback;

            targetUnit.TakeKnockBack(force);
        }
        if (attackDazedTime != 0) {
            targetUnit.TakeDazed(attackDazedTime);
        }
    }

    private List<Unit> FindAOEAttackTargets(Vector3 targetPosition) {

        List<Unit> AOETargetUnitList = new List<Unit>();
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(targetPosition, attackAOE);

        foreach (Collider2D collider in colliderArray) {
            if (collider.TryGetComponent<Unit>(out Unit unit)) {
                // Collider is a unit

                if (unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.GetUnitIsDead()) {
                    // target unit is not from the same team AND Unit is not dead
                    AOETargetUnitList.Add(unit);
                }
            };
        }

        return AOETargetUnitList;
    }

    private void UpdateActiveAttackParameters() {
        attackDamage = unit.GetUnitSO().mainAttackSO.attackDamage;
        attackRate = unit.GetUnitSO().mainAttackSO.attackRate;
        attackKnockback = unit.GetUnitSO().mainAttackSO.attackKnockback;
        attackDazedTime = unit.GetUnitSO().mainAttackSO.attackDazedTime;
        attackAnimationHitDelay = unit.GetUnitSO().mainAttackSO.attackAnimationHitDelay;
        attackAOE = unit.GetUnitSO().mainAttackSO.attackAOE;

        if (IsServer) {
            RandomizeAttackTimersServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnProjectileServerRpc(NetworkObjectReference targetUnitNetworkObjectReference) {

        targetUnitNetworkObjectReference.TryGet(out NetworkObject targetUnitNetworkObject);

        GameObject projectile = Instantiate(unit.GetUnitSO().mainAttackSO.projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        NetworkObject projectileNetworkObject = projectile.GetComponent<NetworkObject>();

        projectileNetworkObject.Spawn(true);

        SpawnProjectileClientRpc(projectileNetworkObject, targetUnitNetworkObject);
    }

    [ClientRpc]
    private void SpawnProjectileClientRpc(NetworkObjectReference projectileNetworkObjectReference, NetworkObjectReference targetUnitNetworkReference) {

        targetUnitNetworkReference.TryGet(out NetworkObject targetUnitNetworkObject);
        Unit targetUnit = targetUnitNetworkObject.GetComponent<Unit>();

        projectileNetworkObjectReference.TryGet(out NetworkObject projectileNetworkObject);
        Projectile projectile = projectileNetworkObject.GetComponent<Projectile>();

        projectile.GetComponent<Projectile>().Initialize(this, targetUnit);
    }


    [ServerRpc]
    private void RandomizeAttackTimersServerRpc() {

        attackTimer = UnityEngine.Random.Range(0, attackRate / 4);
        attackStartTimer = UnityEngine.Random.Range(attackRate / 6, attackRate / 3);
        attackEndTimer = attackRate;

        SetAttackTimersClientRpc(attackTimer, attackStartTimer, attackEndTimer);
    }

    [ClientRpc]
    private void SetAttackTimersClientRpc(float attackTimer, float attackStartTimer, float attackEndTimer) {

        this.attackTimer = attackTimer;
        this.attackStartTimer = attackStartTimer;
        this.attackEndTimer = attackEndTimer;

    }

    public void ProjectileHasHit(Unit targetUnit) {
        if (!IsServer) return;
        InflictDamage(targetUnit);
    }

    #region EVENT RESPONSES

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if (BattleManager.Instance.IsBattlePhaseEnding()) {
            RandomizeAttackTimersServerRpc();
        }
    }

    private void Unit_OnUnitDazed(object sender, Unit.OnUnitDazedEventArgs e) {
        if (!IsServer) return;
        attacking = false;
    }

    private void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        if(unitAI.IsAttacking()) {
            attacking = true;
        } else {
            attacking = false;
        }
    }
    #endregion

    #region SET PARAMETERS
    public void SetAttackTarget(Unit unit) {
        NetworkObject unitNetworkObject = unit.GetComponent<NetworkObject>();
        SetAttackTargetServerRpc(unitNetworkObject);
    }

    [ServerRpc]
    public void SetAttackTargetServerRpc(NetworkObjectReference unitNetworkObjectReference) {
        SetAttackTargetClientRpc(unitNetworkObjectReference);
    }

    [ClientRpc]
    public void SetAttackTargetClientRpc(NetworkObjectReference unitNetworkObjectReference) {
        unitNetworkObjectReference.TryGet(out NetworkObject targetUnitNetworkObject);
        attackTarget = targetUnitNetworkObject.GetComponent<Unit>();
    }
    #endregion

    #region GET PARAMETERS
    public Unit GetAttackTarget() {
        return attackTarget;
    }

    public Vector3 GetProjectileSpawnPointPosition() {
        return projectileSpawnPoint.position;
    }
    #endregion
}
