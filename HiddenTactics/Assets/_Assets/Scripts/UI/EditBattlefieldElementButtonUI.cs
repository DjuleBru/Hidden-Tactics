using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditBattlefieldElementButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private bool editGridTiles;
    [SerializeField] private bool editVillages;
    [SerializeField] private bool editBase;

    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Material hoveredMaterial;

    [SerializeField] private SpriteRenderer battlefieldBaseSpriteRenderer;

    public void OnPointerEnter(PointerEventData eventData)
    {

        if (editGridTiles)
        {
            BattleGrid.Instance.GetGridSystem().SetGridSpritesMaterial(hoveredMaterial);
        }

        if(editVillages)
        {
            BattleGrid.Instance.GetGridSystem().SetVillageSpritesMaterial(hoveredMaterial);
        }

        if(editBase) {
            battlefieldBaseSpriteRenderer.material = hoveredMaterial;
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {

        if (editGridTiles)
        {
            BattleGrid.Instance.GetGridSystem().SetGridSpritesMaterial(cleanMaterial);
        }

        if(editVillages)
        {
            BattleGrid.Instance.GetGridSystem().SetVillageSpritesMaterial(cleanMaterial);

        }

        if(editBase)
        {
            battlefieldBaseSpriteRenderer.material = cleanMaterial;

        }
    }
}
