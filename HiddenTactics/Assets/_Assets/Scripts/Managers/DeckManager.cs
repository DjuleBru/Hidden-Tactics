using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{

    public static DeckManager LocalInstance;
    private Deck deckSelected;

    private int deckNumber;

    public event EventHandler<OnDeckChangedEventArgs> OnDeckModified;
    public event EventHandler<OnDeckChangedEventArgs> OnSelectedDeckChanged;

    [SerializeField] private List<FactionSO> allFactionsList;

    public class OnDeckChangedEventArgs:EventArgs {
        public Deck selectedDeck;
    }

    private void Awake() {
        LocalInstance = this;
    }

    private void Start() {
        // Load previous deck (or default deck) on awake
        deckSelected = SavingManager.Instance.LoadDeck();
        Debug.Log("loaded deck " + deckSelected.deckName);

        deckNumber = deckSelected.deckNumber;

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        OnDeckModified += DeckManager_OnDeckModified;
    }

    private void DeckManager_OnDeckModified(object sender, OnDeckChangedEventArgs e) {
        HiddenTacticsMultiplayer.Instance.SetPlayerFactionSO(PlayerCustomizationDataManager.Instance.GetFactionSOID(deckSelected.deckFactionSO));
    }

    public void SetDeckSelected(int deckNumber) {
        this.deckNumber = deckNumber;

        deckSelected = SavingManager.Instance.LoadDeck(deckSelected.deckFactionSO, deckNumber);

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        OnSelectedDeckChanged?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });
    }

    public void SetDeckSelected(FactionSO factionSO) {

        deckSelected = SavingManager.Instance.LoadDeck(factionSO, deckNumber);

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        OnSelectedDeckChanged?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });
    }

    #region ADDING AND REMOVING ITEMS

    public void AddTroopToDeckSelected(TroopSO troopSO, int troopIndex) {
        deckSelected.troopsInDeck[troopIndex] = troopSO;

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    public void RemoveTroopFromDeckSelected(TroopSO troopSO, int troopIndex) {
        deckSelected.troopsInDeck[troopIndex] = null;

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    public void AddBuildingToDeckSelected(BuildingSO buildingSO, int buildingIndex) {
        deckSelected.buildingsInDeck[buildingIndex] = buildingSO;

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    public void RemoveBuildingFromDeckSelected(BuildingSO buildingSO, int buildingIndex) {
        deckSelected.buildingsInDeck[buildingIndex] = null;

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    #endregion

    public int GetTroopSOIndex(TroopSO troopSO) {
        return Array.IndexOf(deckSelected.troopsInDeck, troopSO);
    }
    public int GetBuildingSOIndex(BuildingSO buildingSO) {
        return Array.IndexOf(deckSelected.buildingsInDeck, buildingSO);
    }

    public void SetDeckName(string name) {
        deckSelected.deckName = name;
        SaveDeckSelected();
    }

    private void SaveDeckSelected() {
        SavingManager.Instance.SaveDeck(deckSelected, deckNumber);
    }

    public Deck GetDeckSelected() {
        return deckSelected;
    }

    public int GetEmptyDeckSlots()
    {
        int emptyDeckSlots = 6;

        foreach(TroopSO troopSO in deckSelected.troopsInDeck)
        {
            if(troopSO != null)
            {
                emptyDeckSlots--;
            }
        }

        foreach (BuildingSO buildingSO in deckSelected.buildingsInDeck)
        {
            if (buildingSO != null)
            {
                emptyDeckSlots--;
            }
        }

        return emptyDeckSlots;
    }

    public List<FactionSO> GetFactionSOList() {
        return allFactionsList;
    }

}
