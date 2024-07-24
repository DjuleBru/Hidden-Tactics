using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTargetingSystem_TreeSpirits : UnitTargetingSystem
{
    protected override List<ITargetable> GetAllyRangedHealTargets(List<GridPosition> attackGridPositionTargetList, AttackSO attackSO) {

        int index = 0;

        List<ITargetable> targetItargetableList = new List<ITargetable>();

        foreach (GridPosition relativeTargetGridPosition in attackGridPositionTargetList) {
            GridPosition targetGridPosition = new GridPosition(unit.GetInitialUnitGridPosition().x + relativeTargetGridPosition.x, unit.GetInitialUnitGridPosition().y + relativeTargetGridPosition.y);

            if (!BattleGrid.Instance.IsValidGridPosition(targetGridPosition)) {
                // Target grid position is not a valid grid position
                continue;
            }

            List<Unit> unitListAtTargetGridPosition = BattleGrid.Instance.GetUnitListAtGridPosition(targetGridPosition);
            foreach (Unit unit in unitListAtTargetGridPosition) {
                if (unit.IsOwnedByPlayer() == this.unit.IsOwnedByPlayer() && unit != this.unit && !unit.GetIsDead() && unit.GetUnitIsBought() && attackSO.attackTargetTypes.Contains(unit.GetTargetType())) {

                    bool unitHasLostHP = unit.GetComponent<UnitHP>().GetHP() < unit.GetComponent<UnitHP>().GetMaxHP();
                    bool unitIsTreeSpirit = (unit.GetComponent<UnitTargetingSystem_TreeSpirits>() != null);
                    if (unitHasLostHP && !unitIsTreeSpirit) {
                        //  targetable has lost HP AND target can be targeted (air unit vs ground unit vs garrisoned unit, building, village)
                        targetItargetableList.Add(unit);
                    }
                }
            }

            index++;
        }
        return targetItargetableList;
    }
}
