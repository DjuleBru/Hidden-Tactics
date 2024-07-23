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
    [SerializeField] SpriteRenderer overlayGridSprite;
    [SerializeField] Sprite overlayGradientSprite;
    [SerializeField] GameObject rangedAttackTargetGameObject;

    private Material cleanMaterial;
    [SerializeField] Material selectedMaterial;
    [SerializeField] Material targetTileMaterial;
    [SerializeField] Material attackSpeedBuffTileMaterial;
    [SerializeField] Material destroyedVillageMaterial;

    [SerializeField] List<SpriteRenderer> villageSpriteRenderers;

    private void Awake() {
        cleanMaterial = gridSprite.material;

        if(!showDebugInfo) {
            debugCanvas.SetActive(false);
        }

        ResetVisual();
    }

    public void SetGridObject(GridObject gridObject) {
        this.gridObject = gridObject;
    }


    private void Update() {
        if (!showDebugInfo) return;
        gridPositionDebugText.text = gridObject.ToString();

        string troopString = "";
        string unitString = "";

        if (gridObject.GetTroop() != null) {
            troopString += gridObject.GetTroop().ToString() + "\n";
            troopDebugText.text = troopString;
        } else {
            troopDebugText.text = "";
        }

        if (gridObject.GetBuildingList() != null) {
            foreach (Building building in gridObject.GetBuildingList()) {
                buildingDebugText.text += building.ToString() + "\n";
            }
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

    public void SetHovered() {
        gridSprite.material = selectedMaterial;
    }

    public void ResetVisual() {
        gridSprite.material = cleanMaterial;
        overlayGridSprite.material = cleanMaterial;

        rangedAttackTargetGameObject.SetActive(false);
        overlayGridSprite.gameObject.SetActive(false);
    }

    public void SetAsAttackTargetTile() {
        overlayGridSprite.sprite = overlayGradientSprite;
        overlayGridSprite.material = targetTileMaterial;
        overlayGridSprite.gameObject.SetActive(true);
        rangedAttackTargetGameObject.SetActive(true);
    }

    public void SetAsAttackSpeedBuffTile() {
        overlayGridSprite.sprite = overlayGradientSprite;
        overlayGridSprite.material = attackSpeedBuffTileMaterial;
        overlayGridSprite.gameObject.SetActive(true);
        rangedAttackTargetGameObject.SetActive(false);
    }

    public void SetGridSprite(List<Sprite> sprites) {
        Sprite sprite = sprites[Random.Range(0, sprites.Count)];

        gridSprite.sprite = sprite;
    }

    public void SetVillageSprites(List<Sprite> sprites) {
        foreach(SpriteRenderer spriteRenderer in villageSpriteRenderers) {
            spriteRenderer.sprite = sprites[Random.Range(0, sprites.Count)];
        }
    }

    public void SetGridSpriteMaterial(Material material)
    {
        gridSprite.material = material;
    }

    public void SetVillageSpriteMaterial(Material material)
    {
        foreach (SpriteRenderer spriteRenderer in villageSpriteRenderers)
        {
            spriteRenderer.material = material;
        }
    } 

}
