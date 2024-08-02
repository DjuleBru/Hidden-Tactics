using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemTemplateUI_BattleDeck : ItemTemplateUI, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] protected Button unlockTroopButton;
    [SerializeField] protected Button recruitTroopButton;
    [SerializeField] protected SpawnIPlaceableButton spawnIPlaceableButton;

    [SerializeField] private bool isBuilding;
    [SerializeField] private bool isSpell;
    [SerializeField] private bool isTroop;
    [SerializeField] private bool isMercenary;

    private Color unHoveredColor;

    private void Awake() {
        unlockTroopButton.onClick.AddListener(() => {
            BattleDeckUI.Instance.OpenUnlockNewItemPanel(isBuilding, isTroop, isSpell);
        });

        // Deactivate unlock button if there are troops set
        if (troopSO != null || buildingSO != null) {
            unlockTroopButton.gameObject.SetActive(false);
        } else {
            recruitTroopButton.gameObject.SetActive(false);
        }

        unHoveredColor = backgroundImage.color;
    }

    public override void SetTroopSO(TroopSO troopSO) {
        this.troopSO = troopSO;

        if(troopSO == null) {
            illustrationImage.gameObject.SetActive(false);
            unlockTroopButton.gameObject.SetActive(true);
            recruitTroopButton.gameObject.SetActive(false);

        } else {
            spawnIPlaceableButton.SetTroopToSpawn(troopSO);
            illustrationImage.gameObject.SetActive(true);
            unlockTroopButton.gameObject.SetActive(false);
            recruitTroopButton.gameObject.SetActive(true);

            if (troopSO.troopIllustrationSlotSprite != null && troopSO.troopIsImplemented) {
                illustrationImage.sprite = troopSO.troopIllustrationSlotSprite;
            }
            else {
                illustrationImage.gameObject.SetActive(false);
                comingSoonText.SetActive(true);
            }

            if (troopSO.troopIllustrationSlotSprite == null && troopSO.troopIsImplemented) {
                illustrationImage.sprite = null;
            }
        }
    }

    public override void SetBuildingSO(BuildingSO buildingSO) {
        this.buildingSO = buildingSO;

        if(buildingSO == null) {
            illustrationImage.gameObject.SetActive(false);
            unlockTroopButton.gameObject.SetActive(true);

        } else {
            spawnIPlaceableButton.SetBuildingToSpawn(buildingSO);
            illustrationImage.gameObject.SetActive(true);
            unlockTroopButton.gameObject.SetActive(false);

            if (buildingSO.buildingIllustrationSlotSprite != null && buildingSO.buildingIsImplemented) {
                illustrationImage.sprite = buildingSO.buildingIllustrationSlotSprite;
            }
            else {
                illustrationImage.gameObject.SetActive(false);
                comingSoonText.SetActive(true);
            }

            if (buildingSO.buildingIllustrationSlotSprite == null && buildingSO.buildingIsImplemented) {
                illustrationImage.sprite = null;
            }
        }

    }

    public TroopSO GetTroopSO() {
        return troopSO;
    }

    public BuildingSO GetBuildingSO() {
        return buildingSO;
    }

    public Image GetSlotImage() {
        return illustrationImage;
    }

    public void OnPointerExit(PointerEventData eventData) {
        backgroundImage.color = unHoveredColor;
        outlineImage.color = unHoveredColor;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        backgroundImage.color = Color.white;
        outlineImage.color = Color.white;
    }
}
