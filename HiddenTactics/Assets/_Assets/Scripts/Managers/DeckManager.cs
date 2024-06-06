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

    public void AddTroopToDeckSelected(TroopSO troopSO) {
        deckSelected.troopsInDeck.Add(troopSO);

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    public void RemoveTroopFromDeckSelected(TroopSO troopSO) {
        deckSelected.troopsInDeck.Remove(troopSO);

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    public void AddBuildingToDeckSelected(BuildingSO buildingSO) {
        deckSelected.buildingsInDeck.Add(buildingSO);

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    public void RemoveBuildingFromDeckSelected(BuildingSO buildingSO) {
        deckSelected.buildingsInDeck.Remove(buildingSO);

        OnDeckModified?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    #endregion

    public void SetDeckName(string name) {
        deckSelected.deckName = name;
        SaveDeckSelected();
    }

    private void SaveDeckSelected() {
        Debug.Log("savec deck " + deckSelected.deckName);
        SavingManager.Instance.SaveDeck(deckSelected, deckNumber);
    }

    public Deck GetDeckSelected() {
        return deckSelected;
    }

    public List<FactionSO> GetFactionSOList() {
        return allFactionsList;
    }

}
