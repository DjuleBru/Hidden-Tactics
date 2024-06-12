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

    [SerializeField] private Transform deckSlotVisualTransform;

    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition0;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition1;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition2;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition3;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition4;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition5;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition6;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition7;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition8;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition9;
    [SerializeField] private DeckSlotVisualSpawnPosition unitSpawnPosition10;

    private List<DeckSlotVisualSpawnPosition> unitSlotTransformList;

    private List<Transform> unitSpawnTransforms;
    private List<Unit> unitVisualsSpawnedList = new List<Unit>();
    private Building buildingSpawned;

    public static event EventHandler OnAnyDeckSlotSelected;
    private bool selecting;

    private void Awake()
    {
        deckSlotUI = GetComponentInChildren<DeckSlotUI>();
        deckSlotVisual = GetComponentInChildren<DeckSlotVisual>();
        deckSlotAnimatorManager = GetComponent<DeckSlotAnimatorManager>();
    }

    private void Start()
    {
        DeckSlot.OnAnyDeckSlotSelected += DeckSlot_OnAnyDeckSlotSelected;
        DeckManager.LocalInstance.OnDeckModified += DeckManager_OnDeckModified;
        DeckManager.LocalInstance.OnSelectedDeckChanged += DeckManager_OnSelectedDeckChanged;

        RefreshDeckSlotTroopSO();
        RefreshDeckSlotBuildingSO();
    }
    #region TROOPS
    private void RefreshDeckSlotTroopSO()
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
            SetTroopSO(troopSO);
        }
    }
    public void SetTroopSO(TroopSO troopSO)
    {
        this.troopSO = troopSO;

        unitSpawnTransforms = troopSO.troopPrefab.GetComponent<Troop>().GetBaseUnitPositions();

        SetSpawnPositions();

        RuntimeAnimatorController unitAnimator = troopSO.unitPrefab.transform.GetComponentInChildren<Animator>().runtimeAnimatorController;

        SpawnUnitVisuals();
        deckSlotUI.DisableAddTroopButton();

        if (DeckSlotMouseHoverManager.Instance.GetEditingDeck())
        {
            deckSlotUI.EnableRemoveTroopButton();
        }
    }

    private void SpawnUnitVisuals()
    {
        foreach (DeckSlotVisualSpawnPosition spawnPosition in unitSlotTransformList)
        {
            Unit deckSlotVisualUnitInstantiated = Instantiate(troopSO.unitPrefab, spawnPosition.transform.position, Quaternion.identity, deckSlotVisualTransform).GetComponent<Unit>();
            deckSlotVisualUnitInstantiated.SetUnitAsVisual();
            deckSlotVisualUnitInstantiated.GetComponentInChildren<UnitVisual>().ChangeAllSpriteRendererListSortingOrder(deckSlotVisual.GetSlotVisualSpriteRenderer().sortingLayerID, deckSlotNumber * 100 + 2);
            deckSlotVisualUnitInstantiated.GetComponentInChildren<UnitVisual>().DeActivateStylizedShadows();

            UnitAnimatorManager unitAnimatorManager = deckSlotVisualUnitInstantiated.GetComponentInChildren<UnitAnimatorManager>();
            SetUnitAnimatorInFunctionOfSpawnPosition(unitAnimatorManager, spawnPosition.GetSpawnPointNumber());

            unitVisualsSpawnedList.Add(deckSlotVisualUnitInstantiated);
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
        deckSlotUI.DisableAddTroopButton();

        if (DeckSlotMouseHoverManager.Instance.GetEditingDeck())
        {
            deckSlotUI.EnableRemoveTroopButton();
        }
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
        if(active)
        {
            if (troopSO == null && buildingSO == null)
            {
                deckSlotUI.EnableAddTroopButton();
            }
            else
            {
                deckSlotUI.EnableRemoveTroopButton();
            }
        } else
        {
            deckSlotUI.DisableAddTroopButton();
            deckSlotUI.DisableRemoveTroopButton();
        }
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
        } else
        {
            OnAnyDeckSlotSelected?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool GetSelecting()
    {
        return selecting;
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
        RefreshDeckSlotTroopSO();
        RefreshDeckSlotBuildingSO();
        deckSlotUI.RefreshAddRemoveButtons();
    }

    private void DeckSlot_OnAnyDeckSlotSelected(object sender, EventArgs e)
    {
        if (sender as DeckSlot == this) return;
        SetSelecting(false);
    }


    #region SET UNIT SPAWN POSITIONS
    private void SetSpawnPositions()
    {
        if (unitSpawnTransforms.Count == 1)
        {
            SetSpawnPositionsFor1UnitTroop();
        }

        if (unitSpawnTransforms.Count == 2)
        {
            SetSpawnPositionsFor2UnitTroop();
        }

        if (unitSpawnTransforms.Count == 3)
        {
            SetSpawnPositionsFor3UnitTroop();
        }

        if (unitSpawnTransforms.Count == 4)
        {
            SetSpawnPositionsFor4UnitTroop();
        }

        if (unitSpawnTransforms.Count == 5)
        {
            SetSpawnPositionsFor5UnitTroop();
        }

        if (unitSpawnTransforms.Count == 6)
        {
            SetSpawnPositionsFor6UnitTroop();
        }

        if (unitSpawnTransforms.Count == 7)
        {
            SetSpawnPositionsFor7UnitTroop();
        }

        if (unitSpawnTransforms.Count == 8)
        {
            SetSpawnPositionsFor8UnitTroop();
        }

        if (unitSpawnTransforms.Count == 9)
        {
            SetSpawnPositionsFor9UnitTroop();
        }

        if (unitSpawnTransforms.Count == 10)
        {
            SetSpawnPositionsFor10UnitTroop();
        }
    }

    private void SetSpawnPositionsFor1UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition0,
        };
    }

    private void SetSpawnPositionsFor2UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition1,
            unitSpawnPosition9,
        };
    }
    private void SetSpawnPositionsFor3UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition6,
            unitSpawnPosition1,
            unitSpawnPosition7,
        };
    }
    private void SetSpawnPositionsFor4UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition4,
            unitSpawnPosition3,
            unitSpawnPosition7,
            unitSpawnPosition10,
        };
    }
    private void SetSpawnPositionsFor5UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition0,
            unitSpawnPosition2,
            unitSpawnPosition7,
            unitSpawnPosition1,
            unitSpawnPosition9,
        };
    }
    private void SetSpawnPositionsFor6UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition5,
            unitSpawnPosition6,
            unitSpawnPosition9,
            unitSpawnPosition7,
            unitSpawnPosition2,
            unitSpawnPosition1,
        };
    }
    private void SetSpawnPositionsFor7UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition0,
            unitSpawnPosition1,
            unitSpawnPosition2,
            unitSpawnPosition7,
            unitSpawnPosition9,
            unitSpawnPosition5,
            unitSpawnPosition6,
        };
    }
    private void SetSpawnPositionsFor8UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition4,
            unitSpawnPosition5,
            unitSpawnPosition6,
            unitSpawnPosition10,
            unitSpawnPosition3,
            unitSpawnPosition2,
            unitSpawnPosition7,
            unitSpawnPosition8,
        };
    }
    private void SetSpawnPositionsFor9UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition0,
            unitSpawnPosition4,
            unitSpawnPosition5,
            unitSpawnPosition6,
            unitSpawnPosition10,
            unitSpawnPosition3,
            unitSpawnPosition2,
            unitSpawnPosition7,
            unitSpawnPosition8,
        };
    }
    private void SetSpawnPositionsFor10UnitTroop() {
        unitSlotTransformList = new List<DeckSlotVisualSpawnPosition> {
            unitSpawnPosition1,
            unitSpawnPosition9,
            unitSpawnPosition4,
            unitSpawnPosition5,
            unitSpawnPosition6,
            unitSpawnPosition10,
            unitSpawnPosition3,
            unitSpawnPosition2,
            unitSpawnPosition7,
            unitSpawnPosition8,
        };
    }

    #endregion

}
