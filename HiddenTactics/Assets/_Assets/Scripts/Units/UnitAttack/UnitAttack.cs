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

    [SerializeField] protected Transform projectileSpawnPoint;
    [SerializeField] protected AttackColliderAOE attackColliderAOE;

    protected Unit unit;
    protected UnitAI unitAI;
    protected UnitMovement unitMovement;

    protected AttackSO activeAttackSO;
    protected Unit attackTarget;
    protected float attackDamage;
    protected float attackTimer;

    protected float attackStartTimer;
    protected float attackEndTimer;
    protected bool attackStarted;

    protected float attackRate;
    protected float attackAOE;
    protected float attackKnockback;
    protected float attackDazedTime;
    protected float meleeAttackAnimationHitDelay;
    protected float meleeAttackAnimationDuration;
    protected bool attackDecomposition;
    protected bool attackHasAOECollider;
    protected bool attackHasAOE;

    protected float attackRateModidier;
    protected float attackDamageModifier;
    protected float attackKnockbackModifier;

    protected bool attacking;

    protected void Awake() {
        unitAI = GetComponent<UnitAI>();
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();

        UpdateActiveAttackParameters(unit.GetUnitSO().mainAttackSO);
        activeAttackSO = unit.GetUnitSO().mainAttackSO;
    }

    protected void Start() {
        if(IsServer) {
            RandomizeAttackTimersServerRpc();
        }
    }

    public override void OnNetworkSpawn() {
        unitAI.OnStateChanged += UnitAI_OnStateChanged;
        unit.OnUnitDazed += Unit_OnUnitDazed;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    protected void Update() {

        if (attackTarget != null) {
            unitMovement.SetWatchDir(attackTarget.transform);
        }

        if (attacking) {
            if (!attackDecomposition) {
                HandleStandardAttack();
            } else {
                HandleDecomposedRangedAttack();
            }
        }

    }

    protected virtual void HandleStandardAttack() {
        attackTimer -= Time.deltaTime;
        if (attackTimer < 0) {
            attackTimer = attackRate;
            StartCoroutine(MeleeAttack(attackTarget));
        }
    }

    protected void HandleDecomposedRangedAttack() {
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

    protected IEnumerator MeleeAttack(Unit targetUnit) {
        OnUnitAttack?.Invoke(this, EventArgs.Empty);
        unitAI.SetAttackStarted(true);

        yield return new WaitForSeconds(meleeAttackAnimationHitDelay);

        if (!unit.GetUnitIsDead() && !targetUnit.GetUnitIsDead()) {
            // Unit is still alive on attack animation hit and target unit is still alive
            InflictDamage(targetUnit);
        }

        yield return new WaitForSeconds(meleeAttackAnimationDuration - meleeAttackAnimationHitDelay);
        unitAI.SetAttackStarted(false);
    }

    protected void Shoot(Unit targetUnit) {
        NetworkObject targetUnitNetworkObject = targetUnit.GetComponent<NetworkObject>();
        if(IsServer) {
            SpawnProjectileServerRpc(targetUnitNetworkObject);
        }
    }

    public void ProjectileHasHit(Unit targetUnit) {
        if (!IsServer) return;
        InflictDamage(targetUnit);
    }

    protected void InflictDamage(Unit targetUnit) {
        if (!IsServer) return;
        if(attackHasAOE) {
            // Attack has one form of AOE

            if (attackAOE != 0) {
                // Attack AOE is centered aroud this unit
                foreach (Unit unitAOETarget in FindAOEAttackTargets(transform.position)) {
                    PerformAllDamageActions(unitAOETarget);
                }
            }

            if (attackHasAOECollider) {
                // Attack AOE uses a specific collider
                List<Collider2D> collidersInAttackAOEList = attackColliderAOE.GetCollidersInAttackAOEList();
                List<Unit> unitsInAttackAOE = FindTargetUnitsInColliderList(collidersInAttackAOEList);

                foreach (Unit unitAOETarget in unitsInAttackAOE) {
                    PerformAllDamageActions(unitAOETarget);
                }
            }
        } else {
            // Attack has no AOE
            PerformAllDamageActions(targetUnit);
        }

    }

    protected virtual void PerformAllDamageActions(Unit targetUnit) {
        targetUnit.GetComponent<UnitHP>().TakeDamage(attackDamage);

        if (attackKnockback != 0) {
            Vector2 incomingDamageDirection = new Vector2(targetUnit.transform.position.x - transform.position.x, targetUnit.transform.position.y - transform.position.y);
            Vector2 force = incomingDamageDirection * attackKnockback;

            targetUnit.TakeKnockBack(force);
        }

        if (attackDazedTime != 0) {
            targetUnit.TakeDazed(attackDazedTime);
        }


    }

    protected List<Unit> FindAOEAttackTargets(Vector3 targetPosition) {

        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(targetPosition, attackAOE);
        List<Collider2D> colliderList = new List<Collider2D>();
        foreach (Collider2D collider in colliderArray) {
            colliderList.Add(collider);
        }

        return FindTargetUnitsInColliderList(colliderList);
    }

    protected List<Unit> FindTargetUnitsInColliderList(List<Collider2D> colliderList) {
        List<Unit> targetUnitList = new List<Unit>();

        foreach (Collider2D collider in colliderList) {
            if (collider.TryGetComponent<Unit>(out Unit unit)) {
                // Collider is a unit

                if (unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.GetUnitIsDead()) {
                    // target unit is not from the same team AND Unit is not dead
                    targetUnitList.Add(unit);
                }
            };
        }
        return targetUnitList;
    }

    protected void UpdateActiveAttackParameters(AttackSO attackSO) {
        attackDamage = attackSO.attackDamage + attackDamageModifier;
        attackRate = attackSO.attackRate + attackRateModidier;
        attackKnockback = attackSO.attackKnockback + attackKnockbackModifier;
        attackDazedTime = attackSO.attackDazedTime;
        meleeAttackAnimationHitDelay = attackSO.meleeAttackAnimationHitDelay;
        meleeAttackAnimationDuration = attackSO.meleeAttackAnimationDuration;
        attackAOE = attackSO.attackAOE;
        attackDecomposition = attackSO.attackDecomposition;
        attackHasAOECollider = attackSO.attackHasAOECollider;
        attackHasAOE = attackSO.attackHasAOE;
    }
        
    [ServerRpc(RequireOwnership = false)]
    protected void SpawnProjectileServerRpc(NetworkObjectReference targetUnitNetworkObjectReference) {

        targetUnitNetworkObjectReference.TryGet(out NetworkObject targetUnitNetworkObject);

        GameObject projectile = Instantiate(activeAttackSO.projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        NetworkObject projectileNetworkObject = projectile.GetComponent<NetworkObject>();

        projectileNetworkObject.Spawn(true);

        InitializeProjectileClientRpc(projectileNetworkObject, targetUnitNetworkObject);
    }

    [ClientRpc]
    protected void InitializeProjectileClientRpc(NetworkObjectReference projectileNetworkObjectReference, NetworkObjectReference targetUnitNetworkReference) {

        targetUnitNetworkReference.TryGet(out NetworkObject targetUnitNetworkObject);
        Unit targetUnit = targetUnitNetworkObject.GetComponent<Unit>();

        projectileNetworkObjectReference.TryGet(out NetworkObject projectileNetworkObject);
        Projectile projectile = projectileNetworkObject.GetComponent<Projectile>();

        projectile.GetComponent<Projectile>().Initialize(this, targetUnit);
    }


    [ServerRpc(RequireOwnership = false)]
    protected void RandomizeAttackTimersServerRpc() {

        attackTimer = UnityEngine.Random.Range(0, attackRate / 4);
        attackStartTimer = UnityEngine.Random.Range(attackRate / 6, attackRate / 3);
        attackEndTimer = attackRate;

        SetAttackTimersClientRpc(attackTimer, attackStartTimer, attackEndTimer);
    }

    [ClientRpc]
    protected void SetAttackTimersClientRpc(float attackTimer, float attackStartTimer, float attackEndTimer) {

        this.attackTimer = attackTimer;
        this.attackStartTimer = attackStartTimer;
        this.attackEndTimer = attackEndTimer;

    }

    #region INVOKE EVENTS

    public void InvokeOnUnitAttack() {
        OnUnitAttack?.Invoke(this, EventArgs.Empty);
    }
    public void InvokeOnUnitAttackStarted() {
        OnUnitAttackStarted?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeOnUnitAttackEnded() {
        OnUnitAttackEnded?.Invoke(this, EventArgs.Empty);
    }


    #endregion

    #region EVENT RESPONSES

    protected void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if (BattleManager.Instance.IsBattlePhaseEnding()) {
            RandomizeAttackTimersServerRpc();
        }
    }

    protected void Unit_OnUnitDazed(object sender, Unit.OnUnitDazedEventArgs e) {
        if (!IsServer) return;
        attacking = false;
    }

    protected void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        if(unitAI.IsAttacking()) {
            attacking = true;
        } else {
            attacking = false;
            attackStarted = false;
        }
    }
    #endregion

    #region SET PARAMETERS

    public void BuffAttackDamage(float attackDamagebuff) {
        attackDamageModifier += attackDamagebuff;
        UpdateActiveAttackParameters(activeAttackSO);
    }

    public void DebuffAttackDamage(float attackDamageDebuff) {
        attackDamageModifier -= attackDamageDebuff;
        UpdateActiveAttackParameters(activeAttackSO);
    }

    public void BuffAttackKnockback(float attackKnockbackbuff) {
        attackKnockbackModifier += attackKnockbackbuff;
        UpdateActiveAttackParameters(activeAttackSO);
    }

    public void DebuffAttackKnockback(float attackKnockbackDebuff) {
        attackKnockbackModifier -= attackKnockbackDebuff;
        UpdateActiveAttackParameters(activeAttackSO);
    }


    public void SetAttackTarget(Unit attackTargetUnit) {
        if(attackTargetUnit != null) {
            NetworkObject unitNetworkObject = attackTargetUnit.GetComponent<NetworkObject>();
            SetAttackTargetServerRpc(unitNetworkObject);
        }
    }

    public void SetAttackTimer(float attackTimer) {
        this.attackTimer = attackTimer;
    }

    public void ResetAttackTarget() {
        attackTarget = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttackTargetServerRpc(NetworkObjectReference unitNetworkObjectReference) {
        SetAttackTargetClientRpc(unitNetworkObjectReference);
    }

    [ClientRpc]
    public void SetAttackTargetClientRpc(NetworkObjectReference unitNetworkObjectReference) {
        unitNetworkObjectReference.TryGet(out NetworkObject targetUnitNetworkObject);
        attackTarget = targetUnitNetworkObject.GetComponent<Unit>();
    }

    public void SetActiveAttackSO(AttackSO attackSO) {
        activeAttackSO = attackSO;
        UpdateActiveAttackParameters(attackSO);

        SetActiveAttackSODecompositionParameterServerRpc(attackSO.attackDecomposition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActiveAttackSODecompositionParameterServerRpc(bool attackSODecomposition) {
        SetActiveAttackSODecompositionParameterClientRpc(attackSODecomposition);
    }

    [ClientRpc]
    private void SetActiveAttackSODecompositionParameterClientRpc(bool attackSODecomposition) {
        attackDecomposition = attackSODecomposition;
    }

    #endregion

    #region GET PARAMETERS
    public Unit GetAttackTarget() {
        return attackTarget;
    }

    public Vector3 GetProjectileSpawnPointPosition() {
        return projectileSpawnPoint.position;
    }

    public float GetAttackDamage() {
        return attackDamage;
    }
    #endregion
}
