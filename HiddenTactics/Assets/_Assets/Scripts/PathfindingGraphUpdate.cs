using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingGraphUpdate : MonoBehaviour
{
    private float graphRecalculationTimer;
    private float graphRecalculationRate = .25f;

    private void Update() {
        graphRecalculationTimer -= Time.deltaTime;

        if(graphRecalculationTimer < 0) {
            graphRecalculationTimer = graphRecalculationRate;
            // Recalculate all graphs
            AstarPath.active.Scan();
        }
    }

}
