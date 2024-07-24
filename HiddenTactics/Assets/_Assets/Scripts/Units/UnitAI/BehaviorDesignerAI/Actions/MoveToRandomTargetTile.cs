using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToRandomTargetTile : Action
{
    private UnitMovement unitMovement;
    private UnitAttack unitAttack;
    private Unit unit;

    public override void OnAwake() {
        unitMovement = GetComponent<UnitMovement>();
        unitAttack = GetComponent<UnitAttack>();
        unit = GetComponent<Unit>();
    }

    public override TaskStatus OnUpdate() {
        List<Vector2> gridPositionVectorList = unitAttack.GetActiveAttackSO().attackTargetTiles;
        List<GridPosition> gridPositionList = new List<GridPosition>();

        foreach(Vector2 gridPositionVector in gridPositionVectorList) {

            GridPosition unitGridPosition = unit.GetInitialUnitGridPosition();
            GridPosition absoluteGridPosition = new GridPosition((int)gridPositionVector.x + unitGridPosition.x, (int)gridPositionVector.y + unitGridPosition.y);

            gridPositionList.Add(absoluteGridPosition);
        }

        GridPosition randomGridPosition = gridPositionList[Random.Range(0, gridPositionList.Count)];

        Vector3 targetPosition = BattleGrid.Instance.GetWorldPosition(randomGridPosition) + new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 0);
        unitMovement.MoveToTarget(targetPosition);

        return TaskStatus.Success;
    }

}
