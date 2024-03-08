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
    [SerializeField] GameObject attackTargetGameObject;

    private Material cleanMaterial;
    [SerializeField] Material selectedMaterial;
    [SerializeField] Material targetTileMaterial;

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
        string unitString = "";

        if (gridObject.GetTroop() != null) {
            troopString += gridObject.GetTroop().ToString() + "\n";
            troopDebugText.text = troopString;
        } else {
            troopDebugText.text = "";
        }

        if (gridObject.GetBuilding() != null) {
            buildingDebugText.text = gridObject.GetBuilding().ToString();
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

    public void ResetVisual() {
        gridSprite.material = cleanMaterial;
        attackTargetGameObject.SetActive(false);
    }

    public void SetAsAttackTargetTile() {
        attackTargetGameObject.SetActive(true);
        gridSprite.material = targetTileMaterial;
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
