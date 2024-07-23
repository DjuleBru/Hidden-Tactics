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
    protected ITargetable attackTarget;
    protected float attackDamage;
    protected float attackTimer;

    protected float attackStartTimer;
    protected float attackEndTimer;
    protected bool attackStarted;

    protected float attackRate;
    protected float attackAOE;
    protected float attackKnockback;
    protected float attackDazedTime;
    protected float attackAnimationHitDelay;
    protected float meleeAttackAnimationDuration;
    protected bool attackDecomposition;
    protected bool attackHasAOECollider;
    protected bool attackHasAOE;
    protected List<AttackSO.UnitAttackSpecial> attackSpecialList;

    protected float attackRateMultiplier = 1;
    protected float attackDamageMultiplier = 1;
    protected float attackKnockbackBuffAbsolute;
    public event EventHandler OnAttackRateBuffed;
    public event EventHandler OnAttackRateDebuffed;

    protected bool attacking;
    protected bool dazed;

    protected void Awake() {
        unitAI = GetComponent<UnitAI>();
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        activeAttackSO = unit.GetUnitSO().mainAttackSO;
        UpdateActiveAttackParameters(activeAttackSO);

    }

    protected void Start() {

        if (IsServer) {
            RandomizeAttackTimersServerRpc();
        }
    }

    public override void OnNetworkSpawn() {
        if (unit.GetUnitIsOnlyVisual()) return;
        unitAI.OnStateChanged += UnitAI_OnStateChanged;
        unit.OnUnitReset += Unit_OnUnitReset;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }


    protected virtual void Update() {

        if (attackTarget as MonoBehaviour == null) return;

        unitMovement.SetWatchDir((attackTarget as MonoBehaviour).transform);

        if (attacking && !dazed) {
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

            if (activeAttackSO.attackType == AttackSO.AttackType.melee) {
                StartCoroutine(MeleeAttack(attackTarget));
            }
            if (activeAttackSO.attackType == AttackSO.AttackType.ranged) {
                StartCoroutine(RangedAttack(attackTarget));
            }
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

    protected IEnumerator MeleeAttack(ITargetable target) {
        OnUnitAttack?.Invoke(this, EventArgs.Empty);
        unitAI.SetAttackStarted(true);

        yield return new WaitForSeconds(attackAnimationHitDelay);


        if (!unit.GetIsDead() && !target.GetIsDead()) {
            // Unit is still alive on attack animation hit and target is still alive
            InflictDamage(target, transform.position);
        }

        yield return new WaitForSeconds(meleeAttackAnimationDuration - attackAnimationHitDelay);
        unitAI.SetAttackStarted(false);
    }

    protected IEnumerator RangedAttack(ITargetable target) {
        OnUnitAttack?.Invoke(this, EventArgs.Empty);
        unitAI.SetAttackStarted(true);

        yield return new WaitForSeconds(attackAnimationHitDelay);

        if (!unit.GetIsDead() && !target.GetIsDead()) {
            // Unit is still alive on attack animation hit and target unit is still alive
            Shoot(attackTarget);
        }

        yield return new WaitForSeconds(meleeAttackAnimationDuration - attackAnimationHitDelay);
        unitAI.SetAttackStarted(false);
    }

    protected void Shoot(ITargetable target) {
        NetworkObject targetUnitNetworkObject = (target as MonoBehaviour).GetComponent<NetworkObject>();
        if(IsServer) {
            SpawnProjectileServerRpc(targetUnitNetworkObject);
        }
    }

    public void ProjectileHasHit(ITargetable target, Vector3 projectileHitPosition) {
        if (!IsServer) return;
        InflictDamage(target, projectileHitPosition);
    }

    protected void InflictDamage(ITargetable target, Vector3 damageHitPosition) {
        if (!IsServer) return;
        PerformAllDamageActions(target, damageHitPosition);

        if (attackHasAOE) {
            // Attack has one form of AOE

            if (attackAOE != 0) {
                // Attack AOE is a circle around damageHitPosition

                foreach (Unit unitAOETarget in FindAOEAttackTargets(damageHitPosition, attackAOE)) {
                    // Don't damage target unit twice
                    if(unitAOETarget != (target as MonoBehaviour)) {
                        PerformAllDamageActions(unitAOETarget, damageHitPosition);
                    }
                }
            }

            if (attackHasAOECollider) {
                // Attack AOE uses a specific collider
                List<Collider2D> collidersInAttackAOEList = attackColliderAOE.GetCollidersInAttackAOEList();
                List<Unit> unitsInAttackAOE = FindTargetUnitsInColliderList(collidersInAttackAOEList);

                foreach (Unit unitAOETarget in unitsInAttackAOE) {
                    // Don't damage target unit twice
                    if (unitAOETarget != (target as MonoBehaviour)) {
                        PerformAllDamageActions(unitAOETarget, damageHitPosition);
                    }
                }
            }
        }

    }

    protected virtual void PerformAllDamageActions(ITargetable target, Vector3 damageHitPosition) {
        IDamageable targetIDamageable = target.GetIDamageable();

        //If attacking village:die
        if (target is Village) {
            PerformDamageActionOnVillage(targetIDamageable);
            return;
        }

        if (target is Unit) {
            PerformDamageActionOnUnit(target, damageHitPosition);
        }

        targetIDamageable.TakeDamage(attackDamage);
    }

    protected virtual void PerformDamageActionOnUnit(ITargetable target, Vector3 damageHitPosition) {

        //Knockback
        if (attackKnockback != 0) {
            Vector2 incomingDamageDirection = new Vector2((target as Unit).transform.position.x - damageHitPosition.x, (target as Unit).transform.position.y - damageHitPosition.y);
            Vector2 force = incomingDamageDirection * attackKnockback;

            (target as Unit).TakeKnockBack(force);
        }

        //Daze
        if (attackDazedTime != 0) {
            (target as Unit).TakeDazed(attackDazedTime);
        }

        //Fire
        if(attackSpecialList.Contains(AttackSO.UnitAttackSpecial.fire)) {
            (target as Unit).TakeSpecial(AttackSO.UnitAttackSpecial.fire, activeAttackSO.specialEffectDuration);
        }

        //Fear
        if (attackSpecialList.Contains(AttackSO.UnitAttackSpecial.fear)) {
            (target as Unit).TakeSpecial(AttackSO.UnitAttackSpecial.fear, activeAttackSO.specialEffectDuration);
        }
    }

    protected virtual void PerformDamageActionOnVillage(IDamageable targetIDamageable) {
        GetComponent<UnitHP>().TakeDamage(unit.GetUnitSO().HP);
        targetIDamageable.TakeDamage(unit.GetUnitSO().damageToVillages);
    }

    protected List<Unit> FindAOEAttackTargets(Vector3 targetPosition, float AOE) {

        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(targetPosition, AOE);
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

                if (unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.GetIsDead()) {
                    // target unit is not from the same team AND Unit is not dead
                    targetUnitList.Add(unit);
                }
            };
        }
        return targetUnitList;
    }

    protected void UpdateActiveAttackParameters(AttackSO attackSO) {
        attackDamage = attackSO.attackDamage * attackDamageMultiplier;
        attackRate = attackSO.attackRate * attackRateMultiplier;
        attackKnockback = attackSO.attackKnockback + attackKnockbackBuffAbsolute;
        attackDazedTime = attackSO.attackDazedTime;
        attackAnimationHitDelay = attackSO.meleeAttackAnimationHitDelay;
        meleeAttackAnimationDuration = attackSO.meleeAttackAnimationDuration;
        attackAOE = attackSO.attackAOE;
        attackDecomposition = attackSO.attackDecomposition;
        attackHasAOECollider = attackSO.attackHasAOECollider;
        attackHasAOE = attackSO.attackHasAOE;
        attackSpecialList = attackSO.attackSpecialList;
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
    protected void InitializeProjectileClientRpc(NetworkObjectReference projectileNetworkObjectReference, NetworkObjectReference targetNetworkReference) {

        targetNetworkReference.TryGet(out NetworkObject targetNetworkObject);
        ITargetable target = targetNetworkObject.GetComponent<ITargetable>();

        projectileNetworkObjectReference.TryGet(out NetworkObject projectileNetworkObject);
        Projectile projectile = projectileNetworkObject.GetComponent<Projectile>();

        projectile.GetComponent<Projectile>().Initialize(this, target);
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

    public void SetDazed(bool dazed) {
        this.dazed = dazed;
    }

    protected virtual void UnitAI_OnStateChanged(object sender, System.EventArgs e) {
        if(unitAI.IsAttacking()) {
            attacking = true;
        } else {
            attacking = false;
            attackStarted = false;
        }

        if(unitAI.IsDead()) {
            ResetBuffs();
        }
    }

    private void Unit_OnUnitReset(object sender, EventArgs e) {
        ResetBuffs();
    }

    #endregion

    #region BUFFS

    public void BuffAttackDamage(float attackDamagebuff) {
        attackDamageMultiplier += attackDamagebuff;
        UpdateActiveAttackParameters(activeAttackSO);
    }

    public void DebuffAttackDamage(float attackDamageDebuff) {
        attackDamageMultiplier -= attackDamageDebuff;
        UpdateActiveAttackParameters(activeAttackSO);
    }

    public void BuffAttackRate(float attackRatebuff) {
        BuffAttackRateServerRpc(attackRatebuff);
    }

    public void ResetAttackRate() {
        ResetAttackRateServerRpc();
    }

    public void BuffAttackKnockbackAbsolute(float attackKnockbackbuff) {
        attackKnockbackBuffAbsolute += attackKnockbackbuff;
        UpdateActiveAttackParameters(activeAttackSO);
    }

    public void DebuffAttackKnockbackAbsolute(float attackKnockbackDebuff) {
        attackKnockbackBuffAbsolute -= attackKnockbackDebuff;
        UpdateActiveAttackParameters(activeAttackSO);
    }

    private void ResetBuffs() {
        if (attackRateMultiplier < 1) {
            ResetAttackRate();
        }

        if (attackDamageMultiplier > 1) {
            attackDamageMultiplier = 1;
        }

        if (attackKnockbackBuffAbsolute > 0) {
            attackKnockbackBuffAbsolute = 0;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void BuffAttackRateServerRpc(float attackRatebuff) {
        attackRateMultiplier -= attackRatebuff;
        UpdateActiveAttackParameters(activeAttackSO);
        Debug.Log("attack rate buffed to " + attackRateMultiplier);

        BuffAttackRateClientRpc();
    }

    [ClientRpc]
    private void BuffAttackRateClientRpc() {
        Debug.Log("BuffAttackRateClientRpc");
        OnAttackRateBuffed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetAttackRateServerRpc() {
        attackRateMultiplier = 1f;
        Debug.Log("attack rate Reset");
        ResetAttackRateClientRpc();
    }

    [ClientRpc]
    private void ResetAttackRateClientRpc() {
        Debug.Log("ResetAttackRateClientRpc");
        OnAttackRateDebuffed?.Invoke(this, EventArgs.Empty);
        UpdateActiveAttackParameters(activeAttackSO);
    }

    #endregion

    #region SET PARAMETERS

    public void SetAttackTarget(ITargetable attackTarget) {
        if(attackTarget != null) {
            NetworkObject targetNetworkObject = (attackTarget as MonoBehaviour).GetComponent<NetworkObject>();
            SetAttackTargetServerRpc(targetNetworkObject);
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
    public void SetAttackTargetClientRpc(NetworkObjectReference targetNetworkObjectReference) {
        targetNetworkObjectReference.TryGet(out NetworkObject targetNetworkObject);
        attackTarget = targetNetworkObject.GetComponent<ITargetable>();
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
    public ITargetable GetAttackTarget() {
        return attackTarget;
    }

    public Vector3 GetProjectileSpawnPointPosition() {
        return projectileSpawnPoint.position;
    }
    public AttackSO GetActiveAttackSO() {
        return activeAttackSO;
    }

    public float GetAttackDamage() {
        return attackDamage;
    }
    #endregion

    public override void OnDestroy() {
        base.OnDestroy();
        if (unit.GetUnitIsOnlyVisual()) return;

        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }
}
