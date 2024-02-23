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

                if (mainAttackType == AttackSO.AttackType.melee) {
                    // Melee attack : constantly re-evaluate attack target
                    FindClosestAttackTarget();
                    return;
                }

                if (mainAttackType == AttackSO.AttackType.ranged && targetUnit == null) {
                    // Ranged attack : keep same attack target
                    FindRandomAttackTarget();
                    return;
                }

            } else {
                targetUnit = null;
            }
        }
    }

    private void FindAttackTargets() {
        targetUnitList.Clear();

        if(mainAttackType == AttackSO.AttackType.melee) {
            UpdateMeleeAttackTargets();
            return;
        }

        if (mainAttackType == AttackSO.AttackType.ranged) {
            UpdateRangedAttackTargets();
            return;
        }

    }

    private void UpdateMeleeAttackTargets() {
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, meleeAttackTargetingRange);

        foreach (Collider2D collider in colliderArray) {
            if (collider.TryGetComponent<Unit>(out Unit unit)) {
                // Collider is a unit

                if (unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.GetUnitIsDead()) {
                    // target unit is not from the same team AND Unit is not dead

                    if (unit.GetUnitCurrentGridPosition().y == this.unit.GetUnitCurrentGridPosition().y) {
                        // target unit is on the same row as this unit
                        targetUnitList.Add(unit);
                    }
                }
            };
        }
    }

    private void UpdateRangedAttackTargets() {
        int index = 0;
        foreach (GridPosition relativeTargetGridPosition in gridPositionAttackTargetList) {
            GridPosition targetGridPosition = new GridPosition(unit.GetUnitCurrentGridPosition().x + relativeTargetGridPosition.x, unit.GetUnitCurrentGridPosition().y + relativeTargetGridPosition.y);

            if(!BattleGrid.Instance.IsValidGridPosition(targetGridPosition)) {
                // Target grid position is not a valid grid position
                continue;
            }

            // PRIORORITY to closest targets(X) : Check if new targetGridPosition is located behind old target grid position.
            if(index != 0) {
                GridPosition previousRelativeTargetGridPosition = gridPositionAttackTargetList[index -1];

                if (Mathf.Abs(previousRelativeTargetGridPosition .x) < Mathf.Abs(relativeTargetGridPosition.x) && targetUnitList.Count > 0) {
                    break;
                }
            }

            List<Unit> unitListAtTargetGridPosition = BattleGrid.Instance.GetUnitListAtGridPosition(targetGridPosition);

            foreach(Unit unit in unitListAtTargetGridPosition) {

                if(unit.GetUnitIsDead()) {
                    targetUnit = null;
                }

                if (unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.GetUnitIsDead() && unit.GetUnitIsBought()) {
                    // target unit is not from the same team AND Unit is not dead AND unit is bought

                    targetUnitList.Add(unit);
                }
            }

            index++;
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

    private void FindRandomAttackTarget() {
        targetUnit = targetUnitList[Random.Range(0, targetUnitList.Count)];
    }

    public void FillGridPositionAttackTargetList() {
        gridPositionAttackTargetList = new List<GridPosition>();

        int xMultiplier = 1;
        // Reverse x if unit is opponent troop
        if(!unit.GetParentTroop().IsOwnedByPlayer()) {
            xMultiplier = -1;
        }

        foreach (Vector2 mainAttackTargetTiles in unit.GetUnitSO().mainAttackSO.attackTargetTiles) {
            gridPositionAttackTargetList.Add(new GridPosition((int)mainAttackTargetTiles.x * xMultiplier, (int)mainAttackTargetTiles.y));
        }
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
