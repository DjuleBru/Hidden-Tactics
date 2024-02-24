using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : NetworkBehaviour
{
    public event EventHandler OnUnitUpgraded;
    public event EventHandler OnUnitSpawned;
    public event EventHandler OnUnitPlaced;
    public event EventHandler<OnHealthChangedEventArgs> OnHealthChanged;
    public event EventHandler OnUnitDied;
    public event EventHandler OnUnitReset;
    public event EventHandler<OnUnitDazedEventArgs> OnUnitDazed;
    public event EventHandler OnUnitSetAsAdditionalUnit;
    public event EventHandler OnAdditionalUnitBought;

    [SerializeField] private Transform projectileTarget;

    public class OnUnitDazedEventArgs : EventArgs {
        public float dazedTime;
    }

    public class OnHealthChangedEventArgs : EventArgs {
        public float previousHealth;
        public float newHealth;
    }

    [SerializeField] protected bool hasAttack;
    [SerializeField] protected bool hasSideAttack;
    [SerializeField] protected bool hasSpecial1;
    [SerializeField] protected bool hasSpecial2;

    [SerializeField] protected UnitSO unitSO;
    private Troop parentTroop;

    private GridPosition currentGridPosition;
    private Vector3 unitPositionInTroop;

    private int unitHP;
    private int unitArmor;

    private Rigidbody2D rb;
    private Collider2D collider2d;
    private bool unitIsDead;
    private bool unitIsBought;

    protected virtual void Awake() {
        collider2d = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        rb.mass = unitSO.mass;

        unitHP = unitSO.HP;
        unitArmor = unitSO.armor;

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
    protected void ParentTroop_OnTroopPlaced(object sender, System.EventArgs e) {
        OnUnitPlaced?.Invoke(this, EventArgs.Empty);
    }

    public virtual void ResetUnit() {
        if (unitIsBought) {
            transform.localPosition = unitPositionInTroop;
            unitIsDead = false;
            collider2d.enabled = true;
            unitHP = unitSO.HP;

            OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs {
                previousHealth = 0,
                newHealth = unitSO.HP
            });

            OnUnitReset?.Invoke(this, EventArgs.Empty);
        }
    }

    public virtual void UpgradeUnit() {
        OnUnitUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnUnit() {
        OnUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void TakeDamage(int damage) {
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void TakeDamageServerRpc(int damage) {
        TakeDamageClientRpc(damage);
    }

    [ClientRpc]
    protected void TakeDamageClientRpc(int damage) {
        unitHP -= (damage - unitArmor);

        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs {
            previousHealth = unitHP + (damage - unitArmor),
            newHealth = unitHP
        });

        if (unitHP < 0) {
            Die();
        }
    }

    public void TakeKnockBack(Vector2 force) {
        rb.AddForce(force);
        //TakeKnockBackServerRpc(force);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void TakeKnockBackServerRpc(Vector2 force) {
        TakeKnockBackClientRpc(force);
    }

    [ClientRpc]
    protected void TakeKnockBackClientRpc(Vector2 force) {

        if (!IsServer) {
            // Mirror force on x axis
            force.x = -force.x;
        }
        rb.AddForce(force);
    }

    public void TakeDazed(float dazedTime) {
        OnUnitDazed?.Invoke(this, new OnUnitDazedEventArgs {
            dazedTime = dazedTime
        });
    }

    protected void Die() {
        OnUnitDied?.Invoke(this, EventArgs.Empty);
        unitIsDead = true;
        collider2d.enabled = false;
    }



    #region GET PARAMETERS

    public bool GetUnitIsDead() {
        return unitIsDead;
    }
    public bool GetUnitIsBought() {
        return unitIsBought;
    }
    public float GetUnitHPNormalized() {
        return (float)unitHP / (float)unitSO.HP;
    }

    public bool GetHasAttack()
    {
        return hasAttack;
    }

    public bool GetHasSpecial1()
    {
        return hasSpecial1;
    }

    public bool GetHasSpecial2()
    {
        return hasSpecial2;
    }

    public bool GetHasSideAttack()
    {
        return hasSideAttack;
    }

    public UnitSO GetUnitSO() {
        return unitSO;
    }

    public Troop GetParentTroop() {
        return parentTroop;
    }

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public GridPosition GetUnitCurrentGridPosition() {
        return currentGridPosition;
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


    public void BuyAdditionalUnit() {
        collider2d.enabled = true;
        unitIsBought = true;
        OnAdditionalUnitBought?.Invoke(this, EventArgs.Empty);
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }
}
