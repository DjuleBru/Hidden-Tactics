using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsBlockedByBuilding : Conditional
{
    private Unit unit;

    public override void OnAwake() {
        unit = GetComponent<Unit>();
    }
    public override TaskStatus OnUpdate() {

        // Determine nextGridPosition in function of unit belonging
        GridPosition nextGridPosition = new GridPosition(0, 0);

        if (unit.IsOwnedByPlayer()) {
            nextGridPosition = new GridPosition(unit.GetCurrentGridPosition().x + 1, unit.GetCurrentGridPosition().y);
        }
        else {
            nextGridPosition = new GridPosition(unit.GetCurrentGridPosition().x - 1, unit.GetCurrentGridPosition().y);
        }

        if (BattleGrid.Instance.IsValidGridPosition(nextGridPosition)) {
            // This GridPosition is a valid grid position
            Building building = BattleGrid.Instance.GetBuildingAtGridPosition(nextGridPosition);

            if (building != null) {

                if (building.GetBuildingSO().buildingBlocksUnitMovement && (building.IsOwnedByPlayer()) == unit.IsOwnedByPlayer()) {
                    // Building blocks unit movement AND is owned by the same player
                    return TaskStatus.Success;
                }
            }
            else {
                return TaskStatus.Failure;
            }
        }
        return TaskStatus.Failure;
    }
}
