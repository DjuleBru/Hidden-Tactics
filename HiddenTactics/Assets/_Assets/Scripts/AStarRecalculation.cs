using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarRecalculation : MonoBehaviour
{

    public static AStarRecalculation Instance;

    private void Awake() {
        Instance = this;
    }
    public void RecalculateGraph() {
        // Recalculate all graphs
        AstarPath.active.Scan();
    }
}
