using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button addTroopButton;
    [SerializeField] private Button removeSlotContentButton;

    [SerializeField] private Color addTroopButtonColorWhenUnhovered;

    private DeckSlot deckSlot;
    private bool selectingTroop;

    private void Awake()
    {
        deckSlot = GetComponentInParent<DeckSlot>();

        addTroopButton.gameObject.SetActive(false);
        addTroopButton.GetComponentInChildren<TextMeshProUGUI>().faceColor = addTroopButtonColorWhenUnhovered;
        removeSlotContentButton.gameObject.SetActive(false);

        addTroopButton.onClick.AddListener(() =>
        {
            deckSlot.SetSelecting(true);
        });

        removeSlotContentButton.onClick.AddListener(() =>
        {
            deckSlot.RemoveSlotContent();
            EnableAddTroopButton();
            DisableRemoveTroopButton();
        });

    }

    private void Update()
    {
        if(deckSlot.GetSelecting())
        {
            if(Input.GetMouseButtonDown(1))
            {
                deckSlot.SetSelecting(false);
            }
        }
    }
    public void SetSelectingTroop(bool selectingTroop)
    {
        this.selectingTroop = selectingTroop;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        addTroopButton.GetComponentInChildren<TextMeshProUGUI>().faceColor = Color.white;
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (selectingTroop) return;
        addTroopButton.GetComponentInChildren<TextMeshProUGUI>().faceColor = addTroopButtonColorWhenUnhovered;
    }

    public void SetTroopSelectingVisualUI(bool selectingTroop)
    {
        if(selectingTroop)
        {
            addTroopButton.GetComponentInChildren<TextMeshProUGUI>().faceColor = Color.white;
        } else
        {
            addTroopButton.GetComponentInChildren<TextMeshProUGUI>().faceColor = addTroopButtonColorWhenUnhovered;
        }
    }

    public void RefreshAddRemoveButtons()
    {
        if (!DeckSlotMouseHoverManager.Instance.GetEditingDeck()) return;
        if(deckSlot.GetSlotTroopSO() == null && deckSlot.GetSlotBuildingSO() == null)
        {
            EnableAddTroopButton();
            DisableRemoveTroopButton();
        } else
        {
            EnableRemoveTroopButton();
            DisableAddTroopButton();
        }
    }

    public void EnableAddTroopButton()
    {
        addTroopButton.gameObject.SetActive(true);
    }

    public void EnableRemoveTroopButton()
    {
        removeSlotContentButton.gameObject.SetActive(true);
    }

    public void DisableAddTroopButton()
    {
        addTroopButton.gameObject.SetActive(false);
    }

    public void DisableRemoveTroopButton()
    {
        removeSlotContentButton.gameObject.SetActive(false);
    }

}
