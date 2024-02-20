using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public event EventHandler OnUnitUpgraded;
    public event EventHandler OnUnitSpawned;
    public event EventHandler OnUnitPlaced;
    public event EventHandler OnHealthChanged;
    public event EventHandler OnUnitDied;
    public event EventHandler OnUnitReset;
    public event EventHandler<OnUnitDazedEventArgs> OnUnitDazed;

    public class OnUnitDazedEventArgs : EventArgs {
        public float dazedTime;
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

    protected virtual void Awake() {
        parentTroop = GetComponentInParent<Troop>();
        collider2d = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        unitPositionInTroop = transform.localPosition;

        unitHP = unitSO.HP;
        unitArmor = unitSO.armor;
    }

    protected virtual void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        parentTroop.OnTroopPlaced += ParentTroop_OnTroopPlaced;
    }

    private void Update() {
        HandlePositionOnGrid();
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

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseEnding()) {
            ResetUnit();
        }
    }
    private void ParentTroop_OnTroopPlaced(object sender, System.EventArgs e) {
        OnUnitPlaced?.Invoke(this, EventArgs.Empty);
    }
    public virtual void ResetUnit() {
        transform.localPosition = unitPositionInTroop;
        unitHP = unitSO.HP;
        unitIsDead = false;

        OnHealthChanged?.Invoke(this, EventArgs.Empty);
        OnUnitReset?.Invoke(this, EventArgs.Empty);
    }

    public virtual void UpgradeUnit() {
        OnUnitUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnUnit() {
        OnUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void TakeDamage(int damage) {
        unitHP -= (damage - unitArmor);
        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        if (unitHP < 0) {
            OnUnitDied?.Invoke(this, EventArgs.Empty);
            Die();
        }
    }

    public void TakeKnockBack(float knockback, Vector3 damageSourcePosition) {
        Vector2 incomingDamageDirection = new Vector2(transform.position.x - damageSourcePosition.x, transform.position.y - damageSourcePosition.y);
        Vector2 force = incomingDamageDirection * knockback;
        rb.AddForce(force);
    }

    public void TakeDazed(float dazedTime) {
        OnUnitDazed?.Invoke(this, new OnUnitDazedEventArgs {
            dazedTime = dazedTime
        });
    } 

    private void Die() {
        unitIsDead = true;
        collider2d.enabled = false;
    }

    public bool GetUnitIsDead() {
        return unitIsDead;
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

    public GridPosition GetUnitCurrentGridPosition() {
        return currentGridPosition;
    }
}
