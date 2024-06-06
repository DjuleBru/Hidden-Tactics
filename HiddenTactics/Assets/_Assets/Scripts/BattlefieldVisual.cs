using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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
        DeckManager.LocalInstance.OnSelectedDeckChanged += DeckManager_OnSelectedDeckChanged;
        RefreshPlayerBattlefieldSprites();
    }

    private void RefreshPlayerBattlefieldSprites() {
        // Load sprites from local machine
        Deck selectedDeck = DeckManager.LocalInstance.GetDeckSelected();

        GridTileVisualSO playerGridTileVisualSO = SavingManager.Instance.LoadGridTileVisualSO(selectedDeck);

        Sprite loadedBattlefieldOutlineSprite = playerGridTileVisualSO.battlefieldOutlineSprite;

        Sprite loadeBattlefieldBaseSprite = SavingManager.Instance.LoadBattlefieldBaseSprite(selectedDeck);

        // Save sprites to local machine
        SetBattlefieldOutlineSprite(loadedBattlefieldOutlineSprite);
        SetBattlefieldBaseSprite(loadeBattlefieldBaseSprite);

        // Save sprites to player data
        SaveBattlefieldBaseSpriteInPlayerData(loadeBattlefieldBaseSprite);
    }

    private void DeckManager_OnSelectedDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshPlayerBattlefieldSprites();
    }

    public void SetBattlefieldOutlineSprite(Sprite battlefieldOutlineSprite) {
        battlefieldOutlineSpriteRenderer.sprite = battlefieldOutlineSprite;
    }

    public void SetBattlefieldBaseSprite(Sprite battlefieldBaseSprite) {
        battlefieldBaseSpriteRenderer.sprite = battlefieldBaseSprite;

        SavingManager.Instance.SaveBattlefieldBaseSprite(battlefieldBaseSprite);

        OnBattlefieldBaseChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SaveBattlefieldBaseSpriteInPlayerData(Sprite battlefieldBaseSprite) {
        HiddenTacticsMultiplayer.Instance.SetPlayerBattlefieldBaseSprite(PlayerCustomizationData.Instance.GetBattlefieldBaseSpriteID(battlefieldBaseSprite));
    }
}
