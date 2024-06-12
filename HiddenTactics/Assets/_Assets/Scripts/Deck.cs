using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    public FactionSO deckFactionSO;
    public int deckNumber;

    public string deckName;

    public TroopSO[] troopsInDeck;
    public BuildingSO[] buildingsInDeck;

    public Deck(FactionSO deckFaction, int deckNumber) {
        this.deckFactionSO = deckFaction;
        this.deckNumber = deckNumber;

        this.deckName = "My " + deckFaction.name + " Deck " + deckNumber;

        troopsInDeck = new TroopSO[7];
        buildingsInDeck = new BuildingSO[7];
    }
}
