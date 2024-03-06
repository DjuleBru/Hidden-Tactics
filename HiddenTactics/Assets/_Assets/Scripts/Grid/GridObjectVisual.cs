using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridObjectVisual : MonoBehaviour
{
    private GridObject gridObject;

    [SerializeField] TextMeshProUGUI gridPositionDebugText;
    [SerializeField] TextMeshProUGUI troopDebugText;
    [SerializeField] TextMeshProUGUI buildingDebugText;
    [SerializeField] TextMeshProUGUI unitDebugText;

    [SerializeField] bool showDebugInfo;
    [SerializeField] GameObject debugCanvas;

    [SerializeField] SpriteRenderer gridSprite;

    private Material cleanMaterial;
    [SerializeField] Material selectedMaterial;

    [SerializeField] List<SpriteRenderer> villageSpriteRenderers;

    private void Awake() {
        cleanMaterial = gridSprite.material;

        if(!showDebugInfo) {
            debugCanvas.SetActive(false);
        }
    }

    public void SetGridObject(GridObject gridObject) {
        this.gridObject = gridObject;
    }

    private void Update() {
        gridPositionDebugText.text = gridObject.ToString();

        string troopString = "";
        string buildingString = "";
        string unitString = "";

        if (gridObject.GetTroopList().Count != 0) {
            foreach(Troop troop in gridObject.GetTroopList()) {
                troopString += troop + "\n";
            }
            troopDebugText.text = troopString;
        } else {
            troopDebugText.text = "";
        }

        if (gridObject.GetBuildingList().Count != 0) {
            foreach (Building building in gridObject.GetBuildingList()) {
                buildingString += building + "\n";
            }
            buildingDebugText.text = buildingString;
        }
        else {
            buildingDebugText.text = "";
        }

        if (gridObject.GetUnitList().Count != 0) {
            foreach (Unit unit in gridObject.GetUnitList()) {
                unitString += unit + "\n";
            }
            unitDebugText.text = unitString;
        }
        else {
            unitDebugText.text = "";
        }
    }

    public void SetSelected() {
        gridSprite.material = selectedMaterial;
    }

    public void SetUnSelected() {
        gridSprite.material = cleanMaterial;
    }

    public void SetGridSprite(List<Sprite> sprites) {
        Sprite sprite = sprites[Random.Range(0, sprites.Count)];

        gridSprite.sprite = sprite;
    }

    public void SetVillageSprites(List<Sprite> villageSprites) {
        foreach(SpriteRenderer villageSpriteRenderer in villageSpriteRenderers) {
            Sprite sprite = villageSprites[Random.Range(0, villageSprites.Count)];

            villageSpriteRenderer.sprite = sprite;
        }
    }

}
