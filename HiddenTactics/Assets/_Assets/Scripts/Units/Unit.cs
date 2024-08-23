using Modern2D;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : NetworkBehaviour, ITargetable {

    public event EventHandler OnUnitUpgraded;
    public event EventHandler OnUnitSpawned;
    public event EventHandler OnUnitPlaced;
    public event EventHandler OnUnitDied;
    public static event EventHandler OnAnyUnitDied;
    public event EventHandler OnUnitReset;
    public event EventHandler OnUnitFell;
    public event EventHandler OnUnitSold;
    public event EventHandler OnUnitSetAsAdditionalUnit;
    public event EventHandler OnAdditionalUnitActivated;
    public event EventHandler OnUnitDynamicallySpawned;

    public event EventHandler OnUnitHovered;
    public event EventHandler OnUnitUnhovered;
    public event EventHandler OnUnitSelectedFromTroop;
    public event EventHandler OnSingleUnitSelected;
    public event EventHandler OnUnitUnselected;

    public event EventHandler<OnUnitSpecialEventArgs> OnUnitDazed;
    public event EventHandler<OnUnitSpecialEventArgs> OnUnitFlamed;
    public event EventHandler<OnUnitSpecialEventArgs> OnUnitPoisoned;
    public event EventHandler<OnUnitSpecialEventArgs> OnUnitScared;
    public event EventHandler<OnUnitSpecialEventArgs> OnUnitWebbed;
    public event EventHandler OnUnitFlamedEnded;
    public event EventHandler OnUnitPoisonedEnded;
    public event EventHandler OnUnitScaredEnded;
    public event EventHandler OnUnitWebbedEnded;


    [SerializeField] private Transform projectileTarget;
    [SerializeField] private UnitVisual unitVisual;
    [SerializeField] private UnitUI unitUI;
    public class OnUnitSpecialEventArgs : EventArgs {
        public float effectDuration;
    }

    private bool burning;
    private float burningTimer;
    private float burningDuration;

    private bool poisoned;
    private float poisonedTimer;
    private float poisonedDuration;
    private NetworkVariable<float> poisonedDurationNormalized = new NetworkVariable<float>();
    private NetworkVariable<float> scaredDurationNormalized = new NetworkVariable<float>();
    private NetworkVariable<float> burningDurationNormalized = new NetworkVariable<float>();
    private float poisonedMaxDuration = 10f;

    private bool scared;
    private float scaredTimer;
    private float scaredDuration;

    private bool webbed;
    private float webbedTimer;
    private float webbedDuration;

    [SerializeField] protected UnitSO unitSO;
    protected Troop parentTroop;
    protected Building parentBuilding;

    protected GridPosition initialUnitGridPosition;
    protected GridPosition currentGridPosition;
    protected Vector3 unitPositionInTroop;
    protected Vector3 initialUnitPosition;
    public event EventHandler OnUnitChangedGridPosition;

    protected Rigidbody2D rb;
    protected Collider2D collider2d;
    protected bool unitIsDead;
    protected bool unitIsPlaced;
    protected bool isAdditionalUnit;
    protected bool isSpawnedUnit;
    protected bool unitIsBought;
    protected bool unitIsOnlyVisual;
    protected bool isSelected;

    protected virtual void Awake() {
        collider2d = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        collider2d.enabled = false;

        rb.mass = unitSO.mass;
    }

    protected virtual void Start() {
        if(!unitIsOnlyVisual)
        {
            BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        }
    }

    protected void Update() {
        if (unitIsOnlyVisual) return;
        HandlePositionOnGrid();

        if(IsServer && BattleManager.Instance.IsBattlePhase()) {
            HandlePositionSyncServerRpc(transform.position);
        }

        if(IsServer) {
            HandleStatusEffects();
        }

    }

    protected void HandleStatusEffects() {
        if(burning) {
            burningTimer += Time.deltaTime;

            if(burningTimer >= burningDuration) {
                burningTimer = 0;
                burning = false;
                RemoveSpecial(AttackSO.UnitAttackSpecial.fire);
            }
            burningDurationNormalized.Value = (burningDuration - burningTimer) / burningDuration;
        }

        if (scared) {
            scaredTimer += Time.deltaTime;

            if (scaredTimer >= scaredDuration) {
                scaredTimer = 0;
                scared = false;
                RemoveSpecial(AttackSO.UnitAttackSpecial.fear);
            }

            scaredDurationNormalized.Value = (scaredDuration - scaredTimer) / scaredDuration;
        }

        if (webbed) {
            webbedTimer += Time.deltaTime;

            if (webbedTimer >= webbedDuration) {
                webbedTimer = 0;
                webbed = false;
                RemoveSpecial(AttackSO.UnitAttackSpecial.webbed);
            }
        }

        if (poisoned) {
            poisonedTimer += Time.deltaTime;

            if (poisonedTimer >= poisonedDuration) {
                poisonedTimer = 0;
                poisoned = false;
                RemoveSpecial(AttackSO.UnitAttackSpecial.poison);
            }

            poisonedDurationNormalized.Value = (poisonedDuration - poisonedTimer) / poisonedMaxDuration;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void HandlePositionSyncServerRpc(Vector3 position) {
        HandlePositionSyncClientRpc(position);
    }

    [ClientRpc]
    protected void HandlePositionSyncClientRpc(Vector3 position) {
        if (!IsServer) {
            // Mirror x position 
            transform.position = new Vector3(position.x + (BattleGrid.Instance.GetBattlefieldMiddlePoint() - position.x) * 2, position.y, 0);
        }
    }

    public void HandlePositionOnGrid() {
        GridPosition newGridPosition = BattleGrid.Instance.GetGridPosition(transform.position);

        // Grid position is not a valid position
        if (!BattleGrid.Instance.IsValidGridPosition(newGridPosition)) return;

        // Unit was not set at a grid position yet
        if (currentGridPosition == null) {

            currentGridPosition = BattleGrid.Instance.GetGridPosition(transform.position);
            BattleGrid.Instance.AddUnitAtGridPosition(currentGridPosition, this);
        }

        // Unit changed grid position
        if (newGridPosition != currentGridPosition) {
            BattleGrid.Instance.UnitMovedGridPosition(this, currentGridPosition, newGridPosition);
            currentGridPosition = newGridPosition;
            OnUnitChangedGridPosition?.Invoke(this, EventArgs.Empty);
        }

    }

    protected void BattleManager_OnStateChanged(object sender, EventArgs e) {

        if(BattleManager.Instance.IsBattlePhaseEnding()) {
            ResetUnit();
        }
    }

    protected virtual void ParentTroop_OnTroopPlaced(object sender, System.EventArgs e) {

        if(unitSO.doesNotMoveGarrisonedUnit) {
            collider2d.enabled = false;
        }

        if(parentTroop.GetTroopSO().isGarrisonedTroop) {
            SetParentBuilding();
        }

        currentGridPosition = parentTroop.GetIPlaceableGridPosition();
        GridPosition newGridPosition = BattleGrid.Instance.GetGridPosition(transform.position);
        BattleGrid.Instance.UnitMovedGridPosition(this, currentGridPosition, newGridPosition);
        currentGridPosition = newGridPosition;

        if (!isAdditionalUnit && !isSpawnedUnit) {
            unitIsPlaced = true;
            collider2d.enabled = true;
            OnUnitPlaced?.Invoke(this, EventArgs.Empty);
        }

    }

    public void SetParentBuilding() {
        GridPosition parentTroopCenterPointGridPosition = BattleGrid.Instance.GetGridPosition(parentTroop.GetTroopCenterPoint());
        Building parentBuilding = BattleGrid.Instance.GetBuildingAtGridPosition(parentTroopCenterPointGridPosition);
        this.parentBuilding = parentBuilding;
        parentTroop.SetParentBuilding(parentBuilding);
    }

    public virtual void ResetUnit() {
        transform.localPosition = unitPositionInTroop;
        unitIsDead = false;

        if (!unitSO.doesNotMoveGarrisonedUnit & unitIsBought) {
            collider2d.enabled = true;
        }

        RemoveAllSpecialEffects();
        OnUnitReset?.Invoke(this, EventArgs.Empty);
    }

    public virtual void UpgradeUnit() {
        OnUnitUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnUnit() {
        OnUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void TakeKnockBack(Vector2 force) {
        rb.AddForce(force);
    }

    public void TakeDazed(float dazedTime) {
        OnUnitDazed?.Invoke(this, new OnUnitSpecialEventArgs {
            effectDuration = dazedTime
        });
    }

    public void TakeSpecial(AttackSO.UnitAttackSpecial specialEffect, float duration) {
        TakeSpecialServerRpc(specialEffect, duration);
    }

    [ServerRpc]
    private void TakeSpecialServerRpc(AttackSO.UnitAttackSpecial specialEffect, float duration) {
        TakeSpecialClientRpc(specialEffect, duration);
    }

    [ClientRpc]
    private void TakeSpecialClientRpc(AttackSO.UnitAttackSpecial specialEffect, float duration) {
        if (specialEffect == AttackSO.UnitAttackSpecial.fire) {
            burning = true;
            burningTimer = 0f;
            burningDuration = duration;
            OnUnitFlamed?.Invoke(this, new OnUnitSpecialEventArgs {
                effectDuration = duration
            });
        }

        if (specialEffect == AttackSO.UnitAttackSpecial.fear) {
            scared = true;
            scaredTimer = 0f;
            scaredDuration = duration;
            OnUnitScared?.Invoke(this, new OnUnitSpecialEventArgs {
                effectDuration = duration
            });
        }

        if (specialEffect == AttackSO.UnitAttackSpecial.webbed) {
            if(!webbed) {
                OnUnitWebbed?.Invoke(this, new OnUnitSpecialEventArgs {
                    effectDuration = duration
                });
            }

            webbed = true;
            webbedTimer = 0f;
            webbedDuration = duration;
        }

        if (specialEffect == AttackSO.UnitAttackSpecial.poison) {
            poisoned = true;
            poisonedTimer = 0f;
            poisonedDuration += duration;

            if(poisonedDuration > poisonedMaxDuration) {
                poisonedDuration = poisonedMaxDuration;
            }

            OnUnitPoisoned?.Invoke(this, new OnUnitSpecialEventArgs {
                effectDuration = duration
            });
        }
    }

    public void RemoveAllSpecialEffects() {
        RemoveSpecial(AttackSO.UnitAttackSpecial.fire);
        RemoveSpecial(AttackSO.UnitAttackSpecial.fear);
        RemoveSpecial(AttackSO.UnitAttackSpecial.ice);
        RemoveSpecial(AttackSO.UnitAttackSpecial.shock);
        RemoveSpecial(AttackSO.UnitAttackSpecial.bleed);
        RemoveSpecial(AttackSO.UnitAttackSpecial.poison);
        RemoveSpecial(AttackSO.UnitAttackSpecial.webbed);
    }

    public void RemoveSpecial(AttackSO.UnitAttackSpecial specialEffect) {
        RemoveSpecialServerRpc(specialEffect);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveSpecialServerRpc(AttackSO.UnitAttackSpecial specialEffect) {
        RemoveSpecialClientRpc(specialEffect);
    }

    [ClientRpc]
    private void RemoveSpecialClientRpc(AttackSO.UnitAttackSpecial specialEffect) {
        if (specialEffect == AttackSO.UnitAttackSpecial.fear) {
            OnUnitScaredEnded?.Invoke(this, EventArgs.Empty);
        }

        if (specialEffect == AttackSO.UnitAttackSpecial.fire) {
            OnUnitFlamedEnded?.Invoke(this, EventArgs.Empty);
        }

        if (specialEffect == AttackSO.UnitAttackSpecial.poison) {
            OnUnitPoisonedEnded?.Invoke(this, EventArgs.Empty);
        }

        if (specialEffect == AttackSO.UnitAttackSpecial.webbed) {
            OnUnitWebbedEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    public virtual void Die() {
        OnUnitDied?.Invoke(this, EventArgs.Empty);
        OnAnyUnitDied?.Invoke(this, EventArgs.Empty);
        unitIsDead = true;
        collider2d.enabled = false;

        RemoveUnitFromBattlePhaseUnitList();
        RemoveAllSpecialEffects();
    }

    public void Fall() {
        OnUnitFell?.Invoke(this, EventArgs.Empty);
        unitIsDead = true;
        collider2d.enabled = false;
        RemoveUnitFromBattlePhaseUnitList();
        RemoveAllSpecialEffects();
    }

    public void SellUnit() {
        OnUnitSold?.Invoke(this, EventArgs.Empty);
        collider2d.enabled = false;
    }

    public void RemoveUnitFromBattlePhaseUnitList() {
        if (IsServer) {
            StartCoroutine(RemoveUnitFromBattlePhaseUnitListCoroutine());
        }
    }

    private IEnumerator RemoveUnitFromBattlePhaseUnitListCoroutine() {
        yield return new WaitForSeconds(2f);
        BattleManager.Instance.RemoveUnitFromUnitsStillInBattleList(this);
    }

    public void SetUnitHovered(bool hovered) {
        if(hovered) {
            OnUnitHovered?.Invoke(this, EventArgs.Empty);
        } else {
            OnUnitUnhovered?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetUnitSelected(bool selected) {

        if(selected) {
            if (isSelected) return;
            OnUnitSelectedFromTroop?.Invoke(this, EventArgs.Empty);
            isSelected = true;
        } else {
            if (!isSelected) return;
            OnUnitUnselected?.Invoke(this, EventArgs.Empty);
            isSelected = false;
        }
    }

    public void SetSingleUnitSelected(bool selected) {
        if (selected) {
            OnSingleUnitSelected?.Invoke(this, EventArgs.Empty);
            isSelected = true;
        }
        else {
            OnUnitUnselected?.Invoke(this, EventArgs.Empty);
            isSelected = false;
        }
    }

    #region GET PARAMETERS

    public Vector3 GetUnitPositionInTroop() {
        return unitPositionInTroop;
    }

    public bool GetIsDead() {
        return unitIsDead;
    }

    public bool GetUnitIsPlaced() {
        return unitIsPlaced;
    }

    public bool GetUnitIsBought() {
        return unitIsBought;
    }

    public bool GetUnitIsAdditionalUnit() {
        return isAdditionalUnit;
    }

    public bool GetUnitIsDynamicallySpawnedUnit() {
        return isSpawnedUnit;
    }

    public bool GetIsPoisoned() {
        return poisoned;
    }

    public float GetPoisonedDurationNormalized() {
        return poisonedDurationNormalized.Value;
    }

    public float GetScaredDurationNormalized() {
        return scaredDurationNormalized.Value;
    }

    public float GetBurningDurationNormalized() {
        return burningDurationNormalized.Value;
    }

    public float GetPoisonedRemainingTime() {
        return poisonedDuration - poisonedTimer;
    }

    public float GetScaredRemainingTime() {
        return scaredDuration - scaredTimer;
    }

    public float GetBurningRemainingTime() {
        return burningDuration - burningTimer;
    }

    public UnitSO GetUnitSO() {
        return unitSO;
    }

    public UnitUI GetUnitUI() {
        return unitUI;
    }

    public Troop GetParentTroop() {
        return parentTroop;
    }

    public ITargetable.TargetType GetTargetType() {
        return unitSO.unitTargetType;
    }

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public GridPosition GetCurrentGridPosition() {
        return currentGridPosition;
    }

    public GridPosition GetInitialUnitGridPosition() {
        return initialUnitGridPosition;
    }

    public Vector3 GetInitialUnitPosition() {
        return initialUnitPosition;
    }

    public bool IsOwnedByPlayer() {
        return parentTroop.IsOwnedByPlayer();
    }

    public IDamageable GetIDamageable() {
        return GetComponent<UnitHP>();
    }

    public UnitVisual GetUnitVisual() {
        return unitVisual;
    }

    public bool GetScared() {
        return scared;
    }

    #endregion

    #region SET PARAMETERS

    public void SetParentTroop(Troop parentTroop) {
        this.parentTroop = parentTroop;
        parentTroop.OnTroopPlaced += ParentTroop_OnTroopPlaced;

        parentTroop.AddUnitToUnitInTroopList(this);
    }

    public void SetLocalPosition(Vector3 positionInTroop, bool debugMode) {
        if(parentTroop.IsOwnedByPlayer()) {
            unitPositionInTroop = positionInTroop;
        } else {
            //Mirror x position in troop
            float mirroredPositionX = positionInTroop.x + (parentTroop.GetTroopCenterPoint().x - positionInTroop.x) * 2;
            unitPositionInTroop = new Vector3(mirroredPositionX, positionInTroop.y, 0);
        }

        if (debugMode) {
            transform.position = unitPositionInTroop;
            unitPositionInTroop = transform.position - parentTroop.transform.position;
            BattleGrid.Instance.AddUnitAtGridPosition(currentGridPosition,this);
        } else {
            transform.localPosition = unitPositionInTroop;
        }
    }

    public void SetInitialUnitPosition(GridPosition gridPosition) {
        initialUnitGridPosition = gridPosition;
        initialUnitPosition = transform.position;
    }

    public void SetUnitAsAdditionalUnit() {
        isAdditionalUnit = true;
        parentTroop.AddUnitToAdditionalUnitsInTroopList(this);

        OnUnitSetAsAdditionalUnit?.Invoke(this, EventArgs.Empty);
        
        unitVisual.gameObject.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<UnitMovement>().enabled = false;
        GetComponent<UnitAI>().enabled = false;
        GetComponent<UnitAttack>().enabled = false;
        GetComponent<UnitTargetingSystem>().enabled = false;
        GetComponent<UnitHP>().enabled = false;
    }

    public void SetUnitAsSpawnedUnit() {
        isSpawnedUnit = true;
        unitIsBought = false;
        parentTroop.AddUnitToSpawnedUnitsInTroopList(this);

        unitVisual.gameObject.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<UnitMovement>().enabled = false;
        GetComponent<UnitAI>().enabled = false;
        GetComponent<UnitAttack>().enabled = false;
        GetComponent<UnitTargetingSystem>().enabled = false;
        GetComponent<UnitHP>().enabled = false;

        OnUnitSetAsAdditionalUnit?.Invoke(this, EventArgs.Empty);
    }

    public void SetUnitAsSpawnedUnit(bool isSpawnedUnit) {
        // Ony for wolf because he is not DYNAMICALLY spawned
        this.isSpawnedUnit = isSpawnedUnit;
    }


    #endregion

    public void InvokeOnUnitPlaced() {
        OnUnitPlaced?.Invoke(this, EventArgs.Empty);
    }

    protected void InvokeOnUnitReset() {
        OnUnitReset?.Invoke(this, EventArgs.Empty);
    }

    public void ActivateAdditionalUnit() {
        ActivateAdditionalUnitServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateAdditionalUnitServerRpc() {
        if (!BattleManager.Instance.GetUnitsInBattlefieldList().Contains(this)) {
            BattleManager.Instance.AddUnitToUnitListInBattlefield(this);
        }
        ActivateAdditionalUnitClientRpc();
    }

    [ClientRpc]
    private void ActivateAdditionalUnitClientRpc() {
        unitIsBought = true;
        unitIsPlaced = true;

        if (PlayerAction_SelectIPlaceable.LocalInstance.IsTroopSelected(parentTroop)) {
            OnUnitSelectedFromTroop?.Invoke(this, EventArgs.Empty);
        }

        OnAdditionalUnitActivated?.Invoke(this, EventArgs.Empty);

        unitVisual.gameObject.SetActive(true);
        GetComponent<Collider2D>().enabled = true;
        GetComponent<UnitMovement>().enabled = true;
        GetComponent<UnitAI>().enabled = true;
        GetComponent<UnitAttack>().enabled = true;
        GetComponent<UnitTargetingSystem>().enabled = true;
        GetComponent<UnitHP>().enabled = true;
    }

    public void ActivateSpawnedUnit() {
        ActivateSpawnedUnitServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateSpawnedUnitServerRpc() {

        if (!BattleManager.Instance.GetUnitsInBattlefieldList().Contains(this)) {
            BattleManager.Instance.AddUnitToUnitListInBattlefield(this);
            unitPositionInTroop = transform.localPosition;
        }

        ActivateSpawnedUnitClientRpc();
    }

    [ClientRpc]
    private void ActivateSpawnedUnitClientRpc() {
        unitIsBought = true;

        unitVisual.gameObject.SetActive(true);
        GetComponent<Collider2D>().enabled = true;
        GetComponent<UnitMovement>().enabled = true;
        GetComponent<UnitAI>().enabled = true;
        GetComponent<UnitAttack>().enabled = true;
        GetComponent<UnitTargetingSystem>().enabled = true;
        GetComponent<UnitHP>().enabled = true;

        currentGridPosition = BattleGrid.Instance.GetGridPosition(transform.position);
        BattleGrid.Instance.AddUnitAtGridPosition(currentGridPosition, this);

        OnUnitDynamicallySpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeactivateDynamicallySpawnedUnit() {
        if (IsServer) {
            if (!BattleManager.Instance.GetUnitsInBattlefieldList().Contains(this)) {
                BattleManager.Instance.RemoveUnitFromUnitListInBattlefield(this);
            }
        }

        DeactivateDynamicallySpawnedUnitServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeactivateDynamicallySpawnedUnitServerRpc() {
        DeactivateDynamicallySpawnedUnitClientRpc();
    }

    [ClientRpc]
    private void DeactivateDynamicallySpawnedUnitClientRpc() {
        SetUnitAsSpawnedUnit();
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    public void DebugModeStartFunction() {
        OnUnitPlaced?.Invoke(this, EventArgs.Empty);
        unitIsBought = true;
        collider2d.enabled = true;
    }

    public void DisableCollider() {
        collider2d.enabled = false;
    }

    public void EnableCollider() {
        collider2d.enabled = true;
    }

    public void SetUnitAsVisual()
    {
        unitIsOnlyVisual = true;
        GetComponent<UnitMovement>().enabled = false;
        GetComponent<UnitTargetingSystem>().enabled = false;
        GetComponent<UnitAttack>().enabled = false;
        GetComponent<NetworkObject>().enabled = false;
        GetComponent<UnitHP>().enabled = false;
        GetComponent<SimpleSmoothModifier>().enabled = false;
        GetComponentInChildren<UnitUI>().enabled = false;
        GetComponentInChildren<UnitAI>().enabled = false;

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if(child.GetComponent<StylizedShadowCaster2D>() != null)
            {
                child.GetComponent<StylizedShadowCaster2D>().enabled = false;
            }
        }
    }

    public bool GetUnitIsOnlyVisual()
    {
        return unitIsOnlyVisual;
    }

    public override void OnDestroy() {
        base.OnDestroy();

        if (unitIsOnlyVisual) return;

        BattleManager.Instance.RemoveUnitFromUnitListInBattlefield(this);
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }

    public bool GetIsServer() {
        return IsServer;
    }

}
