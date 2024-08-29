using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSlot : MonoBehaviour
{
    [SerializeField] private int deckSlotNumber;
    private Deck deckSelected;

    private TroopSO troopSO;
    private BuildingSO buildingSO;
    private DeckSlotUI deckSlotUI;
    private DeckSlotVisual deckSlotVisual;
    private DeckSlotAnimatorManager deckSlotAnimatorManager;

    [SerializeField] private bool canHostTroop;
    [SerializeField] private bool canHostBuilding;
    [SerializeField] private bool canHostHero;
    [SerializeField] private bool canHostSpell;

    [SerializeField] private Transform deckSlotVisualTransform;

    private List<DeckSlotVisualSpawnPosition> unitSlotTransformList;

    [SerializeField] private GameObject smallUnitSlotContainer;
    [SerializeField] private GameObject mediumUnitSlotContainer;
    [SerializeField] private GameObject largeUnitSlotContainer;

    private List<DeckSlotVisualSpawnPosition> smallUnitSlotTransformList = new List<DeckSlotVisualSpawnPosition>();
    private List<DeckSlotVisualSpawnPosition> mediumUnitSlotTransformList = new List<DeckSlotVisualSpawnPosition>();
    private List<DeckSlotVisualSpawnPosition> largeUnitSlotTransformList = new List<DeckSlotVisualSpawnPosition>();

    private List<Unit> unitVisualsSpawnedList = new List<Unit>();
    private Building buildingSpawned;

    public static event EventHandler OnAnyDeckSlotSelected;
    public static event EventHandler OnAnyDeckSlotUnSelected;
    private bool selecting;

    private void Awake()
    {
        deckSlotUI = GetComponentInChildren<DeckSlotUI>();
        deckSlotVisual = GetComponentInChildren<DeckSlotVisual>();
        deckSlotAnimatorManager = GetComponent<DeckSlotAnimatorManager>();

        foreach(DeckSlotVisualSpawnPosition spawnPosition in smallUnitSlotContainer.GetComponentsInChildren<DeckSlotVisualSpawnPosition>()) {
            smallUnitSlotTransformList.Add(spawnPosition);
        }
        foreach (DeckSlotVisualSpawnPosition spawnPosition in mediumUnitSlotContainer.GetComponentsInChildren<DeckSlotVisualSpawnPosition>()) {
            mediumUnitSlotTransformList.Add(spawnPosition);
        }
        foreach (DeckSlotVisualSpawnPosition spawnPosition in largeUnitSlotContainer.GetComponentsInChildren<DeckSlotVisualSpawnPosition>()) {
            largeUnitSlotTransformList.Add(spawnPosition);
        }
    }

    private void Start()
    {
        DeckSlot.OnAnyDeckSlotSelected += DeckSlot_OnAnyDeckSlotSelected;
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;
        DeckManager.LocalInstance.OnSelectedDeckChanged += DeckManager_OnSelectedDeckChanged;

        RefreshDeckSlotTroopSO(true);
        RefreshDeckSlotBuildingSO();
    }

    #region TROOPS
    private void RefreshDeckSlotTroopSO(bool refreshOnStart)
    {
        if (DeckManager.LocalInstance.GetDeckSelected().troopsInDeck[deckSlotNumber] == null || DeckManager.LocalInstance.GetDeckSelected().troopsInDeck[deckSlotNumber] != this.troopSO)
        {
            // No more troop on this slot OR troop changed
            foreach (Unit unit in unitVisualsSpawnedList)
            {
                Destroy(unit.gameObject);
            }

            unitVisualsSpawnedList.Clear();
            this.troopSO = null;
        };

        TroopSO troopSO = DeckManager.LocalInstance.GetDeckSelected().troopsInDeck[deckSlotNumber];

        if(troopSO != null && troopSO != this.troopSO)
        {
            SetTroopSO(troopSO, refreshOnStart);
        }
    }

    public void SetTroopSO(TroopSO troopSO, bool refreshOnStart)
    {
        this.troopSO = troopSO;

        SetSpawnPositions(troopSO.unitSizeInTroop);

        RuntimeAnimatorController unitAnimator = troopSO.unitPrefab.transform.GetComponentInChildren<Animator>().runtimeAnimatorController;

        SpawnUnitVisuals(refreshOnStart);
        deckSlotUI.DisableAddTroopText();

    }

    private void SpawnUnitVisuals(bool refreshOnStart)
    {
        int unit0Number = troopSO.troopPrefab.GetComponent<Troop>().GetBaseUnitPositions().Count + troopSO.troopPrefab.GetComponent<Troop>().GetAdditionalUnitPositions().Count;
        int unit1Number = troopSO.troopPrefab.GetComponent<Troop>().GetBaseUnit1Positions().Count + troopSO.troopPrefab.GetComponent<Troop>().GetAdditionalUnit1Positions().Count;
        int unit2Number = troopSO.troopPrefab.GetComponent<Troop>().GetBaseUnit2Positions().Count + troopSO.troopPrefab.GetComponent<Troop>().GetAdditionalUnit2Positions().Count;

        int unitSpawned = 0;

        foreach (DeckSlotVisualSpawnPosition spawnPosition in unitSlotTransformList)
        {
            GameObject unitPrefab = troopSO.unitPrefab;

            if(unitSpawned >= unit0Number && troopSO.additionalUnit1Prefab != null) {
                unitPrefab = troopSO.additionalUnit1Prefab;
            }
            if (unitSpawned >= (unit0Number + unit1Number) && troopSO.additionalUnit2Prefab != null) {
                unitPrefab = troopSO.additionalUnit2Prefab;
            }


            Unit deckSlotVisualUnitInstantiated = Instantiate(unitPrefab, spawnPosition.transform.position, Quaternion.identity, deckSlotVisualTransform).GetComponent<Unit>();
            deckSlotVisualUnitInstantiated.SetUnitAsVisual();
            deckSlotVisualUnitInstantiated.GetComponentInChildren<UnitVisual>().ChangeAllSpriteRendererListSortingOrder(deckSlotVisual.GetSlotVisualSpriteRenderer().sortingLayerID, deckSlotNumber * 100 + 2);
            deckSlotVisualUnitInstantiated.GetComponentInChildren<UnitVisual>().DeActivateStylizedShadows();

            UnitAnimatorManager unitAnimatorManager = deckSlotVisualUnitInstantiated.GetComponentInChildren<UnitAnimatorManager>();

            if(!refreshOnStart) {
                unitAnimatorManager.TriggerUnitOnlyVisualPlaced();
            }

            SetUnitAnimatorInFunctionOfSpawnPosition(unitAnimatorManager, spawnPosition.GetSpawnPointNumber());

            unitVisualsSpawnedList.Add(deckSlotVisualUnitInstantiated);

            unitSpawned++;
        }
    }

    #endregion

    #region BUILDINGS
    private void RefreshDeckSlotBuildingSO()
    {
        if (DeckManager.LocalInstance.GetDeckSelected().buildingsInDeck[deckSlotNumber] == null || DeckManager.LocalInstance.GetDeckSelected().buildingsInDeck[deckSlotNumber] != this.buildingSO)
        {
            if(buildingSpawned != null)
            {
                Destroy(buildingSpawned.gameObject);
                buildingSpawned = null;
                this.buildingSO = null;
            }
        };

        BuildingSO buildingSO = DeckManager.LocalInstance.GetDeckSelected().buildingsInDeck[deckSlotNumber];
        if (buildingSO != null && buildingSO != this.buildingSO)
        {
            SetBuildingSO(buildingSO);
        }
    }

    public void SetBuildingSO(BuildingSO buildingSO)
    {
        this.buildingSO = buildingSO;

        SpawnBuildingVisuals();
        deckSlotUI.DisableAddTroopText();
    }

    private void SpawnBuildingVisuals()
    {
        Building buildingInstantiated = Instantiate(buildingSO.buildingPrefab, transform.position, Quaternion.identity, deckSlotVisualTransform).GetComponent<Building>();
        buildingInstantiated.transform.position -= buildingInstantiated.GetCenterPoint().localPosition;
        buildingInstantiated.SetBuildingAsVisual();
        buildingSpawned = buildingInstantiated;
    }
    #endregion

    public void RemoveSlotContent()
    {
        if(troopSO != null)
        {
            DeckManager.LocalInstance.RemoveTroopFromDeckSelected(troopSO, deckSlotNumber);
            troopSO = null;
        }
        if(buildingSO != null)
        {
            DeckManager.LocalInstance.RemoveBuildingFromDeckSelected(buildingSO, deckSlotNumber);
            buildingSO = null;
        }

        deckSlotUI.RefreshAddRemoveButtons();
    }

    public void SetUnitAnimatorInFunctionOfSpawnPosition(DeckSlotVisual_Unit deckSlotVisualUnit, int spawnPosition) {
        if (spawnPosition < 5) {
            deckSlotVisualUnit.SetUnitAnimatorXY(-1, -1);
        }
        else {
            deckSlotVisualUnit.SetUnitAnimatorXY(1, -1);
        }
    }

    public void SetUnitAnimatorInFunctionOfSpawnPosition(UnitAnimatorManager deckSlotVisualUnit, int spawnPosition)
    {
        if (spawnPosition < 5)
        {
            deckSlotVisualUnit.SetXY(-1, -1);
        }
        else
        {
            deckSlotVisualUnit.SetXY(1, -1);
        }
    }

    public void SetUIActive(bool active)
    {
        if (active) {
            deckSlotVisual.EnableSlotTypeUI(true);
            if (troopSO == null && buildingSO == null)
            {
                deckSlotUI.EnableAddTroopText();
            }
        } else {
            deckSlotVisual.EnableSlotTypeUI(false);
            deckSlotUI.DisableAddTroopText();
        }
    }

    public DeckSlotAnimatorManager GetDeckSlotAnimatorManager() {
        return deckSlotAnimatorManager;
    }

    public void SetAnimatorActive(bool active) {
        deckSlotAnimatorManager.SetAnimatorActive(active);
    }

    public void TriggerFlyUp() {
        deckSlotAnimatorManager.TriggerFlyUp();
    }

    public void TriggerFlyDown() {
        deckSlotAnimatorManager.TriggerFlyDown();
    }

    public void TriggerFlyDownUp() {
        deckSlotAnimatorManager.TriggerFlyDownUp();
    }

    public void TriggerFlyDownDown() {
        deckSlotAnimatorManager.TriggerFlyDownDown();
    }
    public void SetAnimatorActiveAndTriggerDown() {
        deckSlotAnimatorManager.SetAnimatorActive(true);
        deckSlotAnimatorManager.SetDeckSlotAnimationUnhoveredAbsolute();
    }

    public void SetSelecting(bool selecting)
    {
        this.selecting = selecting;

        deckSlotVisual.SetSelectingTroop(selecting);
        deckSlotAnimatorManager.SetSelectingTroop(selecting);
        deckSlotUI.SetSelectingTroop(selecting);
        deckSlotUI.SetTroopSelectingVisualUI(selecting);

        if (!selecting)
        {
            deckSlotVisual.SetDeckSlotUnhovered();
            deckSlotAnimatorManager.SetDeckSlotAnimationUnhovered();
            deckSlotUI.SetTroopSelectingVisualUI(false);
            OnAnyDeckSlotUnSelected?.Invoke(this, EventArgs.Empty);
        } else
        {
            OnAnyDeckSlotSelected?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool GetSelecting()
    {
        return selecting;
    }
    
    public bool GetCanHostTroop() {
        return canHostTroop;
    }

    public bool GetCanHostBuilding() {
        return canHostBuilding;
    }

    public bool GetCanHostHero() {
        return canHostHero;
    }

    public bool GetCanHostSpell() {
        return canHostSpell;
    }

    public int GetDeckSlotNumber()
    {
        return deckSlotNumber;
    }

    public TroopSO GetSlotTroopSO()
    {
        return troopSO;
    }

    public BuildingSO GetSlotBuildingSO()
    {
        return buildingSO;
    }

    private void DeckManager_OnSelectedDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e)
    {
        SetSelecting(false);
        deckSlotUI.RefreshAddRemoveButtons();
    }

    private void DeckManager_OnDeckModified(object sender, DeckManager.OnDeckChangedEventArgs e)
    {
        deckSelected = e.selectedDeck;
        RefreshDeckSlotTroopSO(false);
        RefreshDeckSlotBuildingSO();
        deckSlotUI.RefreshAddRemoveButtons();
    }

    private void DeckSlot_OnAnyDeckSlotSelected(object sender, EventArgs e)
    {
        if (sender as DeckSlot == this) return;
        SetSelecting(false);
    }

    public DeckSlotVisual GetDeckSlotVisual() {
        return deckSlotVisual;
    }

    #region SET UNIT SPAWN POSITIONS
    private void SetSpawnPositions(TroopSO.UnitSizeInTroop unitSizeInTroop)
    {
        if(unitSizeInTroop == TroopSO.UnitSizeInTroop.small) {
            unitSlotTransformList = smallUnitSlotTransformList;
        }

        if (unitSizeInTroop == TroopSO.UnitSizeInTroop.medium) {
            unitSlotTransformList = mediumUnitSlotTransformList;
        }

        if (unitSizeInTroop == TroopSO.UnitSizeInTroop.big) {
            unitSlotTransformList = largeUnitSlotTransformList;

        }
    }

    #endregion

}
