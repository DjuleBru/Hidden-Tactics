using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitTargetingSystem : NetworkBehaviour
{

    private Unit unit;
    private List<GridPosition> gridPositionAttackTargetList;
    private List<Unit> targetUnitList = new List<Unit>();

    private float meleeAttackTargetingRange;
    private float meleeAttackRange;

    private Unit targetUnit;
    private float distanceToClosestTargetUnit;
    private bool meleeTargetUnitIsInRange;

    private float targetingRate = .05f;
    private float targetingTimer;

    private void Awake() {
        unit = GetComponent<Unit>();
        meleeAttackTargetingRange = unit.GetUnitSO().mainAttackSO.attackDamage;
        meleeAttackRange = unit.GetUnitSO().mainAttackSO.attackDamage;
    }

    private void Start() {
        FillGridPositionMainAttackTargetList();
    }


    private void Update() {
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


        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, meleeAttackTargetingRange);

        foreach(Collider2D collider in colliderArray) {
            if(collider.TryGetComponent<Unit>(out Unit unit)) {
                // Collider is a unit

                if(unit.GetParentTroop().IsOwnedByPlayer() != this.unit.GetParentTroop().IsOwnedByPlayer() && !unit.UnitIsDead()) {
                    // target unit is not from the same team AND Unit is not dead

                    if(unit.GetUnitCurrentGridPosition().y == this.unit.GetUnitCurrentGridPosition().y) {
                        // target unit is on the same row as this unit
                        targetUnitList.Add(unit);
                    }
                }
            };
        }

    }

    private void FindClosestAttackTarget() {
        float distanceToClosestTargetUnit = float.MaxValue;

        foreach(Unit targetUnit in targetUnitList) {
            float distanceToMeleeTargetUnit = Vector3.Distance(transform.position, targetUnit.transform.position);
            if(distanceToMeleeTargetUnit < distanceToClosestTargetUnit) {
                distanceToClosestTargetUnit = distanceToMeleeTargetUnit;
                this.targetUnit = targetUnit;

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

        foreach (Vector2 mainAttackTargetTiles in unit.GetUnitSO().mainAttackSO.attackTargetTiles) {
            gridPositionAttackTargetList.Add(new GridPosition((int)mainAttackTargetTiles.x, (int)mainAttackTargetTiles.y));
        }
    }

    public List<Unit> GetUnitMeleeTargets() {
        return targetUnitList;
    }

    public Unit GetMeleeTargetUnit() {
        return targetUnit; 
    }

    public float GetClosestTargetDistance() {
        return distanceToClosestTargetUnit;
    }

    public bool GetMeleeTargetUnitIsInRange() { return meleeTargetUnitIsInRange; }


}
