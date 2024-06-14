using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattlePhaseIPlaceableSlotTemplateUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private Animator animator;
    private CanvasGroup canvasGroup;

    [SerializeField] private Image iPlaceableHPBar;
    [SerializeField] private Image iPlaceableTypeIcon;
    [SerializeField] private TextMeshProUGUI iPlaceableNameText;

    [SerializeField] private Gradient healthGradient = new Gradient();

    private IPlaceable iPlaceable;

    private void Awake() {
        animator = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        //canvasGroup.alpha = .75f;
    }

    public void UpdateHPBar(float iPlaceableHPNormalized) {
        if(iPlaceableHPNormalized > 0) {
            iPlaceableHPBar.color = healthGradient.Evaluate(iPlaceableHPNormalized);
        } else {
            iPlaceableHPBar.color = Color.black;
        }
    }

    public void SetIPlaceable(IPlaceable iPlaceable) {
        this.iPlaceable = iPlaceable;

        if (iPlaceable is Troop) {
            iPlaceableNameText.text = (iPlaceable as Troop).GetTroopSO().name;
            Troop troop = iPlaceable as Troop;
            troop.OnTroopHPChanged += Troop_OnTroopHPChanged;
            iPlaceableTypeIcon.sprite = troop.GetTroopSO().troopTypeIconSprite;
        }
    }

    private void Troop_OnTroopHPChanged(object sender, System.EventArgs e) {
        float newTroopHealth = (sender as Troop).GetTroopHPNormalized();
        UpdateHPBar(newTroopHealth);
    }

    public IPlaceable GetIPlaceable() {
        return iPlaceable;
    }

    public void OpenIPlaceableCard() {
        animator.SetTrigger("SlideUp");

        if(iPlaceable is Troop) {
            Troop troop = (Troop)iPlaceable;
            troop.GetComponentInChildren<TroopTypeUI>().SetUIHovered();

            foreach (Unit unit in troop.GetUnitInTroopList()) {
                if (!unit.GetUnitIsBought()) continue;

                unit.GetUnitVisual().SetUnitSelected(true);
            }
        }
    }

    public void CloseIPlaceableCard() {
        animator.SetTrigger("SlideDown"); 
        
        if (iPlaceable is Troop) {
            Troop troop = (Troop)iPlaceable;
            troop.GetComponentInChildren<TroopTypeUI>().ResetUI();

            foreach (Unit unit in troop.GetUnitInTroopList()) {
                if (!unit.GetUnitIsBought()) continue;

                unit.GetUnitVisual().SetUnitHovered(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        OpenIPlaceableCard();
    }

    public void OnPointerExit(PointerEventData eventData) {
        CloseIPlaceableCard();
    }
}
