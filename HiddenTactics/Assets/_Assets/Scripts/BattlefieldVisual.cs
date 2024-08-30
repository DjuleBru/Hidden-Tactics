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
    private Animator battlefieldAnimator;

    private void Awake() {
        Instance = this;
        if(BattleManager.Instance == null) {
            battlefieldAnimator = GetComponentInParent<Animator>();
        }
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

        BattlefieldBaseSO loadeBattlefieldBaseSO = SavingManager.Instance.LoadBattlefieldBaseSO(selectedDeck);

        // Save sprites to local machine
        SetBattlefieldOutlineSprite(loadedBattlefieldOutlineSprite);
        SetBattlefieldBaseSO(loadeBattlefieldBaseSO);

        // Save sprites to player data
        SaveBattlefieldBaseSpriteInPlayerData(loadeBattlefieldBaseSO);
    }

    private void DeckManager_OnSelectedDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        RefreshPlayerBattlefieldSprites();
    }

    public void SetBattlefieldOutlineSprite(Sprite battlefieldOutlineSprite) {
        battlefieldOutlineSpriteRenderer.sprite = battlefieldOutlineSprite;
    }

    public void SetBattlefieldBaseSO(BattlefieldBaseSO battlefieldBaseSO) {
        battlefieldBaseSpriteRenderer.sprite = battlefieldBaseSO.battlefieldBaseSprite;

        SavingManager.Instance.SaveBattlefieldBaseSO(battlefieldBaseSO);

        OnBattlefieldBaseChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SaveBattlefieldBaseSpriteInPlayerData(BattlefieldBaseSO battlefieldBaseSO) {
        HiddenTacticsMultiplayer.Instance.SetPlayerBattlefieldBaseSO(PlayerCustomizationDataManager.Instance.GetBattlefieldBaseSOID(battlefieldBaseSO));
    }

    public void MoveBattlefieldToEdit() {
        battlefieldAnimator.SetTrigger("ToEdit");
    }

    public void MoveBattlefieldFromEdit() {
        Debug.Log("from");
        battlefieldAnimator.SetTrigger("FromEdit");
    }
}
