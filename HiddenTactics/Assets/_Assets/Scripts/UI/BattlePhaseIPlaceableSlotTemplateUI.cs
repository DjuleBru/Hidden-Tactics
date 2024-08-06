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
    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI iPlaceableNameText;

    [SerializeField] private Gradient healthGradient = new Gradient();


    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material selectedMaterial;

    private IPlaceable iPlaceable;

    private bool cardOpened;

    private void Awake() {
        animator = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void UpdateHPBar(float iPlaceableHPNormalized) {
        if(iPlaceableHPNormalized > 0) {
            iPlaceableHPBar.color = healthGradient.Evaluate(iPlaceableHPNormalized);
            iPlaceableNameText.color = healthGradient.Evaluate(iPlaceableHPNormalized);
        } else {
            iPlaceableHPBar.color = Color.black;
            iPlaceableNameText.color = Color.black;
        }
    }

    public void SetIPlaceable(IPlaceable iPlaceable) {
        this.iPlaceable = iPlaceable;

        if (iPlaceable is Troop) {
            iPlaceableNameText.text = (iPlaceable as Troop).GetTroopSO().troopName;
            Troop troop = iPlaceable as Troop;
            troop.OnTroopHPChanged += Troop_OnTroopHPChanged;
            troop.OnTroopSelected += Troop_OnTroopSelected;
            troop.OnTroopUnselected += Troop_OnTroopUnselected;
            iPlaceableTypeIcon.sprite = troop.GetTroopSO().troopTypeIconSprite;
        }

        if(iPlaceable is Building) {
            iPlaceableNameText.text = (iPlaceable as Building).GetBuildingSO().buildingName;
            Building building = iPlaceable as Building;
            building.OnBuildingSelected += Building_OnTroopSelected;
            building.OnBuildingUnselected += Building_OnTroopUnselected;
            iPlaceableTypeIcon.sprite = building.GetBuildingSO().buildingTypeSprite;
        }
    }

    private void Troop_OnTroopUnselected(object sender, System.EventArgs e) {
        borderImage.material = cleanMaterial;
        backgroundImage.material = cleanMaterial;
    }

    private void Troop_OnTroopSelected(object sender, System.EventArgs e) {
        borderImage.material = selectedMaterial;
        backgroundImage.material = selectedMaterial;

        if(!cardOpened) {
            OpenIPlaceableCard();
        }
    }

    private void Troop_OnTroopHPChanged(object sender, System.EventArgs e) {
        float newTroopHealth = (sender as Troop).GetTroopHPNormalized();
        UpdateHPBar(newTroopHealth);
    }

    private void Building_OnTroopUnselected(object sender, System.EventArgs e) {
        borderImage.material = cleanMaterial;
        backgroundImage.material = cleanMaterial;
    }

    private void Building_OnTroopSelected(object sender, System.EventArgs e) {
        borderImage.material = selectedMaterial;
        backgroundImage.material = selectedMaterial;

        if (!cardOpened) {
            OpenIPlaceableCard();
        }
    }


    public IPlaceable GetIPlaceable() {
        return iPlaceable;
    }

    public void OpenIPlaceableCard() {
        animator.SetTrigger("SlideUp");
        animator.ResetTrigger("SlideDown");
        cardOpened = true;

        if (iPlaceable is Troop) {
            Troop troop = (Troop)iPlaceable;
            troop.GetComponentInChildren<TroopTypeUI>().SetUIHovered();

            foreach (Unit unit in troop.GetUnitInTroopList()) {
                if (!unit.GetUnitIsBought()) continue;

                unit.GetUnitVisual().SetUnitSelected(true);
            }
        }

        if (iPlaceable is Building) {
            Building building = (Building)iPlaceable;
            building.GetComponentInChildren<TroopTypeUI>().SetUIHovered();
        }
    }

    public void CloseIPlaceableCard() {
        animator.SetTrigger("SlideDown");
        animator.ResetTrigger("SlideUp");

        cardOpened = false;

        if (iPlaceable is Troop) {
            Troop troop = (Troop)iPlaceable;
            troop.GetComponentInChildren<TroopTypeUI>().ResetUI();

            foreach (Unit unit in troop.GetUnitInTroopList()) {
                if (!unit.GetUnitIsBought()) continue;

                unit.GetUnitVisual().SetUnitHovered(false);
            }
        }

        if (iPlaceable is Building) {
            Building building = (Building)iPlaceable;
            building.GetComponentInChildren<TroopTypeUI>().ResetUI();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        OpenIPlaceableCard();
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (iPlaceable.GetSelected()) return;
        CloseIPlaceableCard();
    }
}
