using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public event EventHandler OnUnitUpgraded;
    public event EventHandler OnUnitSpawned;

    [SerializeField] protected bool hasAttack;
    [SerializeField] protected bool hasSideAttack;
    [SerializeField] protected bool hasSpecial1;
    [SerializeField] protected bool hasSpecial2;

    [SerializeField] protected UnitSO unitSO;
    private Troop parentTroop;

    private GridPosition currentBattleGridPosition;
    private GridPosition currentPreparationGridPosition;

    protected virtual void Awake() {
        parentTroop = GetComponentInParent<Troop>();
    }

    private void Start() {
        currentBattleGridPosition = BattleGrid.Instance.GetGridPosition(transform.position);
        BattleGrid.Instance.AddUnitAtGridPosition(currentBattleGridPosition, this);
    }

    private void Update() {
        HandlePositionOnGrid();
    }

    public void HandlePositionOnGrid() {
        GridPosition newGridPosition = BattleGrid.Instance.GetGridPosition(transform.position);
        if (newGridPosition != currentBattleGridPosition) {
            // Unit changed grid position
            BattleGrid.Instance.UnitMovedGridPosition(this, currentBattleGridPosition, newGridPosition);
            currentBattleGridPosition = newGridPosition;
        }
    }

    public virtual void UpgradeUnit() {
        OnUnitUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnUnit() {
        OnUnitSpawned?.Invoke(this, EventArgs.Empty);
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
}
