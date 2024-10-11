using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAttack : NetworkBehaviour, IDamageSource
{
    public event EventHandler OnUnitAttack;
    public event EventHandler OnUnitAttackStarted;
    public event EventHandler OnUnitAttackEnded;
    public event EventHandler OnUnitDecomposedAttackEnded;

    [SerializeField] protected Transform projectileSpawnPoint;
    [SerializeField] protected AttackColliderAOE attackColliderAOE;
    [SerializeField] protected float antiLargeUnitDamageMultiplierToLargeUnits;
    [SerializeField] protected float trampleUnitDamageMultiplierToSmallUnits;
    [SerializeField] protected float siegeUnitDamageMultiplierToBuildings;

    protected Unit unit;
    protected UnitAI unitAI;
    protected UnitMovement unitMovement;
    protected List<Projectile> projectilePool = new List<Projectile>();

    protected AttackSO activeAttackSO;
    protected ITargetable attackTarget;
    protected float attackDamage;

    protected NetworkVariable<float> attackTimerServer = new NetworkVariable<float>();
    protected NetworkVariable<float> attackStartTimerServer = new NetworkVariable<float>();

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
    protected NetworkVariable<bool> attackDecomposition = new NetworkVariable<bool>();
    protected bool attackHasAOECollider;
    protected bool attackHasAOE;
    protected List<AttackSO.UnitAttackSpecial> attackSpecialList;

    protected NetworkVariable<bool> mainAttackActive = new NetworkVariable<bool>();
    protected NetworkVariable<bool> sideAttackActive = new NetworkVariable<bool>();
    public event EventHandler OnMainAttackActivated;
    public event EventHandler OnSideAttackActivated;

    protected float attackRateMultiplier = 1;
    protected float attackDamageMultiplier = 1;
    protected float attackKnockbackBuffAbsolute;

    protected bool attacking;
    protected bool dazed;
    protected bool triggerDeathAttack;
    protected float deadTimer;

    protected float syncWatchDirTimer;
    protected float syncWatchDirRate = .3f;

    protected void Awake() {
        unitAI = GetComponent<UnitAI>();
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        activeAttackSO = unit.GetUnitSO().mainAttackSO;
    }

    protected void Start() {
        RandomizeAttackTimers();
    }

    public override void OnNetworkSpawn() {
        if (unit.GetUnitIsOnlyVisual()) return;
        unitAI.OnStateChanged += UnitAI_OnStateChanged;

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        if(IsServer) {
            InitializeProjectilePoolServerRpc();
        }

        attackTimerServer.OnValueChanged += AttackTimerServer_OnValueChanged;
        attackStartTimerServer.OnValueChanged += AttackStartTimerServer_OnValueChanged;

        UpdateActiveAttackParameters(activeAttackSO);
    }


    protected virtual void Update() {

        if(triggerDeathAttack) {
            deadTimer += Time.deltaTime;
            if(deadTimer >= unit.GetUnitSO().deathTriggerAttackSO.meleeAttackAnimationHitDelay) {
                TriggerDeathTriggerAttacks();
            }
        }

        if (attackTarget as MonoBehaviour == null) return;

        unitMovement.SetWatchDir((attackTarget as MonoBehaviour).transform);

        //if (IsServer) {
        //    syncWatchDirTimer += Time.deltaTime;

        //    if(syncWatchDirTimer >= syncWatchDirRate) {
        //        syncWatchDirTimer = 0;
        //    }
        //}

        if (attacking && !dazed) {
            if (!attackDecomposition.Value) {
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

            if (activeAttackSO.attackType == AttackSO.AttackType.melee || activeAttackSO.attackType == AttackSO.AttackType.jump) {
                StartCoroutine(MeleeAttack(attackTarget));
            }

            if (activeAttackSO.attackType == AttackSO.AttackType.ranged) {
                StartCoroutine(RangedAttack(attackTarget));
            }

            if (activeAttackSO.attackType == AttackSO.AttackType.healAllyMeleeTargeting || activeAttackSO.attackType == AttackSO.AttackType.healAllyRangedTargeting) {
                StartCoroutine(HealAlly(attackTarget));
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

                OnUnitDecomposedAttackEnded?.Invoke(this, EventArgs.Empty);
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

    protected IEnumerator HealAlly(ITargetable target) {
        OnUnitAttack?.Invoke(this, EventArgs.Empty);
        unitAI.SetAttackStarted(true);

        yield return new WaitForSeconds(attackAnimationHitDelay);

        if (!unit.GetIsDead() && !target.GetIsDead()) {
            // Unit is still alive on attack animation hit and target is still alive
            IDamageable targetIDamageable = target.GetIDamageable();

            targetIDamageable.Heal(attackDamage);
        }

        yield return new WaitForSeconds(meleeAttackAnimationDuration - attackAnimationHitDelay);
        unitAI.SetAttackStarted(false);
    }

    protected void Shoot(ITargetable target) {
        if (target as MonoBehaviour == null) return;

        int projectileIndex = 0;

        foreach (Projectile proj in projectilePool) {
            if (proj.GetIsPooledProjectile()) {
                projectileIndex = projectilePool.IndexOf(proj);
                break;
            }
        }

        if(IsServer) {
            int attackTargetIndex = BattleManager.Instance.GetITargetableKey(target);
            InitializeProjectileServerRpc(projectileIndex, attackTargetIndex);
        }
    }

    public void ProjectileHasHit(ITargetable target, Vector3 projectileHitPosition) {
        if (!IsServer) return;
        InflictDamage(target, projectileHitPosition);
    }

    public void UnitHasLanded(Vector3 landPosition, ITargetable target) {
        if (!IsServer) return;
        InflictAOEDamage(landPosition);
        InflictDamage(target, transform.position);
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

        OnUnitAttackEnded?.Invoke(this, EventArgs.Empty);
    }

    protected void InflictAOEDamage(Vector3 damageHitPosition) {
        if (!IsServer) return;

        if (attackHasAOE) {
            // Attack has one form of AOE

            if (attackAOE != 0) {
                // Attack AOE is a circle around damageHitPosition

                foreach (Unit unitAOETarget in FindAOEAttackTargets(damageHitPosition, attackAOE)) {
                    // Don't damage target unit twice
                    PerformAllDamageActions(unitAOETarget, damageHitPosition);
                }
            }

            if (attackHasAOECollider) {
                // Attack AOE uses a specific collider
                List<Collider2D> collidersInAttackAOEList = attackColliderAOE.GetCollidersInAttackAOEList();
                List<Unit> unitsInAttackAOE = FindTargetUnitsInColliderList(collidersInAttackAOEList);

                foreach (Unit unitAOETarget in unitsInAttackAOE) {
                    // Don't damage target unit twice
                    PerformAllDamageActions(unitAOETarget, damageHitPosition);
                    
                }
            }
        }

    }

    protected virtual void PerformAllDamageActions(ITargetable target, Vector3 damageHitPosition) {
        if(target as MonoBehaviour == null) return;
        IDamageable targetIDamageable = target.GetIDamageable();

        if (target.GetIsDead()) return;

        //If attacking village:die
        if (target is Village) {
            PerformDamageActionOnVillage(targetIDamageable);
            return;
        }

        if (target is Building) {
            PerformDamageActionOnBuilding(target);
            return;
        }

        if (target is Unit) {
            PerformDamageActionOnUnit(target, damageHitPosition);
        }

    }

    protected virtual void PerformDamageActionOnUnit(ITargetable target, Vector3 damageHitPosition) {

        IDamageable targetIDamageable = target.GetIDamageable();
        Unit targetUnit = (Unit)target;
        float attackDamageModified = attackDamage;

        // Anti-large 
        if (unit.GetUnitSO().unitKeywordsList.Contains(UnitSO.UnitKeyword.AntiLarge) && targetUnit.GetUnitSO().unitKeywordsList.Contains(UnitSO.UnitKeyword.Large)) {
            attackDamageModified = attackDamage * antiLargeUnitDamageMultiplierToLargeUnits;
        }

        // Trample
        if (unit.GetUnitSO().unitKeywordsList.Contains(UnitSO.UnitKeyword.Trample) && !targetUnit.GetUnitSO().unitKeywordsList.Contains(UnitSO.UnitKeyword.Large)) {
            attackDamageModified = attackDamage * antiLargeUnitDamageMultiplierToLargeUnits;
        }

        bool attackIgnoresArmor = activeAttackSO.attackSpecialList.Contains(AttackSO.UnitAttackSpecial.pierce);
        targetIDamageable.TakeDamage(attackDamageModified, this, attackIgnoresArmor);

        //Daze
        if (attackDazedTime != 0) {
            (target as Unit).TakeDazed(attackDazedTime);
        }

        //Knockback
        if (attackKnockback != 0) {
            Vector2 incomingDamageDirection = new Vector2((target as Unit).transform.position.x - damageHitPosition.x, (target as Unit).transform.position.y - damageHitPosition.y);
            Vector2 force = incomingDamageDirection * attackKnockback;

            (target as Unit).TakeKnockBack(force);
        }

        //Fire
        if (attackSpecialList.Contains(AttackSO.UnitAttackSpecial.fire)) {
            (target as Unit).TakeSpecial(AttackSO.UnitAttackSpecial.fire, activeAttackSO.specialEffectDuration);
        }

        //Fear
        if (attackSpecialList.Contains(AttackSO.UnitAttackSpecial.fear)) {
            (target as Unit).TakeSpecial(AttackSO.UnitAttackSpecial.fear, activeAttackSO.specialEffectDuration);
        }

        //Poison
        if (attackSpecialList.Contains(AttackSO.UnitAttackSpecial.poison)) {
            (target as Unit).TakeSpecial(AttackSO.UnitAttackSpecial.poison, activeAttackSO.specialEffectDuration);
        }

        //Webbes
        if (attackSpecialList.Contains(AttackSO.UnitAttackSpecial.webbed)) {
            (target as Unit).TakeSpecial(AttackSO.UnitAttackSpecial.webbed, activeAttackSO.specialEffectDuration);
        }
    }
    protected virtual void PerformDamageActionOnBuilding(ITargetable target) {

        IDamageable targetIDamageable = target.GetIDamageable();
        float attackDamageModified = attackDamage;

        // Siege
        if (unit.GetUnitSO().unitKeywordsList.Contains(UnitSO.UnitKeyword.Siege)) {
            attackDamageModified = attackDamage * siegeUnitDamageMultiplierToBuildings;
        }

        targetIDamageable.TakeDamage(attackDamageModified, this, false);

    }
    protected virtual void PerformDamageActionOnVillage(IDamageable targetIDamageable) {
        GetComponent<UnitHP>().TakeDamage(unit.GetUnitSO().HP, this, true);
        targetIDamageable.TakeDamage(unit.GetUnitSO().damageToVillages, this, true);
    }

    protected List<Unit> FindAOEAttackTargets(Vector3 targetPosition, float AOE, bool targetAllyUnits = false) {

        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(targetPosition, AOE);
        List<Collider2D> colliderList = new List<Collider2D>();
        foreach (Collider2D collider in colliderArray) {
            colliderList.Add(collider);
        }

        return FindTargetUnitsInColliderList(colliderList, targetAllyUnits);
    }

    protected List<Unit> FindTargetUnitsInColliderList(List<Collider2D> colliderList, bool targetAllyUnits = false) {
        List<Unit> targetUnitList = new List<Unit>();

        foreach (Collider2D collider in colliderList) {
            if (collider.TryGetComponent<Unit>(out Unit unit)) {
                // Collider is a unit
                bool correctTeam = unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer();

                if (targetAllyUnits) {
                    correctTeam = unit.GetParentTroop().IsOwnedByPlayer() == this.unit.GetParentTroop().IsOwnedByPlayer();
                }

                if (correctTeam && !unit.GetIsDead()) {
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
        attackDecomposition.Value = attackSO.attackDecomposition;
        attackHasAOECollider = attackSO.attackHasAOECollider;
        attackHasAOE = attackSO.attackHasAOE;
        attackSpecialList = attackSO.attackSpecialList;
    }

    #region PROJECTILES

    [ServerRpc(RequireOwnership = false)]
    public void InitializeProjectilePoolServerRpc() {

        if (unit.GetUnitSO().mainAttackSO.projectilePrefab != null) {
            int projectileNumber = unit.GetUnitSO().projectileNumberToPool;

            for (int i = 0; i < projectileNumber; i++) {
                SpawnProjectileServerRpc();
            }
        }
       
    }


    [ServerRpc(RequireOwnership = false)]
    protected void SpawnProjectileServerRpc() {
        GameObject projectile = Instantiate(unit.GetUnitSO().mainAttackSO.projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        NetworkObject projectileNetworkObject = projectile.GetComponent<NetworkObject>();

        projectileNetworkObject.Spawn(true);
        SpawnProjectileClientRpc(projectileNetworkObject);
    }

    [ClientRpc]
    protected void SpawnProjectileClientRpc(NetworkObjectReference projectileNetworkObjectReferenc) {
        projectileNetworkObjectReferenc.TryGet(out NetworkObject projectileNetworkObject);
        projectileNetworkObject.gameObject.SetActive(false);

        projectilePool.Add(projectileNetworkObject.GetComponent<Projectile>());
    }

    [ServerRpc(RequireOwnership = false)]
    protected void InitializeProjectileServerRpc(int projectileIndex, int iTargetableIndex) {
        InitializeProjectileClientRpc(projectileIndex, iTargetableIndex);
    }

    [ClientRpc]
    protected void InitializeProjectileClientRpc(int projectileIndex, int iTargetableIndex) {
        Projectile projectile = projectilePool[projectileIndex];
        ITargetable attackTarget = BattleManager.Instance.GetITargetableFromKey(iTargetableIndex);

        projectile.ActivateAndInitialize(this, attackTarget);
    }

    #endregion

    protected void RandomizeAttackTimers() {
        if (IsServer) {
            attackTimerServer.Value = UnityEngine.Random.Range(0, attackRate / 4);
            attackStartTimerServer.Value = UnityEngine.Random.Range(attackRate / 6, attackRate / 3);
        }
    }

    protected void ResetAttackTimers() {
        attackTimer = attackTimerServer.Value;
        attackStartTimer = attackStartTimerServer.Value;
        attackEndTimer = attackRate;
    }

    private void AttackTimerServer_OnValueChanged(float previousValue, float newValue) {
        attackTimer = attackTimerServer.Value;
    }
    private void AttackStartTimerServer_OnValueChanged(float previousValue, float newValue) {
        attackStartTimer = attackStartTimerServer.Value;

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
        OnUnitDecomposedAttackEnded?.Invoke(this, EventArgs.Empty);
    }


    #endregion

    #region EVENT RESPONSES

    protected void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if (BattleManager.Instance.IsBattlePhaseEnding()) {
            if (!unit.gameObject.activeInHierarchy) return;

            ResetAttackTimers();
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

        if (unitAI.IsDead()) {
            if (unit.GetUnitSO().deathTriggerAttackSO != null) {
                // Death trigger attacks
                deadTimer = 0f;
            triggerDeathAttack = true;
            activeAttackSO = unit.GetUnitSO().deathTriggerAttackSO;
            }
        }

    }

    private void TriggerDeathTriggerAttacks() {
        foreach (Unit unitAOETarget in FindAOEAttackTargets(transform.position, unit.GetUnitSO().deathTriggerAttackSO.attackAOE)) {
            InflictDamage(unitAOETarget, this.transform.position);
        }
        triggerDeathAttack = false;
    }

    #endregion

    #region BUFFS
    public void SetAttackRateMultiplier(float attackRateMultiplier) {
        this.attackRateMultiplier = attackRateMultiplier;
        UpdateActiveAttackParameters(activeAttackSO);
    }
    public void SetAttackDamageMultiplier(float attackDamageMultiplier) {
        this.attackDamageMultiplier = attackDamageMultiplier;
        UpdateActiveAttackParameters(activeAttackSO);
    }
    public void SetAttackKnockbackAbsolute(float attackKnockbackAbsolute) {
        this.attackKnockbackBuffAbsolute = attackKnockbackAbsolute;
        UpdateActiveAttackParameters(activeAttackSO);
        UpdateActiveAttackParameters(activeAttackSO);
    }
    #endregion

    #region SET PARAMETERS

    public void SetAttackTarget(ITargetable attackTarget) {
        if (attackTarget as MonoBehaviour != null) {
            if (attackTarget == this.attackTarget) return;

            //NetworkObject targetNetworkObject = (attackTarget as MonoBehaviour).GetComponent<NetworkObject>();
            int attackTargetIndex = BattleManager.Instance.GetITargetableKey(attackTarget);

            SetAttackTargetServerRpc(attackTargetIndex);
        }
    }

    public void SetAttackTimer(float attackTimer) {
        this.attackTimer = attackTimer;
    }

    public void ResetAttackTarget() {
        attackTarget = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAttackTargetServerRpc(int iTargetableIndex) {
        SetAttackTargetClientRpc(iTargetableIndex);
    }

    [ClientRpc]
    public void SetAttackTargetClientRpc(int iTargetableIndex) {
        attackTarget = BattleManager.Instance.GetITargetableFromKey(iTargetableIndex);

        if (IsServer) return;
        Vector3 moveDir = (attackTarget as MonoBehaviour).transform.position - transform.position;
        //unitMovement.SetMoveDir(moveDir);
        //Debug.Log(unit + " SetAttackTargetClientRpc " + iTargetableIndex + " "  + (attackTarget as MonoBehaviour));
    }

    public void SetActiveAttackSO(AttackSO attackSO) {
        if(attackSO == activeAttackSO) return;

        activeAttackSO = attackSO;

        if(attackSO == unit.GetUnitSO().mainAttackSO) {
            OnMainAttackActivated?.Invoke(this, EventArgs.Empty);
        }
        if(attackSO == unit.GetUnitSO().sideAttackSO) {
            OnSideAttackActivated?.Invoke(this, EventArgs.Empty);
        }

        UpdateActiveAttackParameters(attackSO);
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
