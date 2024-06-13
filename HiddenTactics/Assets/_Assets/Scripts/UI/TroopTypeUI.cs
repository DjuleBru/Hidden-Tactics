using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TroopTypeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Troop troop;
    [SerializeField] private GameObject troopTypeUIGameObject;
    private CanvasGroup canvasGroup;
    private Animator animator;

    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
        animator = GetComponent<Animator>();
        troopTypeUIGameObject.SetActive(false);
    }

    private void Start() {
        troop.OnTroopPlaced += Troop_OnTroopPlaced;
    }

    private void Troop_OnTroopPlaced(object sender, System.EventArgs e) {
        troopTypeUIGameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!BattleManager.Instance.IsBattlePhase()) return;
        foreach(Unit unit in troop.GetUnitInTroopList()) {
            if(!unit.GetUnitIsBought()) continue;

            unit.GetUnitVisual().SetUnitSelected(true);
            canvasGroup.alpha = 1f;
            animator.SetTrigger("Grow");
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!BattleManager.Instance.IsBattlePhase()) return;
        foreach (Unit unit in troop.GetUnitInTroopList()) {
            if (!unit.GetUnitIsBought()) continue;

            unit.GetUnitVisual().SetUnitHovered(false);
            canvasGroup.alpha = .75f;
            animator.SetTrigger("Shrink");
        }
    }
}
