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
    [SerializeField] private Image iPlaceableDetailedHPBar;
    [SerializeField] private Image iPlaceableTypeIcon;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI iPlaceableNameText;

    [SerializeField] private Transform singleIconsContainer;
    [SerializeField] private Transform singleIconTemplate;

    [SerializeField] private Gradient healthGradient = new Gradient();

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material selectedMaterial;

    private List<IPlaceableSlotTemplate_SingleUnitIcon> singleUnitIconsList = new List<IPlaceableSlotTemplate_SingleUnitIcon>();

    private IPlaceable iPlaceable;

    private bool cardOpened;
    private bool cardSelected;
    private bool pointerEntered;

    private void Awake() {
        animator = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        singleIconTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        GameInput.Instance.OnLeftClickPerformed += GameInput_OnLeftClickPerformed;
        GameInput.Instance.OnRightClickPerformed += GameInput_OnRightClickPerformed;
    }

    private void GameInput_OnRightClickPerformed(object sender, System.EventArgs e) {
        if (!cardSelected) return;
        CloseIPlaceableCard();
    }

    private void GameInput_OnLeftClickPerformed(object sender, System.EventArgs e) {
        if (!cardOpened) return;
        if (!pointerEntered) return;
        if (cardSelected) return;

        SelectCard();
    }

    public void UpdateHPBar(float iPlaceableHPNormalized) {
        if(iPlaceableHPNormalized > 0) {
            iPlaceableHPBar.color = healthGradient.Evaluate(iPlaceableHPNormalized);
            iPlaceableNameText.color = healthGradient.Evaluate(iPlaceableHPNormalized);
        } else {
            iPlaceableHPBar.color = Color.black;
            iPlaceableNameText.color = Color.black;
        }

        iPlaceableDetailedHPBar.fillAmount = iPlaceableHPNormalized;
    }

    public void SetIPlaceable(IPlaceable iPlaceable) {
        this.iPlaceable = iPlaceable;

        if (iPlaceable is Troop) {
            iPlaceableNameText.text = (iPlaceable as Troop).GetTroopSO().troopName;
            Troop troop = iPlaceable as Troop;
            troop.OnTroopHPChanged += Troop_OnTroopHPChanged;
            troop.OnTroopSelected += Troop_OnTroopSelected;
            troop.OnTroopUnselected += Troop_OnTroopUnselected;
            troop.OnAdditionalUnitsBought += Troop_OnAdditionalUnitsBought;
            iPlaceableTypeIcon.sprite = troop.GetTroopSO().troopTypeIconSprite;
            RefreshSingleUnitTemplates(troop);
        }
    }

    private void Troop_OnAdditionalUnitsBought(object sender, System.EventArgs e) {
        RefreshSingleUnitTemplates(sender as Troop);
    }

    private void Troop_OnTroopUnselected(object sender, System.EventArgs e) {
        borderImage.material = cleanMaterial;
        backgroundImage.material = cleanMaterial;
    }

    private void Troop_OnTroopSelected(object sender, System.EventArgs e) {
        borderImage.material = selectedMaterial;
        backgroundImage.material = selectedMaterial;

        SelectCard();

        if (!cardOpened) {
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
            SelectCard();
        }
    }

    public IPlaceable GetIPlaceable() {
        return iPlaceable;
    }

    private void RefreshSingleUnitTemplates(Troop troop) {
        foreach(IPlaceableSlotTemplate_SingleUnitIcon singleUnit in singleUnitIconsList) {
            singleUnit.ResetUnit();
            Destroy(singleUnit.gameObject);
        }

        singleUnitIconsList.Clear();

        foreach (Unit unit in troop.GetBoughtUnitInTroopList()) {
            IPlaceableSlotTemplate_SingleUnitIcon singleUnit = Instantiate(singleIconTemplate, singleIconsContainer).GetComponent<IPlaceableSlotTemplate_SingleUnitIcon>();
            singleUnit.SetUnit(unit, this);
            singleUnit.gameObject.SetActive(true);
            singleUnitIconsList.Add(singleUnit);
        }
    }

    public void OpenIPlaceableCard() {
        animator.SetTrigger("SlideUp");
        animator.ResetTrigger("SlideDown");
        cardOpened = true;

        if (cardSelected) return;
        HoverIPlaceableOnBattleField(true);
    }

    public void CloseIPlaceableCard() {
        animator.SetTrigger("SlideDown");
        animator.ResetTrigger("SlideUp");

        cardOpened = false;
        cardSelected = false;
        borderImage.material = cleanMaterial;
        backgroundImage.material = cleanMaterial;

        foreach (IPlaceableSlotTemplate_SingleUnitIcon singleUnit in singleUnitIconsList) {
            singleUnit.SetInteractable(false);
            singleUnit.SetUnitUnSelected();
        }

        HoverIPlaceableOnBattleField(false);
    }

    private void HoverIPlaceableOnBattleField(bool hovered) {

        if(hovered) {

            if (iPlaceable is Troop) {
                Troop troop = (Troop)iPlaceable;
                troop.GetComponentInChildren<TroopTypeUI>().SetUIHovered();

                foreach (Unit unit in troop.GetBoughtUnitInTroopList()) {
                    if(hovered) {
                        unit.GetUnitVisual().SetUnitHovered(true);
                    }
                }
            }

            if (iPlaceable is Building) {
                Building building = (Building)iPlaceable;
                building.GetComponentInChildren<TroopTypeUI>().SetUIHovered();
            }

        } else {

            if (iPlaceable is Troop) {
                Troop troop = (Troop)iPlaceable;
                troop.GetComponentInChildren<TroopTypeUI>().ResetUI();

                foreach (Unit unit in troop.GetBoughtUnitInTroopList()) {
                    unit.GetUnitVisual().SetUnitHovered(false);
                }
            }

            if (iPlaceable is Building) {
                Building building = (Building)iPlaceable;
                building.GetComponentInChildren<TroopTypeUI>().ResetUI();
            }
        }
    }

    private void SelectCard() {
        cardSelected = true;
        cardOpened = true;
        borderImage.material = selectedMaterial;
        backgroundImage.material = selectedMaterial;

        if (iPlaceable is Troop) {
            Troop troop = (Troop)iPlaceable;
            troop.GetComponentInChildren<TroopTypeUI>().SetUIHovered();

            foreach (Unit unit in troop.GetBoughtUnitInTroopList()) {
                unit.SetUnitSelected(true);
            }
        }

        foreach (IPlaceableSlotTemplate_SingleUnitIcon singleUnitIcon in singleUnitIconsList) {
            singleUnitIcon.SetInteractable(true);
        }

        if (iPlaceable is Building) {
            Building building = (Building)iPlaceable;
            building.GetComponentInChildren<TroopTypeUI>().SetUIHovered();
        }
    }

    public void DeselectOtherSlots(IPlaceableSlotTemplate_SingleUnitIcon selectedSlot) {
        foreach(IPlaceableSlotTemplate_SingleUnitIcon singleUnitIcon in singleUnitIconsList) {
            if (singleUnitIcon == selectedSlot) continue;
            singleUnitIcon.SetUnitUnSelected();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        pointerEntered = true;
        OpenIPlaceableCard();
    }

    public void OnPointerExit(PointerEventData eventData) {
        pointerEntered = false;

        if (cardSelected || iPlaceable.GetSelected()) {
            HoverIPlaceableOnBattleField(true);
            return;
        }

        CloseIPlaceableCard();
    }

    public void OnDestroy() {
        GameInput.Instance.OnLeftClickPerformed -= GameInput_OnLeftClickPerformed;
        GameInput.Instance.OnRightClickPerformed -= GameInput_OnRightClickPerformed;

        if (iPlaceable is Troop) {
            Troop troop = iPlaceable as Troop;
            troop.OnTroopHPChanged -= Troop_OnTroopHPChanged;
            troop.OnTroopSelected -= Troop_OnTroopSelected;
            troop.OnTroopUnselected -= Troop_OnTroopUnselected;
            troop.OnAdditionalUnitsBought -= Troop_OnAdditionalUnitsBought;
        }
    }
}
