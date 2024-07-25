using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitTargetingSystem : NetworkBehaviour
{

    protected Unit unit;
    protected List<GridPosition> mainAttackGridPositionTargetList;
    protected List<GridPosition> sideAttackGridPositionTargetList;
    protected List<GridPosition> specialAttackGridPositionTargetList;

    protected List<ITargetable> mainAttackITargerableList = new List<ITargetable>();
    protected List<ITargetable> sideAttackITargetableList = new List<ITargetable>();
    protected List<ITargetable> specialAttackITargetableList = new List<ITargetable>();

    public enum AttackMode {
        mainAttack,
        sideAttack,
        specialAttack,
    }

    protected AttackSO mainAttackSO;
    protected AttackSO sideAttackSO;
    protected AttackSO specialAttackSO;

    protected ITargetable mainAttackITargetable;
    protected ITargetable sideAttackITargetable;
    protected ITargetable specialAttackITargetable;

    protected float distanceToClosestTargetITargetable;
    protected bool ITargetableIsInRange;

    protected float targetingRate = .05f;
    protected float targetingTimer;

    protected void Awake() {
        unit = GetComponent<Unit>();

        mainAttackSO = unit.GetUnitSO().mainAttackSO;
        sideAttackSO = unit.GetUnitSO().sideAttackSO;
        specialAttackSO = unit.GetUnitSO().specialAttackSO;
    }

    protected void Start() {

        mainAttackGridPositionTargetList = FillGridPositionAttackTargetList(mainAttackSO);
        if(sideAttackSO != null) {
            sideAttackGridPositionTargetList = FillGridPositionAttackTargetList(sideAttackSO);
        }
        if(specialAttackSO != null) {
            specialAttackGridPositionTargetList = FillGridPositionAttackTargetList(specialAttackSO);
        }
    }

    protected void Update() {
        if (!BattleManager.Instance.IsBattlePhase()) return;

        CheckIfTargetUnitsAreDead();

        targetingTimer -= Time.deltaTime;
        if(targetingTimer < 0) {
            targetingTimer = targetingRate;

            mainAttackITargerableList = FindAttackTargetList(mainAttackSO, mainAttackGridPositionTargetList);
            if (sideAttackSO != null) {
                sideAttackITargetableList = FindAttackTargetList(sideAttackSO, sideAttackGridPositionTargetList);
            }
            if (specialAttackSO != null) {
                specialAttackITargetableList = FindAttackTargetList(specialAttackSO, specialAttackGridPositionTargetList);
            }

            mainAttackITargetable = FindAttackTarget(mainAttackSO, mainAttackITargerableList, mainAttackITargetable);

            if (sideAttackSO != null) {
                sideAttackITargetable = FindAttackTarget(sideAttackSO, sideAttackITargetableList, sideAttackITargetable);
            }
            if (specialAttackSO != null) {
                specialAttackITargetable = FindAttackTarget(specialAttackSO, specialAttackITargetableList, specialAttackITargetable);
            }
        }
    }

    protected void CheckIfTargetUnitsAreDead() {
        if (mainAttackITargetable != null) {
            if (mainAttackITargetable.GetIsDead()) {
                mainAttackITargetable = null;
            }
        }

        if (sideAttackITargetable != null) {
            if (sideAttackITargetable.GetIsDead()) {
                sideAttackITargetable = null;
            }
        }

        if (specialAttackITargetable != null) {
            if (specialAttackITargetable.GetIsDead()) {
                specialAttackITargetable = null;
            }
        }
    }

    protected ITargetable FindAttackTarget(AttackSO attackSO, List<ITargetable> targetList, ITargetable target) {
        if (targetList.Count > 0) {
            // There are potential targets within targeting range of attack

            if (attackSO.attackType == AttackSO.AttackType.melee) {
                // Melee attack : constantly re-evaluate attack target
                target = FindClosestAttackTarget(targetList);
            }

            if (attackSO.attackType == AttackSO.AttackType.healAllyMeleeTargeting) {
                // Melee attack : constantly re-evaluate attack target
                target = FindClosestAttackTarget(targetList);
            }

            if (attackSO.attackType == AttackSO.AttackType.healAllyRangedTargeting) {
                //  heal : constantly re-evaluate attack target
                target = FindClosestAttackTarget(targetList);
            }

            // Ranged attack : keep same attack target if still in range
            if (attackSO.attackType == AttackSO.AttackType.ranged) {

                if(target == null || !targetList.Contains(target)) {
                    target = FindRandomAttackTarget(targetList);
                }
            }

        } else {
            target = null;
        }

        return target;
    }

    protected List<ITargetable> FindAttackTargetList(AttackSO attackSO, List<GridPosition> rangedGridPositionTargetList) {
        List<ITargetable> targetUnitList = new List<ITargetable>();

        if(attackSO.attackType == AttackSO.AttackType.melee) {
            targetUnitList = GetMeleeAttackTargetList(attackSO);
        }

        if (attackSO.attackType == AttackSO.AttackType.healAllyMeleeTargeting) {
            targetUnitList = GetAllyMeleeHealTargets(attackSO);
        }

        if (attackSO.attackType == AttackSO.AttackType.healAllyRangedTargeting) {
            targetUnitList = GetAllyRangedHealTargets(rangedGridPositionTargetList, attackSO);
        }

        if (attackSO.attackType == AttackSO.AttackType.ranged) {
            targetUnitList = GetRangedAttackTargets(rangedGridPositionTargetList, attackSO);
        }

        return targetUnitList;
    }

    protected List<ITargetable> GetMeleeAttackTargetList(AttackSO attackSO) {
        float attackTargetingRange = attackSO.meleeAttackTargetingRange;
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, attackTargetingRange);
        List<ITargetable> targetItargetableList = new List<ITargetable>();

        foreach (Collider2D collider in colliderArray) {
            if (collider.TryGetComponent<ITargetable>(out ITargetable targetITargetable)) {
                // Collider is targetable


                if (targetITargetable.IsOwnedByPlayer() != unit.IsOwnedByPlayer() && unit.GetUnitIsBought() && !targetITargetable.GetIsDead()) {
                    // targetable is not from the same team AND targetable is not dead


                    if (attackSO.attackTargetTypes.Contains(targetITargetable.GetTargetType())) {
                        // target can be targeted (air unit vs ground unit vs garrisoned unit, building, village)


                        if (targetITargetable.GetCurrentGridPosition().y == unit.GetCurrentGridPosition().y) {
                            // target unit is on the same row as this unit
                            targetItargetableList.Add(targetITargetable);
                        }
                    }
                }
            };
        }

        return targetItargetableList;
    }

    protected List<ITargetable> GetRangedAttackTargets(List<GridPosition> attackGridPositionTargetList, AttackSO attackSO) {
        int index = 0;

        List<ITargetable> targetItargetableList = new List<ITargetable>();

        foreach (GridPosition relativeTargetGridPosition in attackGridPositionTargetList) {
            GridPosition targetGridPosition = new GridPosition(unit.GetCurrentGridPosition().x + relativeTargetGridPosition.x, unit.GetCurrentGridPosition().y + relativeTargetGridPosition.y);

            if(!BattleGrid.Instance.IsValidGridPosition(targetGridPosition)) {
                // Target grid position is not a valid grid position
                continue;
            }

            // PRIORORITY to closest targets(X) : Check if new targetGridPosition is located behind old target grid position.
            if(index != 0) {
                GridPosition previousRelativeTargetGridPosition = attackGridPositionTargetList[index -1];

                if (Mathf.Abs(previousRelativeTargetGridPosition .x) < Mathf.Abs(relativeTargetGridPosition.x) && targetItargetableList.Count > 0) {
                    break;
                }
            }

            List<Unit> unitListAtTargetGridPosition = BattleGrid.Instance.GetUnitListAtGridPosition(targetGridPosition);
            List<Building> buildingListAtTargetGridPosition = BattleGrid.Instance.GetBuildingListAtGridPosition(targetGridPosition);

            foreach (Unit unit in unitListAtTargetGridPosition) {
                if (unit.IsOwnedByPlayer() != this.unit.IsOwnedByPlayer() && !unit.GetIsDead() && unit.GetUnitIsBought() && attackSO.attackTargetTypes.Contains(unit.GetTargetType())) {
                    // target unit is not from the same team AND Unit is not dead AND unit is bought AND unit is targetable (air vs ground)
                    targetItargetableList.Add(unit);
                }
            }

            foreach(Building buildingAtTargetGridPosition in buildingListAtTargetGridPosition) {
                if (buildingAtTargetGridPosition.IsOwnedByPlayer() != unit.IsOwnedByPlayer() && attackSO.attackTargetTypes.Contains(buildingAtTargetGridPosition.GetTargetType())) {
                    // target building is not from the same team AND building is targetable
                    targetItargetableList.Add(buildingAtTargetGridPosition);
                }
            }
            
            index++;
        }
        return targetItargetableList;
    }
    protected virtual List<ITargetable> GetAllyRangedHealTargets(List<GridPosition> attackGridPositionTargetList, AttackSO attackSO) {
        
        int index = 0;

        List<ITargetable> targetItargetableList = new List<ITargetable>();

        foreach (GridPosition relativeTargetGridPosition in attackGridPositionTargetList) {
            GridPosition targetGridPosition = new GridPosition(unit.GetCurrentGridPosition().x + relativeTargetGridPosition.x, unit.GetCurrentGridPosition().y + relativeTargetGridPosition.y);

            if (!BattleGrid.Instance.IsValidGridPosition(targetGridPosition)) {
                // Target grid position is not a valid grid position
                continue;
            }

            // PRIORORITY to closest targets(X) : Check if new targetGridPosition is located behind old target grid position.
            if (index != 0) {
                GridPosition previousRelativeTargetGridPosition = attackGridPositionTargetList[index - 1];

                if (Mathf.Abs(previousRelativeTargetGridPosition.x) < Mathf.Abs(relativeTargetGridPosition.x) && targetItargetableList.Count > 0) {
                    break;
                }
            }

            List<Unit> unitListAtTargetGridPosition = BattleGrid.Instance.GetUnitListAtGridPosition(targetGridPosition);
            foreach (Unit unit in unitListAtTargetGridPosition) {
                if (unit.IsOwnedByPlayer() == this.unit.IsOwnedByPlayer() && unit != this.unit && !unit.GetIsDead() && unit.GetUnitIsBought() && attackSO.attackTargetTypes.Contains(unit.GetTargetType())) {

                    bool unitHasLostHP = unit.GetComponent<UnitHP>().GetHP() < unit.GetComponent<UnitHP>().GetMaxHP();

                    if (unitHasLostHP) {
                        //  targetable has lost HP AND target can be targeted (air unit vs ground unit vs garrisoned unit, building, village)
                        targetItargetableList.Add(unit);
                    }
                }
            }

            index++;
        }
        return targetItargetableList;
    }

    protected List<ITargetable> GetAllyMeleeHealTargets(AttackSO attackSO) {
        float attackTargetingRange = attackSO.meleeAttackTargetingRange;

        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, attackTargetingRange);
        List<ITargetable> targetItargetableList = new List<ITargetable>();

        foreach (Collider2D collider in colliderArray) {
            if (collider.TryGetComponent<ITargetable>(out ITargetable targetITargetable)) {
                // Collider is targetable

                if((targetITargetable as MonoBehaviour).GetComponent<Unit>()  != null) {
                    // targetable is a unit
                    Unit targetUnit = (targetITargetable as MonoBehaviour).GetComponent<Unit>();

                    if (targetITargetable.IsOwnedByPlayer() == unit.IsOwnedByPlayer() && unit.GetUnitIsBought() && !targetITargetable.GetIsDead() && targetUnit != this.unit) {
                        // targetable is from the same team AND targetable is not dead

                        bool unitHasLostHP = targetUnit.GetComponent<UnitHP>().GetHP() < targetUnit.GetComponent<UnitHP>().GetMaxHP();
                        
                        if (unitHasLostHP && attackSO.attackTargetTypes.Contains(targetITargetable.GetTargetType())) {
                            //  targetable has lost HP AND target can be targeted (air unit vs ground unit vs garrisoned unit, building, village)

                            targetItargetableList.Add(targetITargetable);
                        }
                    }
                }

            };
        }

        return targetItargetableList;
    }

    protected ITargetable FindClosestAttackTarget(List<ITargetable> targetList) {
        float distanceToClosestTarget = float.MaxValue;

        ITargetable closestTarget = null;

        foreach(ITargetable target in targetList) {
            ColliderDistance2D distanceBetweenUnitColliders = Physics2D.Distance(gameObject.GetComponent<Collider2D>(), (target as MonoBehaviour).gameObject.GetComponent<Collider2D>());
            float distanceToTarget = distanceBetweenUnitColliders.distance;

            if (distanceToTarget < distanceToClosestTarget) {
                distanceToClosestTarget = distanceToTarget;
                closestTarget = target;
            }
        }

        this.distanceToClosestTargetITargetable = distanceToClosestTarget;
        return closestTarget;
    }

    protected ITargetable FindRandomAttackTarget(List<ITargetable> targetList) {
        return targetList[Random.Range(0, targetList.Count)];
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
    public ITargetable GetMainAttackTarget() {
        return mainAttackITargetable; 
    }

    public ITargetable GetSideAttackTarget() {
        return sideAttackITargetable;
    }

    public ITargetable GetSpecialAttackTarget() {
        return specialAttackITargetable;
    }

    public ITargetable GetRandomMainAttackTarget() {
        return mainAttackITargerableList[Random.Range(0, mainAttackITargerableList.Count)];
    }

    public ITargetable GetRandomSideAttackTarget() {
        return sideAttackITargetableList[Random.Range(0, sideAttackITargetableList.Count)];
    }

    public float GetClosestTargetDistance() {
        return distanceToClosestTargetITargetable;
    }

    public bool GetTargetUnitIsInRange(AttackSO attackSO) {
        if (attackSO.attackType == AttackSO.AttackType.melee) {
            // MELEE ATTACK : check if unit is in range

            if (distanceToClosestTargetITargetable < attackSO.meleeAttackRange) {
                ITargetableIsInRange = true;
            }
            else {
                ITargetableIsInRange = false;
            }
        }

        if (attackSO.attackType == AttackSO.AttackType.ranged) {
            ITargetableIsInRange = true;
        }

        return ITargetableIsInRange; 
    }

    #endregion

}
