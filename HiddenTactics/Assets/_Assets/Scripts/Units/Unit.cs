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
    public event EventHandler OnUnitReset;
    public event EventHandler<OnUnitDazedEventArgs> OnUnitDazed;
    public event EventHandler OnUnitSetAsAdditionalUnit;
    public event EventHandler OnAdditionalUnitBought;

    [SerializeField] private Transform projectileTarget;

    public class OnUnitDazedEventArgs : EventArgs {
        public float dazedTime;
    }

    [SerializeField] protected UnitSO unitSO;
    protected Troop parentTroop;
    protected Building parentBuilding;

    protected GridPosition currentGridPosition;
    protected Vector3 unitPositionInTroop;

    protected Rigidbody2D rb;
    protected Collider2D collider2d;
    protected bool unitIsDead;
    protected bool unitIsBought;

    protected virtual void Awake() {
        collider2d = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        collider2d.enabled = false;

        rb.mass = unitSO.mass;
        unitIsBought = true;
    }

    protected virtual void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    protected void Update() {
        HandlePositionOnGrid();
        if(IsServer && BattleManager.Instance.IsBattlePhase()) {
            HandlePositionSyncServerRpc(transform.position);
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
        }
    }

    protected void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseEnding()) {
            ResetUnit();
        }
    }
    protected virtual void ParentTroop_OnTroopPlaced(object sender, System.EventArgs e) {
        OnUnitPlaced?.Invoke(this, EventArgs.Empty);

        if(unitIsBought && !unitSO.isGarrisonedUnit) {
            collider2d.enabled = true;
        }

        if(unitSO.isGarrisonedUnit) {
            SetParentBuilding();
        }
    }

    protected void SetParentBuilding() {
        Building parentBuilding = BattleGrid.Instance.GetBuildingAtGridPosition(currentGridPosition);
        this.parentBuilding = parentBuilding;
        parentBuilding.OnBuildingDestroyed += ParentBuilding_OnBuildingDestroyed;
    }

    private void ParentBuilding_OnBuildingDestroyed(object sender, EventArgs e) {
        HiddenTacticsMultiplayer.Instance.DestroyIPlaceable(parentTroop.GetComponent<NetworkObject>());
    }

    public virtual void ResetUnit() {
        if (unitIsBought) {
            transform.localPosition = unitPositionInTroop;
            unitIsDead = false;

            if(!unitSO.isGarrisonedUnit) {
                collider2d.enabled = true;
            }

            OnUnitReset?.Invoke(this, EventArgs.Empty);
        }
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
        OnUnitDazed?.Invoke(this, new OnUnitDazedEventArgs {
            dazedTime = dazedTime
        });
    }

    public void Die() {
        OnUnitDied?.Invoke(this, EventArgs.Empty);
        unitIsDead = true;
        collider2d.enabled = false;
    }

    #region GET PARAMETERS

    public bool GetIsDead() {
        return unitIsDead;
    }
    public bool GetUnitIsBought() {
        return unitIsBought;
    }

    public UnitSO GetUnitSO() {
        return unitSO;
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

    public bool IsOwnedByPlayer() {
        return parentTroop.IsOwnedByPlayer();
    }

    public IDamageable GetIDamageable() {
        return GetComponent<UnitHP>();
    }

    #endregion

    #region SET PARAMETERS

    public void SetParentTroop(Troop parentTroop) {
        this.parentTroop = parentTroop;
        parentTroop.OnTroopPlaced += ParentTroop_OnTroopPlaced;
        parentTroop.AddUnitToUnitInTroopList(this);
    }

    public void SetPosition(Vector3 positionInTroop) {

        if(parentTroop.IsOwnedByPlayer()) {

            unitPositionInTroop = positionInTroop;
        } else {
            //Mirror x position in troop
            float mirroredPositionX = positionInTroop.x + (parentTroop.GetTroopCenterPoint().x - positionInTroop.x) * 2;
            unitPositionInTroop = new Vector3(mirroredPositionX, positionInTroop.y, 0);
        }

        transform.position = unitPositionInTroop;
    }

    public void SetUnitAsAdditionalUnit() {
        collider2d.enabled = false;
        unitIsBought = false;
        OnUnitSetAsAdditionalUnit?.Invoke(this, EventArgs.Empty);
    }


    #endregion

    protected void InvokeOnUnitPlaced() {
        OnUnitPlaced?.Invoke(this, EventArgs.Empty);
    }

    protected void InvokeOnUnitReset() {
        OnUnitReset?.Invoke(this, EventArgs.Empty);
    }

    public void BuyAdditionalUnit() {
        collider2d.enabled = true;
        unitIsBought = true;
        OnAdditionalUnitBought?.Invoke(this, EventArgs.Empty);
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    public void DebugModeStartFunction() {
        OnUnitPlaced?.Invoke(this, EventArgs.Empty);
        if (unitIsBought) {
            collider2d.enabled = true;
        }
    }
}
