using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlefieldBorder : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<Unit>() != null) {
            Unit unitExitingBattlefield = collision.gameObject.GetComponent<Unit>();
            unitExitingBattlefield.RemoveUnitFromBattlePhaseUnitList();
        }
    }
}
