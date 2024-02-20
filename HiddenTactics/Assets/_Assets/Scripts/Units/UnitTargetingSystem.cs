using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTargetingSystem : MonoBehaviour
{

    private Unit unit;
    private List<GridPosition> gridPositionAttackTargetList;
    private List<Unit> meleeTargetUnitList = new List<Unit>();

    private float meleeAttackTargetingRange;
    private float meleeAttackRange;

    private Unit meleeTargetUnit;
    private float distanceToClosestTargetUnit;
    private bool meleeTargetUnitIsInRange;

    private float targetingRate = .2f;
    private float targetingTimer;

    private void Awake() {
        unit = GetComponent<Unit>();
        meleeAttackTargetingRange = unit.GetUnitSO().mainAttackTargetingRange;
        meleeAttackRange = unit.GetUnitSO().mainAttackRange;
    }

    private void Start() {
        FillGridPositionMainAttackTargetList();
    }

    private void Update() {
        targetingTimer -= Time.deltaTime;

        if(targetingTimer < 0) {
            targetingTimer = targetingRate;
            FindMeleeAttackTargets();

            if(meleeTargetUnitList.Count > 0) {
                // There are potential targets within targeting range
                FindClosestMeleeAttackTarget();
            } else {
                meleeTargetUnit = null;
            }
        }
    }

    private void FindMeleeAttackTargets() {
        meleeTargetUnitList.Clear();
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, meleeAttackTargetingRange);

        foreach(Collider2D collider in colliderArray) {
            if(collider.TryGetComponent<Unit>(out Unit unit)) {
                // Collider is a unit

                if(unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.GetUnitIsDead()) {
                    // target unit is not from the same team AND Unit is not dead

                    if(unit.GetUnitCurrentGridPosition().y == this.unit.GetUnitCurrentGridPosition().y) {
                        // target unit is on the same row as this unit
                        meleeTargetUnitList.Add(unit);
                    }
                }
            };
        }
    }

    private void FindClosestMeleeAttackTarget() {
        float distanceToClosestTargetUnit = float.MaxValue;

        foreach(Unit targetUnit in meleeTargetUnitList) {
            float distanceToMeleeTargetUnit = Vector3.Distance(transform.position, targetUnit.transform.position);
            if(distanceToMeleeTargetUnit < distanceToClosestTargetUnit) {
                distanceToClosestTargetUnit = distanceToMeleeTargetUnit;
                meleeTargetUnit = targetUnit;

                if(distanceToClosestTargetUnit < meleeAttackRange) {
                    meleeTargetUnitIsInRange = true;
                } else {
                    meleeTargetUnitIsInRange = false;
                }
            }
        }

        this.distanceToClosestTargetUnit = distanceToClosestTargetUnit;
    }

    public void FillGridPositionMainAttackTargetList() {
        gridPositionAttackTargetList = new List<GridPosition>();

        foreach (Vector2 mainAttackTargetTiles in unit.GetUnitSO().mainAttackTargetTiles) {
            gridPositionAttackTargetList.Add(new GridPosition((int)mainAttackTargetTiles.x, (int)mainAttackTargetTiles.y));
        }
    }

    public List<Unit> GetUnitMeleeTargets() {
        return meleeTargetUnitList;
    }

    public Unit GetMeleeTargetUnit() {
        return meleeTargetUnit; 
    }

    public float GetClosestTargetDistance() {
        return distanceToClosestTargetUnit;
    }

    public bool GetMeleeTargetUnitIsInRange() { return meleeTargetUnitIsInRange; }


}
