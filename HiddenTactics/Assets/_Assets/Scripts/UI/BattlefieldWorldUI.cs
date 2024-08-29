using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattlefieldWorldUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private Animator battlefieldAnimator;


    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material hoveredMaterial;

    [SerializeField] private SpriteRenderer battlefieldBaseSpriteRenderer;
    [SerializeField] private SpriteRenderer battlefieldOutlineSpriteRenderer;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            EditBattlefieldUI.Instance.SwitchToEditBattlefield();

            BattleGrid.Instance.GetGridSystem().SetGridSpritesMaterial(cleanMaterial);
            BattleGrid.Instance.GetGridSystem().SetVillageSpritesMaterial(cleanMaterial);
            battlefieldBaseSpriteRenderer.material = cleanMaterial;
            battlefieldOutlineSpriteRenderer.material = cleanMaterial;
            battlefieldAnimator.SetTrigger("Unhovered");
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BattleGrid.Instance.GetGridSystem().SetGridSpritesMaterial(hoveredMaterial);
        BattleGrid.Instance.GetGridSystem().SetVillageSpritesMaterial(hoveredMaterial);
        battlefieldBaseSpriteRenderer.material = hoveredMaterial;
        battlefieldOutlineSpriteRenderer.material = hoveredMaterial;

        battlefieldAnimator.SetTrigger("Hovered");

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BattleGrid.Instance.GetGridSystem().SetGridSpritesMaterial(cleanMaterial);
        BattleGrid.Instance.GetGridSystem().SetVillageSpritesMaterial(cleanMaterial);
        battlefieldBaseSpriteRenderer.material = cleanMaterial;
        battlefieldOutlineSpriteRenderer.material = cleanMaterial;
        battlefieldAnimator.SetTrigger("Unhovered");
    }
}
