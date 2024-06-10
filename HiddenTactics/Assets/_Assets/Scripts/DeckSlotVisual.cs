using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSlotVisual : MonoBehaviour
{
    [SerializeField] private TroopSO troopSO;
    [SerializeField] private Transform deckSlotVisualUnitPrefab;

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

    private void Start() {
        SetTroopSO(troopSO);
    }

    public void SetTroopSO(TroopSO troopSO) {
        this.troopSO = troopSO;

        unitSpawnTransforms = troopSO.troopPrefab.GetComponent<Troop>().GetBaseUnitPositions();

        if(unitSpawnTransforms.Count == 1) {
            SetSpawnPositionsFor1UnitTroop();
        }

        if(unitSpawnTransforms.Count == 2) {
            SetSpawnPositionsFor2UnitTroop();
        }

        if (unitSpawnTransforms.Count == 3) {
            SetSpawnPositionsFor3UnitTroop();
        }

        if (unitSpawnTransforms.Count == 4) {
            SetSpawnPositionsFor4UnitTroop();
        }

        if (unitSpawnTransforms.Count == 5) {
            SetSpawnPositionsFor5UnitTroop();
        }

        if (unitSpawnTransforms.Count == 6) {
            SetSpawnPositionsFor6UnitTroop();
        }

        if (unitSpawnTransforms.Count == 7) {
            SetSpawnPositionsFor7UnitTroop();
        }

        if (unitSpawnTransforms.Count == 8) {
            SetSpawnPositionsFor8UnitTroop();
        }

        if (unitSpawnTransforms.Count == 9) {
            SetSpawnPositionsFor9UnitTroop();
        }

        if (unitSpawnTransforms.Count == 10) {
            SetSpawnPositionsFor10UnitTroop();
        }

        RuntimeAnimatorController unitAnimator = troopSO.unitPrefab.transform.GetComponentInChildren<Animator>().runtimeAnimatorController;

        foreach (DeckSlotVisualSpawnPosition spawnPosition in unitSlotTransformList) {
            DeckSlotVisual_Unit deckSlotVisualUnitInstantiated = Instantiate(deckSlotVisualUnitPrefab, spawnPosition.transform.position, Quaternion.identity, transform).GetComponent<DeckSlotVisual_Unit>();
            deckSlotVisualUnitInstantiated.SetUnitAnimator(unitAnimator);

            if (troopSO.unitPrefab.GetComponent<UCW>()) {
                //Unit carries a weapon : get weapon sprite
                Sprite unitWeaponSprite = troopSO.unitPrefab.GetComponent<Unit>().GetUnitSO().mainWeaponSO.idleSprite;
                deckSlotVisualUnitInstantiated.SetWeaponSprite(unitWeaponSprite);
            }

            SetUnitAnimatorInFunctionOfSpawnPosition(deckSlotVisualUnitInstantiated, spawnPosition.GetSpawnPointNumber());
        }
    }

    public void SetUnitAnimatorInFunctionOfSpawnPosition(DeckSlotVisual_Unit deckSlotVisualUnit, int spawnPosition) {
        if (spawnPosition < 5) {
            deckSlotVisualUnit.SetUnitAnimatorXY(-1, -1);
        }
        else {
            deckSlotVisualUnit.SetUnitAnimatorXY(1, -1);
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
            unitSpawnPosition2,
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


}
