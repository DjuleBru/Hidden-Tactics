using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TroopTypeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Troop troop;
    [SerializeField] private Image troopTypeIcon;
    [SerializeField] private GameObject troopTypeUIGameObject;
    private CanvasGroup canvasGroup;
    private Animator animator;

    private Vector3 hoveringOverUnitPosition;
    private bool active_State = true;
    private bool active_PlayerInput;

    private float worldIconScaleMultiplier;
    private float worldIconScale;

    private void Awake() {
        troop.OnTroopPlaced += Troop_OnTroopPlaced;

        canvasGroup = GetComponent<CanvasGroup>();
        animator = GetComponent<Animator>();

        hoveringOverUnitPosition = transform.localPosition;

        active_PlayerInput = GameInput.Instance.GetShowIPlaceableIconSetting();

        troopTypeIcon.sprite = troop.GetTroopSO().troopTypeIconSprite;

        troopTypeUIGameObject.SetActive(false);
    }

    private void Start() {
        troop.OnTroopHPChanged += Troop_OnTroopHPChanged;
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        GameInput.Instance.OnShowIPlaceableIconPerformed += GameInput_OnShowIPlaceableIconPerformed;
        CameraMovement.Instance.OnCameraZoomedChanged += CameraMovement_OnCameraZoomedChanged;

        worldIconScaleMultiplier = CameraMovement.Instance.GetWorldIconScaleMultiplier();
        worldIconScale = 1.5f;

        transform.localScale = Vector3.one * worldIconScale * worldIconScaleMultiplier;
        transform.localPosition = Vector3.zero;
    }

    private void CameraMovement_OnCameraZoomedChanged(object sender, System.EventArgs e) {
        worldIconScaleMultiplier = CameraMovement.Instance.GetWorldIconScaleMultiplier();
        transform.localScale = Vector3.one * worldIconScale * worldIconScaleMultiplier;
    }

    private void GameInput_OnShowIPlaceableIconPerformed(object sender, System.EventArgs e) {
        active_PlayerInput = GameInput.Instance.GetShowIPlaceableIconSetting();
        troopTypeUIGameObject.SetActive(active_State && active_PlayerInput);
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {

        if (BattleManager.Instance.IsBattlePhaseEnding()) {
            canvasGroup.alpha = 1f;
            animator.SetTrigger("Shrink");
            transform.localPosition = Vector3.zero;
            worldIconScale = 1.5f;
            transform.localScale = Vector3.one * worldIconScale * worldIconScaleMultiplier;
        }

        if(BattleManager.Instance.IsBattlePhase()) {
            canvasGroup.alpha = .8f;
            transform.position = transform.parent.position;
            transform.localPosition = hoveringOverUnitPosition;
            worldIconScale = 1f;
            transform.localScale = Vector3.one * worldIconScale * worldIconScaleMultiplier;
        }
    }

    private void Troop_OnTroopHPChanged(object sender, System.EventArgs e) {
        if(troop.GetTroopHPNormalized() <= 0) {
            active_State = false;
            troopTypeUIGameObject.SetActive(active_State && active_PlayerInput);
        } else {
            if(!active_State) {
                active_State = true;
                troopTypeUIGameObject.SetActive(active_State && active_PlayerInput);
            }
        }
    }

    private void Troop_OnTroopPlaced(object sender, System.EventArgs e) {
        troopTypeUIGameObject.SetActive(active_State && active_PlayerInput);
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
    }

    public void ResetUI() {
        canvasGroup.alpha = .8f;
        animator.SetTrigger("Shrink");
    }

    private void OnDestroy() {
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
        GameInput.Instance.OnShowIPlaceableIconPerformed -= GameInput_OnShowIPlaceableIconPerformed;
        CameraMovement.Instance.OnCameraZoomedChanged -= CameraMovement_OnCameraZoomedChanged;
    }
}
