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
    [SerializeField] Material healTargetTileMaterial;
    [SerializeField] Material attackSpeedBuffTileMaterial;
    [SerializeField] Material attackDamageBuffTileMaterial;
    [SerializeField] Material moveSpeedBuffTileMaterial;
    [SerializeField] Material destroyedVillageMaterial;

    [SerializeField] List<SpriteRenderer> villageSpriteRenderers;

    private bool gridObjectVisualReset;

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
            troopString = gridObject.GetTroop().ToString() + "\n";
            troopDebugText.text = troopString;
        } else {
            troopDebugText.text = "";
        }

        if (gridObject.GetBuilding() != null) {
            buildingDebugText.text = gridObject.GetBuilding().ToString() + "\n";
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
        gridObjectVisualReset = false;
    }

    public void SetAsValidPosition() {
        overlayGridSprite.gameObject.SetActive(true);
        overlayGridSprite.material = healTargetTileMaterial;
        gridObjectVisualReset = false;
    }

    public void SetAsUnvalidPosition() {
        overlayGridSprite.gameObject.SetActive(true);
        overlayGridSprite.material = targetTileMaterial;
        gridObjectVisualReset = false;
    }

    public void ResetVisual() {
        gridSprite.material = cleanMaterial;
        overlayGridSprite.material = cleanMaterial;

        rangedAttackTargetGameObject.SetActive(false);
        overlayGridSprite.gameObject.SetActive(false);
        gridObjectVisualReset = true;
    }

    public void SetAsAttackTargetTile() {
        overlayGridSprite.sprite = overlayGradientSprite;
        overlayGridSprite.material = targetTileMaterial;
        overlayGridSprite.gameObject.SetActive(true);
        rangedAttackTargetGameObject.SetActive(true);
        gridObjectVisualReset = false;
    }
    public void SetAsHealTargetTile() {
        overlayGridSprite.sprite = overlayGradientSprite;
        overlayGridSprite.material = healTargetTileMaterial;
        overlayGridSprite.gameObject.SetActive(true);
        rangedAttackTargetGameObject.SetActive(false);
        gridObjectVisualReset = false;
    }
    public void SetAsAttackSpeedBuffTile() {
        overlayGridSprite.sprite = overlayGradientSprite;
        overlayGridSprite.material = attackSpeedBuffTileMaterial;
        overlayGridSprite.gameObject.SetActive(true);
        rangedAttackTargetGameObject.SetActive(false);
        gridObjectVisualReset = false;
    }

    public void SetAsMoveSpeedBuffTile() {
        overlayGridSprite.sprite = overlayGradientSprite;
        overlayGridSprite.material = moveSpeedBuffTileMaterial;
        overlayGridSprite.gameObject.SetActive(true);
        rangedAttackTargetGameObject.SetActive(false);
        gridObjectVisualReset = false;
    }

    public void SetAsAttackDamageBuffTile() {
        overlayGridSprite.sprite = overlayGradientSprite;
        overlayGridSprite.material = attackDamageBuffTileMaterial;
        overlayGridSprite.gameObject.SetActive(true);
        rangedAttackTargetGameObject.SetActive(false);
        gridObjectVisualReset = false;
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

    public bool GetGridObjectVisualIsReset() {
        return gridObjectVisualReset;
    }

}
