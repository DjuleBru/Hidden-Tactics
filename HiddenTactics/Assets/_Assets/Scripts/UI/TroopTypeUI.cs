using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TroopTypeUI : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Troop troop;
    [SerializeField] private Image troopTypeIcon;
    [SerializeField] private Image troopTypeBackground;
    [SerializeField] private Image troopTypeOutline;
    [SerializeField] private GameObject troopTypeUIGameObject;

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material placingUnitMaterial;
    [SerializeField] private Material troopSelectedMaterial;

    private CanvasGroup canvasGroup;
    private Animator animator;

    private Vector3 hoveringOverUnitPosition;
    private bool tacticalView;
    private bool active_PlayerInput;

    private float worldIconScaleMultiplier;
    [SerializeField] private float tacticalViewIconScale;
    [SerializeField] private float battleViewIconScale;
    private float worldIconScale;

    private void Awake() {

        canvasGroup = GetComponent<CanvasGroup>();
        animator = GetComponent<Animator>();

        hoveringOverUnitPosition = transform.localPosition;
        troopTypeIcon.sprite = troop.GetTroopSO().troopTypeIconSprite;

        active_PlayerInput = SettingsManager.Instance.GetShowTacticalIconSetting();
        tacticalView = SettingsManager.Instance.GetTacticalViewSetting();

        troopTypeUIGameObject.SetActive(tacticalView);

        if(tacticalView) {
            SetIconTacticalView();
        }
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        SettingsManager.Instance.OnTacticalViewDisabled += SettingsManager_OnTacticalViewDisabled;
        SettingsManager.Instance.OnTacticalViewEnabled += SettingsManager_OnTacticalViewEnabled;
        SettingsManager.Instance.OnShowTacticalIconsDisabled += SettingsManager_OnShowTacticalIconsDisabled;
        SettingsManager.Instance.OnShowTacticalIconsEnabled += SettingsManager_OnShowTacticalIconsEnabled;

        if(troop.IsOwnedByPlayer()) {
            worldIconScale = tacticalViewIconScale;
            transform.localScale = Vector3.one * worldIconScale;
            transform.localPosition = Vector3.zero;
        }

        SetBackgroundSprite();

        canvasGroup.alpha = .6f;
    }

    public override void OnNetworkSpawn() {
        troop.OnTroopPlaced += Troop_OnTroopPlaced;
        troop.OnTroopHPChanged += Troop_OnTroopHPChanged;
    }


    private void SetBackgroundSprite() {
        FactionSO deckFactionSO = DeckManager.LocalInstance.GetDeckSelected().deckFactionSO;

        if (!troop.IsOwnedByPlayer()) {
            PlayerCustomizationData opponentCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData();
            deckFactionSO = PlayerCustomizationDataManager.Instance.GetFactionSOFromId(opponentCustomizationData.factionID);
        }
        
        if (!HiddenTacticsMultiplayer.Instance.GetPlayerAndOpponentSameFaction()) {
            troopTypeBackground.sprite = deckFactionSO.troopIconBackgroundSprite_differentPlayerFaction;
        }

        else {
            if (troop.IsOwnedByPlayer()) {
                troopTypeBackground.sprite = deckFactionSO.troopIconBackgroundSprite_differentPlayerFaction;
            } else {
                troopTypeBackground.sprite = deckFactionSO.troopIconBackgroundSprite_samePlayerFaction_Opponent;
            }
        }
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {

        if(BattleManager.Instance.IsBattlePhaseStarting()) {
            Debug.Log("setting standard view for icon");
            troopTypeUIGameObject.SetActive(active_PlayerInput);
            SetIconStandardView();
        }

        if (BattleManager.Instance.IsBattlePhaseEnding()) {
            Debug.Log("setting tactical view for icon");
            troopTypeUIGameObject.SetActive(false);
            SetIconTacticalView();
        }
    }

    private void Troop_OnTroopHPChanged(object sender, System.EventArgs e) {
        if(troop.GetTroopHPNormalized() <= 0) {
            tacticalView = false;
            troopTypeUIGameObject.SetActive(tacticalView && active_PlayerInput);
        } else {
            if(!tacticalView) {
                tacticalView = true;
                troopTypeUIGameObject.SetActive(tacticalView && active_PlayerInput);
            }
        }
    }

    private void Troop_OnTroopPlaced(object sender, System.EventArgs e) {
        canvasGroup.alpha = 1f;
        Debug.Log("troop placed received! is preparation phase ? " + BattleManager.Instance.IsPreparationPhase());

        if (BattleManager.Instance.IsPreparationPhase()) {

            troopTypeUIGameObject.SetActive(tacticalView);

        } else {
            SetIconStandardView();
            troopTypeUIGameObject.SetActive(active_PlayerInput);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!BattleManager.Instance.IsBattlePhase()) return;
        SetUIHovered();

        BattlePhaseIPlaceablePanel.Instance.OpenIPlaceableCard(troop);

        foreach(Unit unit in troop.GetUnitInTroopList()) {
            if(!unit.GetUnitIsBought()) continue;

            unit.GetUnitVisual().SetUnitSelected(true);
        }

    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!BattleManager.Instance.IsBattlePhase()) return;
        ResetUI();

        BattlePhaseIPlaceablePanel.Instance.CloseIPlaceableCard(troop);

        foreach (Unit unit in troop.GetUnitInTroopList()) {
            if (!unit.GetUnitIsBought()) continue;

            unit.GetUnitVisual().SetUnitHovered(false);
        }
    }

    public void SetUIHovered() {
        canvasGroup.alpha = 1f;
        animator.SetTrigger("Grow");
        animator.ResetTrigger("Shrink");
    }

    public void SetUIUnHovered() {
        canvasGroup.alpha = 1f;
        animator.SetTrigger("Shrink");
        animator.ResetTrigger("Grow");
    }

    public void ResetUI() {
        canvasGroup.alpha = .8f;
        animator.SetTrigger("Shrink");
        animator.ResetTrigger("Grow");
    }

    public void SetUISelected(bool selected) {
        if(selected) {
            troopTypeOutline.material = troopSelectedMaterial;
            troopTypeBackground.material = troopSelectedMaterial;
        } else {
            troopTypeOutline.material = cleanMaterial;
            troopTypeBackground.material = cleanMaterial;
            SetUIUnHovered();
        }
    }

    private void SetIconTacticalView() {
        Debug.Log("set icon tactical view");
        canvasGroup.alpha = 1f;
        animator.SetTrigger("Shrink");
        transform.localPosition = Vector3.zero;
        worldIconScale = tacticalViewIconScale;
        transform.localScale = Vector3.one * worldIconScale;
    }

    private void SetIconStandardView() {
        Debug.Log("set icon standards view");
        canvasGroup.alpha = .8f;
        transform.position = transform.parent.position;
        transform.localPosition = hoveringOverUnitPosition;
        worldIconScale = battleViewIconScale;
        transform.localScale = Vector3.one * worldIconScale;

        if(active_PlayerInput) {
            troopTypeUIGameObject.SetActive(true);
        }
    }

    private void SettingsManager_OnShowTacticalIconsEnabled(object sender, System.EventArgs e) {
        active_PlayerInput = SettingsManager.Instance.GetShowTacticalIconSetting();

        if(tacticalView) {
            troopTypeUIGameObject.SetActive(true);
        } else {
            troopTypeUIGameObject.SetActive(active_PlayerInput);
        }
    }

    private void SettingsManager_OnShowTacticalIconsDisabled(object sender, System.EventArgs e) {
        active_PlayerInput = SettingsManager.Instance.GetShowTacticalIconSetting();
        troopTypeUIGameObject.SetActive(active_PlayerInput);
    }

    private void SettingsManager_OnTacticalViewEnabled(object sender, System.EventArgs e) {

        tacticalView = true;
        troopTypeUIGameObject.SetActive(true);
    }

    private void SettingsManager_OnTacticalViewDisabled(object sender, System.EventArgs e) {
        tacticalView = false;
        troopTypeUIGameObject.SetActive(false);
    }

    private void OnDestroy() {
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged; 

        SettingsManager.Instance.OnTacticalViewDisabled -= SettingsManager_OnTacticalViewDisabled;
        SettingsManager.Instance.OnTacticalViewEnabled -= SettingsManager_OnTacticalViewEnabled;
        SettingsManager.Instance.OnShowTacticalIconsDisabled -= SettingsManager_OnShowTacticalIconsDisabled;
        SettingsManager.Instance.OnShowTacticalIconsEnabled -= SettingsManager_OnShowTacticalIconsEnabled;
    }
}
