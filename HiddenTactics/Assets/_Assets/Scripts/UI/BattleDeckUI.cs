using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDeckUI : MonoBehaviour
{
    private FactionSO factionSO;
    private Deck playerDeck;

    [SerializeField] private List<ItemTemplateUI> buildingsItemTemplateVisualUIList;
    [SerializeField] private List<ItemTemplateUI> troopsItemTemplateVisualUIList;
    [SerializeField] private List<ItemTemplateUI> spellsItemTemplateVisualUIList;

    private void Start() {
        playerDeck = DeckManager.LocalInstance.GetDeckSelected();

        int i = 0;
        foreach(TroopSO troopSO in playerDeck.troopsInDeck) {
            if (troopSO == null) {
                i++;
                continue;
            }
            troopsItemTemplateVisualUIList[i].SetTroopSO(troopSO);
            troopsItemTemplateVisualUIList[i].GetComponentInChildren<SpawnIPlaceableButton>().SetTroopToSpawn(troopSO);
            i++;
        }

        i = 0;
        foreach (BuildingSO buildingSO in playerDeck.buildingsInDeck) {
            if (buildingSO == null) {
                i++;
                continue;
            }
            buildingsItemTemplateVisualUIList[i].SetBuildingSO(buildingSO);
            buildingsItemTemplateVisualUIList[i].GetComponentInChildren<SpawnIPlaceableButton>().SetBuildingToSpawn(buildingSO);
            i++;
        }

    }
}
