using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingVisual : MonoBehaviour
{
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material placingMaterial;
    [SerializeField] List<SpriteRenderer> buildingSpriteRendererList;

    private Building building;

    private void Awake() {
        building = GetComponentInParent<Building>();
    }

    private void Start() {
        building.OnBuildingPlaced += Building_OnBuildingPlaced;
        building.OnBuildingDestroyed += Building_OnBuildingDestroyed;

        if(!building.GetBuildingIsOnlyVisual())
        {
            foreach (SpriteRenderer spriteRenderer in buildingSpriteRendererList)
            {
                spriteRenderer.material = placingMaterial;
            }
        }
    }

    private void Building_OnBuildingDestroyed(object sender, System.EventArgs e) {
        gameObject.SetActive(false);
    }

    private void Building_OnBuildingPlaced(object sender, System.EventArgs e) {
        foreach (SpriteRenderer spriteRenderer in buildingSpriteRendererList) {
            spriteRenderer.material = cleanMaterial;
        }
    }
}
