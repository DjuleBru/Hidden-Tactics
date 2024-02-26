using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitTargetingSystem : NetworkBehaviour
{

    private Unit unit;
    private List<GridPosition> mainAttackGridPositionTargetList;
    private List<GridPosition> sideAttackGridPositionTargetList;

    private List<Unit> mainAttackTargetUnitList = new List<Unit>();
    private List<Unit> sideAttackTargetUnitList = new List<Unit>();

    private AttackSO mainAttackSO;
    private AttackSO sideAttackSO;

    private float mainAttackRange;

    private Unit mainAttackTargetUnit;
    private Unit sideAttackTargetUnit;

    private float distanceToClosestTargetUnit;
    private bool targetUnitIsInRange;

    private float targetingRate = .05f;
    private float targetingTimer;

    private void Awake() {
        unit = GetComponent<Unit>();

        mainAttackRange = unit.GetUnitSO().mainAttackSO.meleeAttackRange;

        mainAttackSO = unit.GetUnitSO().mainAttackSO;
        sideAttackSO = unit.GetUnitSO().sideAttackSO;
    }

    private void Start() {
        mainAttackGridPositionTargetList = FillGridPositionAttackTargetList(mainAttackSO);
        if(sideAttackSO != null) {
            sideAttackGridPositionTargetList = FillGridPositionAttackTargetList(sideAttackSO);
        }
    }

    private void Update() {
        if (!BattleManager.Instance.IsBattlePhase()) return;

        CheckIfTargetUnitsAreDead();

        targetingTimer -= Time.deltaTime;
        if(targetingTimer < 0) {
            targetingTimer = targetingRate;

            mainAttackTargetUnitList = FindAttackTargets(mainAttackSO, mainAttackGridPositionTargetList);
            if (sideAttackSO != null) {
                sideAttackTargetUnitList = FindAttackTargets(sideAttackSO, sideAttackGridPositionTargetList);
            }

            mainAttackTargetUnit = FindAttackTargetUnit(mainAttackSO, mainAttackTargetUnitList, mainAttackTargetUnit);

            if (sideAttackSO != null) {
                sideAttackTargetUnit = FindAttackTargetUnit(sideAttackSO, sideAttackTargetUnitList, sideAttackTargetUnit);
            }
        }
    }

    private void CheckIfTargetUnitsAreDead() {
        if (mainAttackTargetUnit != null) {
            if (mainAttackTargetUnit.GetUnitIsDead()) {
                mainAttackTargetUnit = null;
            }
        }

        if (sideAttackTargetUnit != null) {
            if (sideAttackTargetUnit.GetUnitIsDead()) {
                sideAttackTargetUnit = null;
            }
        }
    }

    private Unit FindAttackTargetUnit(AttackSO attackSO, List<Unit> targetUnitList, Unit targetUnit) {
        if (targetUnitList.Count > 0) {
            // There are potential targets within targeting range of attack

            if (attackSO.attackType == AttackSO.AttackType.melee) {
                // Melee attack : constantly re-evaluate attack target
                targetUnit = FindClosestAttackTarget(targetUnitList);
            }

            if (attackSO.attackType == AttackSO.AttackType.ranged && targetUnit == null) {
                // Ranged attack : keep same attack target
                targetUnit = FindRandomAttackTarget(targetUnitList);
            }
        } else {
            targetUnit = null;
        }

        return targetUnit;
    }

    private List<Unit> FindAttackTargets(AttackSO attackSO, List<GridPosition> rangedGridPositionTargetList) {
        List<Unit> targetUnitList = new List<Unit>();

        if(attackSO.attackType == AttackSO.AttackType.melee) {
            targetUnitList = GetMeleeAttackTargets(attackSO);
        }

        if (attackSO.attackType == AttackSO.AttackType.ranged) {
            targetUnitList = GetRangedAttackTargets(rangedGridPositionTargetList);
        }
        return targetUnitList;
    }

    private List<Unit> GetMeleeAttackTargets(AttackSO attackSO) {
        float attackTargetingRange = attackSO.meleeAttackTargetingRange;

        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, attackTargetingRange);
        List<Unit> targetUnitList = new List<Unit>();

        foreach (Collider2D collider in colliderArray) {
            if (collider.TryGetComponent<Unit>(out Unit targetUnit)) {
                // Collider is a unit

                if (targetUnit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !targetUnit.GetUnitIsDead()) {
                    // target unit is not from the same team AND Unit is not dead

                    if(attackSO.moveTypeAttackTargets.Contains(targetUnit.GetUnitSO().moveType)) {
                        // target unit can be targeted (air vs ground)

                        if (targetUnit.GetUnitCurrentGridPosition().y == this.unit.GetUnitCurrentGridPosition().y) {
                            // target unit is on the same row as this unit
                            targetUnitList.Add(targetUnit);
                        }
                    }
                }
            };
        }

        return targetUnitList;
    }

    private List<Unit> GetRangedAttackTargets(List<GridPosition> attackGridPositionTargetList) {
        int index = 0;

        List<Unit> targetUnitList = new List<Unit>();
        foreach (GridPosition relativeTargetGridPosition in attackGridPositionTargetList) {
            GridPosition targetGridPosition = new GridPosition(unit.GetUnitCurrentGridPosition().x + relativeTargetGridPosition.x, unit.GetUnitCurrentGridPosition().y + relativeTargetGridPosition.y);

            if(!BattleGrid.Instance.IsValidGridPosition(targetGridPosition)) {
                // Target grid position is not a valid grid position
                continue;
            }

            // PRIORORITY to closest targets(X) : Check if new targetGridPosition is located behind old target grid position.
            if(index != 0) {
                GridPosition previousRelativeTargetGridPosition = attackGridPositionTargetList[index -1];

                if (Mathf.Abs(previousRelativeTargetGridPosition .x) < Mathf.Abs(relativeTargetGridPosition.x) && targetUnitList.Count > 0) {
                    break;
                }
            }

            List<Unit> unitListAtTargetGridPosition = BattleGrid.Instance.GetUnitListAtGridPosition(targetGridPosition);

            foreach(Unit unit in unitListAtTargetGridPosition) {
                if (unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.GetUnitIsDead() && unit.GetUnitIsBought()) {
                    // target unit is not from the same team AND Unit is not dead AND unit is bought

                    targetUnitList.Add(unit);
                }
            }

            index++;
        }
        return targetUnitList;
    }

    private Unit FindClosestAttackTarget(List<Unit> targetUnitList) {
        float distanceToClosestTargetUnit = float.MaxValue;

        Unit closestUnit = null;
        foreach(Unit targetUnit in targetUnitList) {
            ColliderDistance2D distanceBetweenUnitColliders = Physics2D.Distance(gameObject.GetComponent<Collider2D>(), targetUnit.gameObject.GetComponent<Collider2D>());
            float distanceToTargetUnit = distanceBetweenUnitColliders.distance;

            if (distanceToTargetUnit < distanceToClosestTargetUnit) {
                distanceToClosestTargetUnit = distanceToTargetUnit;
                closestUnit = targetUnit;
            }
        }

        this.distanceToClosestTargetUnit = distanceToClosestTargetUnit;
        return closestUnit;
    }

    private Unit FindRandomAttackTarget(List<Unit> targetUnitList) {
        return targetUnitList[Random.Range(0, targetUnitList.Count)];
    }

    public List<GridPosition> FillGridPositionAttackTargetList(AttackSO attackSO) {
        if(attackSO.attackType == AttackSO.AttackType.melee) {
            return null;
        }

        List<GridPosition> gridPositionAttackTargetList = new List<GridPosition>();

        int xMultiplier = 1;
        // Reverse x if unit is opponent troop
        if(!unit.GetParentTroop().IsOwnedByPlayer()) {
            xMultiplier = -1;
        }

        foreach (Vector2 mainAttackTargetTiles in attackSO.attackTargetTiles) {
            gridPositionAttackTargetList.Add(new GridPosition((int)mainAttackTargetTiles.x * xMultiplier, (int)mainAttackTargetTiles.y));
        }

        return gridPositionAttackTargetList;
    }

    #region GET PARAMETERS
    public Unit GetMainAttackTargetUnit() {
        return mainAttackTargetUnit; 
    }

    public Unit GetSideAttackTargetUnit() {
        return sideAttackTargetUnit;
    }

    public float GetClosestTargetDistance() {
        return distanceToClosestTargetUnit;
    }

    public bool GetTargetUnitIsInRange(AttackSO attackSO) {
        if (attackSO.attackType == AttackSO.AttackType.melee) {
            // MELEE ATTACK : check if unit is in range

            if (distanceToClosestTargetUnit < attackSO.meleeAttackRange) {
                targetUnitIsInRange = true;
            }
            else {
                targetUnitIsInRange = false;
            }
        }

        if (attackSO.attackType == AttackSO.AttackType.ranged) {
            targetUnitIsInRange = true;
        }

        return targetUnitIsInRange; 
    }
    #endregion

}
