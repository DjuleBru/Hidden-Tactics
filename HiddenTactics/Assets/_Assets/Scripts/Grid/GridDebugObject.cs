using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridDebugObject : MonoBehaviour
{
    private GridObject gridObject;

    [SerializeField] TextMeshProUGUI gridPositionDebugText;
    [SerializeField] TextMeshProUGUI troopDebugText;
    [SerializeField] TextMeshProUGUI unitDebugText;

    public void SetGridObject(GridObject gridObject) {
        this.gridObject = gridObject;
    }

    private void Update() {
        gridPositionDebugText.text = gridObject.ToString();

        string troopString = "";
        string unitString = "";

        if (gridObject.GetTroopList().Count != 0) {
            foreach(Troop troop in gridObject.GetTroopList()) {
                troopString += troop + "\n";
            }
            troopDebugText.text = troopString;
        } else {
            troopDebugText.text = "";
        }

        if(gridObject.GetUnitList().Count != 0) {
            foreach (Unit unit in gridObject.GetUnitList()) {
                unitString += unit + "\n";
            }
            unitDebugText.text = unitString;
        }
        else {
            unitDebugText.text = "";
        }
    }
}
