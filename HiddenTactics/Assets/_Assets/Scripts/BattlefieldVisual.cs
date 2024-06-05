using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlefieldVisual : MonoBehaviour
{
    public static BattlefieldVisual Instance { get; private set; }

    [SerializeField] private SpriteRenderer battlefieldOutlineSpriteRenderer;
    [SerializeField] private SpriteRenderer battlefieldBaseSpriteRenderer;
    [SerializeField] private Sprite battlefieldBaseDefaultSprite;

    public event EventHandler OnBattlefieldBaseChanged;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        DeckManager.LocalInstance.OnDeckChanged += DeckManager_OnDeckChanged;
    }

    private void DeckManager_OnDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        string factionBattlefieldOutlineSpriteKey = e.selectedDeck.deckFactionSO.ToString() + "_battlefieldOutlineSprite";
        Sprite defaultFactionBattlefieldOutlineSprite = e.selectedDeck.deckFactionSO.factionDefaultGridTileVisualSO.battlefieldOutlineSprite;
        Sprite factionBattlefieldOutlineSprite = ES3.Load(factionBattlefieldOutlineSpriteKey, defaultValue: defaultFactionBattlefieldOutlineSprite);

        string battlefieldBaseSpriteKey = e.selectedDeck.deckFactionSO.ToString() + "_battlefieldBaseSprite";
        Sprite battlefieldBaseSprite = ES3.Load(battlefieldBaseSpriteKey, defaultValue: battlefieldBaseDefaultSprite);

        SetBattlefieldOutlineSprite(factionBattlefieldOutlineSprite);
        SetBattlefieldBaseSprite(battlefieldBaseSprite);
    }

    public void SetBattlefieldOutlineSprite(Sprite battlefieldOutlineSprite) {
        string battlefieldOutlineSpriteKey = DeckManager.LocalInstance.GetDeckSelected().deckFactionSO.ToString() + "_battlefieldOutlineSprite";
        ES3.Save(battlefieldOutlineSpriteKey, battlefieldOutlineSprite);

        battlefieldOutlineSpriteRenderer.sprite = battlefieldOutlineSprite;
    }

    public void SetBattlefieldBaseSprite(Sprite battlefieldBaseSprite) {
        string battlefieldBaseSpriteKey = DeckManager.LocalInstance.GetDeckSelected().deckFactionSO.ToString() + "_battlefieldBaseSprite";
        ES3.Save(battlefieldBaseSpriteKey, battlefieldBaseSprite);

        battlefieldBaseSpriteRenderer.sprite = battlefieldBaseSprite;

        OnBattlefieldBaseChanged?.Invoke(this, EventArgs.Empty);
    }
}
