using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitTargetingSystem : NetworkBehaviour
{

    private Unit unit;
    private List<GridPosition> gridPositionAttackTargetList;
    private List<Unit> targetUnitList = new List<Unit>();

    private AttackSO.AttackType mainAttackType;

    private float meleeAttackTargetingRange;
    private float meleeAttackRange;

    private Unit targetUnit;
    private float distanceToClosestTargetUnit;
    private bool targetUnitIsInRange;

    private float targetingRate = .05f;
    private float targetingTimer;

    private void Awake() {
        unit = GetComponent<Unit>();

        meleeAttackTargetingRange = unit.GetUnitSO().mainAttackSO.meleeAttackTargetingRange;
        meleeAttackRange = unit.GetUnitSO().mainAttackSO.meleeAttackRange;

        mainAttackType = unit.GetUnitSO().mainAttackSO.attackType;
    }

    private void Start() {
        FillGridPositionAttackTargetList();
    }


    private void Update() {
        if (!BattleManager.Instance.IsBattlePhase()) return;

        targetingTimer -= Time.deltaTime;

        if(targetingTimer < 0) {
            targetingTimer = targetingRate;
            FindAttackTargets();

            if(targetUnitList.Count > 0) {
                // There are potential targets within targeting range
                FindClosestAttackTarget();
            } else {
                targetUnit = null;
            }
        }
    }

    private void FindAttackTargets() {
        targetUnitList.Clear();

        if(mainAttackType == AttackSO.AttackType.melee) {
            FindMeleeAttackTargets();
            return;
        }

        if (mainAttackType == AttackSO.AttackType.ranged) {
            FindRangedAttackTargets();
            return;
        }

    }

    private void FindMeleeAttackTargets() {
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, meleeAttackTargetingRange);

        foreach (Collider2D collider in colliderArray) {
            if (collider.TryGetComponent<Unit>(out Unit unit)) {
                // Collider is a unit

                if (unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.UnitIsDead()) {
                    // target unit is not from the same team AND Unit is not dead

                    if (unit.GetUnitCurrentGridPosition().y == this.unit.GetUnitCurrentGridPosition().y) {
                        // target unit is on the same row as this unit
                        targetUnitList.Add(unit);
                    }
                }
            };
        }
    }

    private void FindRangedAttackTargets() {
        foreach(GridPosition relativeTargetGridPosition in gridPositionAttackTargetList) {
            GridPosition targetGridPosition = new GridPosition(unit.GetUnitCurrentGridPosition().x + relativeTargetGridPosition.x, unit.GetUnitCurrentGridPosition().y + relativeTargetGridPosition.y);
            List<Unit> unitListAtTargetGridPosition = BattleGrid.Instance.GetUnitListAtGridPosition(targetGridPosition);

            foreach(Unit unit in unitListAtTargetGridPosition) {
                if (unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.UnitIsDead()) {
                    // target unit is not from the same team AND Unit is not dead
                    targetUnitList.Add(unit);
                }
            }
        }
    }

    private void FindClosestAttackTarget() {
        float distanceToClosestTargetUnit = float.MaxValue;

        foreach(Unit targetUnit in targetUnitList) {
            float distanceToTargetUnit = Vector3.Distance(transform.position, targetUnit.transform.position);

            if(distanceToTargetUnit < distanceToClosestTargetUnit) {
                distanceToClosestTargetUnit = distanceToTargetUnit;
                this.targetUnit = targetUnit;
            }
        }

        this.distanceToClosestTargetUnit = distanceToClosestTargetUnit;
    }

    public void FillGridPositionAttackTargetList() {
        gridPositionAttackTargetList = new List<GridPosition>();

        foreach (Vector2 mainAttackTargetTiles in unit.GetUnitSO().mainAttackSO.attackTargetTiles) {
            gridPositionAttackTargetList.Add(new GridPosition((int)mainAttackTargetTiles.x, (int)mainAttackTargetTiles.y));
        }
    }

    public List<Unit> GetUnitMeleeTargets() {
        return targetUnitList;
    }

    public Unit GetTargetUnit() {
        return targetUnit; 
    }

    public float GetClosestTargetDistance() {
        return distanceToClosestTargetUnit;
    }

    public bool GetTargetUnitIsInRange() {
        if (mainAttackType == AttackSO.AttackType.melee) {
            // MELEE ATTACK : check if unit is in range

            if (distanceToClosestTargetUnit < meleeAttackRange) {
                targetUnitIsInRange = true;
            }
            else {
                targetUnitIsInRange = false;
            }
        }
        if (mainAttackType == AttackSO.AttackType.ranged) {
            targetUnitIsInRange = true;
        }
        return targetUnitIsInRange; 
    }

    public AttackSO.AttackType GetMainAttackType() { 
        return mainAttackType; 
    }

}
