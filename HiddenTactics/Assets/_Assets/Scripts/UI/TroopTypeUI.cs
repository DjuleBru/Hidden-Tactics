using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TroopTypeUI : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Troop troop;
    [SerializeField] private Building building;
    private IPlaceable iPlaceable;

    [SerializeField] private Image typeIcon;
    [SerializeField] private Image typeBackground;
    [SerializeField] private Image typeOutline;
    [SerializeField] private GameObject typeUIGameObject;

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material troopSelectedMaterial;

    private CanvasGroup canvasGroup;
    private Animator animator;

    private Vector3 hoveringOverUnitPosition;
    private Vector3 UIResetScale = Vector3.one;
    private bool active_PlayerInput;

    private float worldIconScaleMultiplier;
    [SerializeField] private float tacticalViewIconScale;
    [SerializeField] private float battleViewIconScale;
    private float worldIconScale;

    private bool troopIsGarrisoned;
    private Button button;

    private void Awake() {

        canvasGroup = GetComponent<CanvasGroup>();
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            SetTroopSelected();
        });

        hoveringOverUnitPosition = transform.localPosition;

        if(troop != null) {
            iPlaceable = troop;
            typeIcon.sprite = troop.GetTroopSO().troopTypeIconSprite;
        } else {
            iPlaceable = building;
            typeIcon.sprite = building.GetBuildingSO().buildingTypeSprite;
        }

        active_PlayerInput = SettingsManager.Instance.GetShowTacticalIconSetting();

        // Check if troop is garrisoned: if it is, disable game object
        
        if (troop != null) {
            troopIsGarrisoned = troop.GetTroopSO().isGarrisonedTroop;
            if (troopIsGarrisoned) {
                typeUIGameObject.SetActive(false);
                return;
            }
        }

        if(SettingsManager.Instance.GetTacticalViewSetting()) {
            SetIconTacticalView();
        } else {
            SetIconStandardView();
            typeUIGameObject.SetActive(active_PlayerInput);
        }

    }

    private void Start() {
        SettingsManager.Instance.OnTacticalViewDisabled += SettingsManager_OnTacticalViewDisabled;
        SettingsManager.Instance.OnTacticalViewEnabled += SettingsManager_OnTacticalViewEnabled;
        SettingsManager.Instance.OnShowTacticalIconsDisabled += SettingsManager_OnShowTacticalIconsDisabled;
        SettingsManager.Instance.OnShowTacticalIconsEnabled += SettingsManager_OnShowTacticalIconsEnabled;
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        SetBackgroundSprite();
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(!BattleManager.Instance.IsPreparationPhase()) {
            SetIconStandardView();
        }
    }

    public override void OnNetworkSpawn() {
        if(troop != null) {
            troop.OnTroopPlaced += Troop_OnTroopPlaced;
            troop.OnTroopHPChanged += Troop_OnTroopHPChanged;
        } else {
            building.OnBuildingPlaced += Building_OnBuildingPlaced;
        }
    }


    private void SetBackgroundSprite() {
        FactionSO deckFactionSO = DeckManager.LocalInstance.GetDeckSelected().deckFactionSO;

        if (!iPlaceable.IsOwnedByPlayer()) {
            PlayerCustomizationData opponentCustomizationData = HiddenTacticsMultiplayer.Instance.GetLocalOpponentCustomizationData();
            deckFactionSO = PlayerCustomizationDataManager.Instance.GetFactionSOFromId(opponentCustomizationData.factionID);
        }
        
        if (!HiddenTacticsMultiplayer.Instance.GetPlayerAndOpponentSameFaction()) {
            typeBackground.sprite = deckFactionSO.troopIconBackgroundSprite_differentPlayerFaction;
        }

        else {
            if (iPlaceable.IsOwnedByPlayer()) {
                typeBackground.sprite = deckFactionSO.troopIconBackgroundSprite_differentPlayerFaction;
            } else {
                typeBackground.sprite = deckFactionSO.troopIconBackgroundSprite_samePlayerFaction_Opponent;
            }
        }
    }

    private void Troop_OnTroopHPChanged(object sender, System.EventArgs e) {
        if(troop.GetTroopHPNormalized() <= 0) {
            typeUIGameObject.SetActive(false);
        }
    }

    private void Troop_OnTroopPlaced(object sender, System.EventArgs e) {
        if (troopIsGarrisoned) return;
        if (!troop.IsOwnedByPlayer()) {
            UIResetScale = new Vector3(-1, 1, 1);
        }
        if (SettingsManager.Instance.GetTacticalViewSetting()) {
            canvasGroup.alpha = 1f;
        }

        if (BattleManager.Instance.IsPreparationPhase()) {
            typeUIGameObject.SetActive(active_PlayerInput);

        } else {
            SetIconStandardView();
            typeUIGameObject.SetActive(active_PlayerInput);
        }
    }


    private void Building_OnBuildingPlaced(object sender, System.EventArgs e) {
        if (!building.IsOwnedByPlayer()) {
            UIResetScale = new Vector3(-1, 1, 1);
        }
        if (SettingsManager.Instance.GetTacticalViewSetting()) {
            canvasGroup.alpha = 1f;
        }

        if (BattleManager.Instance.IsPreparationPhase()) {
            typeUIGameObject.SetActive(active_PlayerInput);

        }
        else {
            SetIconStandardView();
            typeUIGameObject.SetActive(active_PlayerInput);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!BattleManager.Instance.IsBattlePhase()) return;
        SetUIHovered();

        BattlePhaseIPlaceablePanel.Instance.OpenIPlaceableCard(troop);

        foreach(Unit unit in troop.GetUnitInTroopList()) {
            if(!unit.GetUnitIsBought()) continue;

            unit.SetUnitHovered(true);
        }

    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!BattleManager.Instance.IsBattlePhase()) return;
        if (troop.GetSelected()) return;

        ResetUI();

        BattlePhaseIPlaceablePanel.Instance.CloseIPlaceableCard(troop);

        foreach (Unit unit in troop.GetUnitInTroopList()) {
            if (!unit.GetUnitIsBought()) continue;

            unit.SetUnitHovered(false);
        }
    }

    private void SetTroopSelected() {
        if (!BattleManager.Instance.IsBattlePhase()) return;
        SetUISelected(true);
        PlayerAction_SelectTroop.LocalInstance.SelectTroop(troop);
    }

    public void SetUIHovered() {
        canvasGroup.alpha = 1f;
        animator.SetTrigger("Grow");
        animator.ResetTrigger("Shrink");
        animator.ResetTrigger("Select");
    }

    public void SetUIUnHovered() {

        if (!SettingsManager.Instance.GetTacticalViewSetting()) {
            canvasGroup.alpha = .7f;
        }
        animator.SetTrigger("Shrink");
        animator.ResetTrigger("Grow");
        animator.ResetTrigger("Select");
    }

    public void ResetUI() {
        if (!SettingsManager.Instance.GetTacticalViewSetting()) {
            canvasGroup.alpha = .7f;
        }

        animator.SetTrigger("Shrink");
        animator.ResetTrigger("Grow");
        animator.ResetTrigger("Select");
    }

    public void SetUISelected(bool selected) {
        if(selected) {
            canvasGroup.alpha = 1f;
            typeOutline.material = troopSelectedMaterial;
            typeBackground.material = troopSelectedMaterial;
            animator.SetTrigger("Select");
            animator.ResetTrigger("Shrink");
            animator.ResetTrigger("Grow");
        } else {
            typeOutline.material = cleanMaterial;
            typeBackground.material = cleanMaterial;
            SetUIUnHovered();
        }
    }

    private void SetIconTacticalView() {
        if (troopIsGarrisoned) return;

        typeUIGameObject.SetActive(true);

        canvasGroup.alpha = 1f;
        animator.SetTrigger("Shrink");
        animator.ResetTrigger("Grow");
        animator.ResetTrigger("Select");

        transform.localPosition = Vector3.zero;

        worldIconScale = tacticalViewIconScale;
        transform.localScale = UIResetScale * worldIconScale;
    }

    private void SetIconStandardView() {
        if (troopIsGarrisoned) return;

        canvasGroup.alpha = .7f;
        transform.position = transform.parent.position;

        transform.localPosition = hoveringOverUnitPosition;

        worldIconScale = battleViewIconScale;
        transform.localScale = UIResetScale * worldIconScale;

        if(active_PlayerInput) {
            typeUIGameObject.SetActive(true);
        }
    }

    private void SettingsManager_OnShowTacticalIconsEnabled(object sender, System.EventArgs e) {
        if (troopIsGarrisoned) return;

        active_PlayerInput = SettingsManager.Instance.GetShowTacticalIconSetting();

        if(SettingsManager.Instance.GetTacticalViewSetting()) {
            typeUIGameObject.SetActive(true);
        } else {
            typeUIGameObject.SetActive(active_PlayerInput);
        }
    }

    private void SettingsManager_OnShowTacticalIconsDisabled(object sender, System.EventArgs e) {
        if (troopIsGarrisoned) return;

        active_PlayerInput = SettingsManager.Instance.GetShowTacticalIconSetting();
        typeUIGameObject.SetActive(active_PlayerInput);
    }

    private void SettingsManager_OnTacticalViewEnabled(object sender, System.EventArgs e) {
        if (troopIsGarrisoned) return;

        typeUIGameObject.SetActive(true);
        SetIconTacticalView();
    }

    private void SettingsManager_OnTacticalViewDisabled(object sender, System.EventArgs e) {
        if (troopIsGarrisoned) return;

        typeUIGameObject.SetActive(false);
        SetIconStandardView();
    }

    public override void OnDestroy() {
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged; 

        SettingsManager.Instance.OnTacticalViewDisabled -= SettingsManager_OnTacticalViewDisabled;
        SettingsManager.Instance.OnTacticalViewEnabled -= SettingsManager_OnTacticalViewEnabled;
        SettingsManager.Instance.OnShowTacticalIconsDisabled -= SettingsManager_OnShowTacticalIconsDisabled;
        SettingsManager.Instance.OnShowTacticalIconsEnabled -= SettingsManager_OnShowTacticalIconsEnabled;
    }
}
