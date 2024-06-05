using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{

    public static DeckManager LocalInstance;
    private Deck deckSelected;

    [SerializeField] private FactionSO defaultDeckFactionSO;
    private int deckNumber;

    public event EventHandler<OnDeckChangedEventArgs> OnDeckChanged;
    [SerializeField] private List<FactionSO> allFactionsList;

    public class OnDeckChangedEventArgs:EventArgs {
        public Deck selectedDeck;
    }

    private void Awake() {
        LocalInstance = this;
    }

    private void Start() {
        // Load previous deck (or default deck) on awake
        Deck defaultDeckOnStartup = new Deck(defaultDeckFactionSO, 1);
        deckSelected = ES3.Load("DeckSelected", defaultDeckOnStartup);

        deckNumber = deckSelected.deckNumber;

        OnDeckChanged?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });
    }

    public void SetDeckSelected(int deckNumber) {
        this.deckNumber = deckNumber;
        string deckId = "deck_" + deckSelected.deckFactionSO.ToString() + "_" + deckNumber.ToString();
        Debug.Log(deckId);
        // Create empty deck if there is no deck yet
        Deck defaultEmpyFactionDeck = new Deck(deckSelected.deckFactionSO, deckNumber);
        deckSelected = ES3.Load(deckId, defaultEmpyFactionDeck);

        OnDeckChanged?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });
    }

    public void SetDeckSelected(FactionSO factionSO) {
        string deckId = "deck_" + factionSO.ToString() + "_" + deckNumber.ToString();

        // Create empty deck if there is no deck yet
        Deck defaultEmpyFactionDeck = new Deck(factionSO, deckNumber);
        deckSelected = ES3.Load(deckId, defaultEmpyFactionDeck);

        OnDeckChanged?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });
    }

    #region ADDING AND REMOVING ITEMS

    public void AddTroopToDeckSelected(TroopSO troopSO) {
        deckSelected.troopsInDeck.Add(troopSO);

        OnDeckChanged?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    public void RemoveTroopFromDeckSelected(TroopSO troopSO) {
        deckSelected.troopsInDeck.Remove(troopSO);

        OnDeckChanged?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    public void AddBuildingToDeckSelected(BuildingSO buildingSO) {
        deckSelected.buildingsInDeck.Add(buildingSO);

        OnDeckChanged?.Invoke(this, new OnDeckChangedEventArgs {
            selectedDeck = deckSelected,
        });

        SaveDeckSelected();
    }

    public void RemoveBuildingFromDeckSelected(BuildingSO buildingSO) {
        deckSelected.buildingsInDeck.Remove(buildingSO);

        OnDeckChanged?.Invoke(this, new OnDeckChangedEventArgs {
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
        string deckId = "deck_" + deckSelected.deckFactionSO.ToString() + "_" + deckNumber.ToString();
        ES3.Save(deckId, deckSelected);
        ES3.Save("DeckSelected", deckSelected);
    }

    public Deck GetDeckSelected() {
        return deckSelected;
    }

    public List<FactionSO> GetFactionSOList() {
        return allFactionsList;
    }

}
