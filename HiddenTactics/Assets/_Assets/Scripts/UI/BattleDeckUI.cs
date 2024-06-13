using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleDeckUI : MonoBehaviour
{
    public static BattleDeckUI Instance;

    private FactionSO factionSO;
    private Deck playerDeck;

    [SerializeField] private List<ItemTemplateUI_BattleDeck> buildingsItemTemplateVisualUIList;
    [SerializeField] private List<ItemTemplateUI_BattleDeck> troopsItemTemplateVisualUIList;
    [SerializeField] private List<ItemTemplateUI_BattleDeck> spellsItemTemplateVisualUIList;

    private List<TroopSO> troopsUnlockedDuringBattleList = new List<TroopSO>();
    private List<BuildingSO> buildingsUnlockedDuringBattleList = new List<BuildingSO>();

    [SerializeField] private GameObject unlockNewItemPanel;
    [SerializeField] private Transform unlockNewItemContainer;
    [SerializeField] private Transform unlockNewItemTemplate;
    [SerializeField] private Button unlockNewItemPanelCloseButton;

    [SerializeField] private TextMeshProUGUI unlockNewItemCostText;
    [SerializeField] private TextMeshProUGUI unlockNewItemText;

    [SerializeField] private List<int> unlockNextItemCostList;
    private int unlockNextItemCost;

    private void Awake() {
        Instance = this;
        unlockNewItemPanelCloseButton.onClick.AddListener(() => {
            unlockNewItemPanel.gameObject.SetActive(false);
        });
    }

    private void Start() {
        playerDeck = DeckManager.LocalInstance.GetDeckSelected();

        int i = 0;

        foreach(TroopSO troopSO in playerDeck.troopsInDeck) {
            if (troopSO == null) {
                continue;
            }
            troopsItemTemplateVisualUIList[i].SetTroopSO(troopSO);
            troopsItemTemplateVisualUIList[i].GetComponentInChildren<SpawnIPlaceableButton>().SetTroopToSpawn(troopSO);
            i++;
        }

        i = 0;
        
        foreach (BuildingSO buildingSO in playerDeck.buildingsInDeck) {
            if (buildingSO == null) {
                continue;
            }
            buildingsItemTemplateVisualUIList[i].SetBuildingSO(buildingSO);
            buildingsItemTemplateVisualUIList[i].GetComponentInChildren<SpawnIPlaceableButton>().SetBuildingToSpawn(buildingSO);
            i++;
        }
    }

    public void AddNewBuilding(BuildingSO buildingSO) {
        foreach(ItemTemplateUI_BattleDeck itemTemplateUI in buildingsItemTemplateVisualUIList) {
            if (itemTemplateUI.GetBuildingSO() != null) continue;

            itemTemplateUI.SetBuildingSO(buildingSO);
            buildingsUnlockedDuringBattleList.Add(buildingSO);
            return;
        }
    }

    public void AddNewTroop(TroopSO troopSO) {
        foreach (ItemTemplateUI_BattleDeck itemTemplateUI in troopsItemTemplateVisualUIList) {
            if (itemTemplateUI.GetTroopSO() != null) continue;

            itemTemplateUI.SetTroopSO(troopSO);
            troopsUnlockedDuringBattleList.Add(troopSO);
            return;
        }
    }

    public void OpenUnlockNewItemPanel(bool building, bool troop, bool spell) {
        unlockNewItemPanel.SetActive(true);
        unlockNewItemCostText.text = GetNextUnlockCost().ToString();

        if (building) {
            unlockNewItemText.text = "Next building cost :";
        }

        if (troop) {
            unlockNewItemText.text = "Next unit cost :";
        }

        if (spell) {
            unlockNewItemText.text = "Next spell cost :";
        }



        RefreshUnlockNewItemPanel(building, troop, spell);
    }

    private void RefreshUnlockNewItemPanel(bool building, bool troop, bool spell) {
        Deck deck = DeckManager.LocalInstance.GetDeckSelected();
        unlockNewItemTemplate.gameObject.SetActive(true);

        foreach (Transform child in unlockNewItemContainer) {
            if (child == unlockNewItemTemplate) continue;
            Destroy(child.gameObject);
        }

        if(building) {
            List<BuildingSO> buildingsNotInDeck = new List<BuildingSO>();
            List<BuildingSO> buildingsInFaction = deck.deckFactionSO.buildingsInFaction;

            // Find buildings in deck
            foreach(BuildingSO buildingSO in buildingsInFaction) {
                if (!buildingSO.buildingIsImplemented) continue;
                if (Array.IndexOf(deck.buildingsInDeck, buildingSO) == -1) {
                    // Array does not contain building
                    buildingsNotInDeck.Add(buildingSO);
                }
            }

            //Find buildigs unlocked during battle
            foreach(BuildingSO buildingSO1 in buildingsUnlockedDuringBattleList) {
                if(buildingsNotInDeck.Contains(buildingSO1)) {
                    buildingsNotInDeck.Remove(buildingSO1);
                }
            }

            foreach(BuildingSO buildingSO in buildingsNotInDeck) {
                ItemTemplateUI_UnlockItem unlockItemTemplate = Instantiate(unlockNewItemTemplate.transform, unlockNewItemContainer).GetComponent<ItemTemplateUI_UnlockItem>();
                unlockItemTemplate.SetBuildingSO(buildingSO);
            }

        }

        if (troop) {
            List<TroopSO> troopsNotInDeck = new List<TroopSO>();
            List<TroopSO> troopssInFaction = deck.deckFactionSO.troopsInFaction;

            foreach (TroopSO troopSO in troopssInFaction) {
                if (!troopSO.troopIsImplemented) continue;
                if (Array.IndexOf(deck.troopsInDeck, troopSO) == -1) {
                    // Array does not contain building
                    troopsNotInDeck.Add(troopSO);
                }
            }

            //Find buildigs unlocked during battle
            foreach (TroopSO troopSO1 in troopsUnlockedDuringBattleList) {
                if (troopsNotInDeck.Contains(troopSO1)) {
                    troopsNotInDeck.Remove(troopSO1);
                }
            }

            foreach (TroopSO troopSO in troopsNotInDeck) {
                ItemTemplateUI_UnlockItem unlockItemTemplate = Instantiate(unlockNewItemTemplate.transform, unlockNewItemContainer).GetComponent<ItemTemplateUI_UnlockItem>();
                unlockItemTemplate.SetTroopSO(troopSO);
            }

        }

        unlockNewItemTemplate.gameObject.SetActive(false);
    }

    public void CloseUnlockPanel() {
        unlockNewItemPanel.gameObject.SetActive(false);
    }

    public int GetNextUnlockCost() {
        int numberOfUnlocks = troopsUnlockedDuringBattleList.Count + buildingsUnlockedDuringBattleList.Count;
        return unlockNextItemCostList[numberOfUnlocks];
    }
}
