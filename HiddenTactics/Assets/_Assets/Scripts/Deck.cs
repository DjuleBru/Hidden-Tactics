using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    public FactionSO deckFactionSO;
    public int deckNumber;

    public string deckName;

    public List<TroopSO> troopsInDeck;
    public List<BuildingSO> buildingsInDeck;

    public Deck(FactionSO deckFaction, int deckNumber) {
        this.deckFactionSO = deckFaction;
        this.deckNumber = deckNumber;

        this.deckName = "My " + deckFaction.name + " Deck " + deckNumber;

        troopsInDeck = new List<TroopSO>();
        buildingsInDeck = new List<BuildingSO>();
    }
}
