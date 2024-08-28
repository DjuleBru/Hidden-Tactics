using TMPro;
using UnityEngine;

public class DeckSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI addTroopText;

    [SerializeField] private Color addTroopButtonColorWhenUnhovered;

    private DeckSlot deckSlot;
    private bool selectingTroop;

    private void Awake()
    {
        deckSlot = GetComponentInParent<DeckSlot>();

        addTroopText.gameObject.SetActive(false);
        addTroopText.faceColor = addTroopButtonColorWhenUnhovered;
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

    public void SetAddTroopTextHovered() {
        if (!DeckSlotMouseHoverManager.Instance.GetEditingDeck()) return;
        addTroopText.faceColor = Color.white;
    }

    public void SetAddTroopTextUnhovered()
    {
        if (selectingTroop) return;
        addTroopText.faceColor = addTroopButtonColorWhenUnhovered;
    }

    public void SetTroopSelectingVisualUI(bool selectingTroop)
    {
        if(selectingTroop)
        {
            addTroopText.faceColor = Color.white;
        } else
        {
            addTroopText.faceColor = addTroopButtonColorWhenUnhovered;
        }
    }

    public void RefreshAddRemoveButtons()
    {
        if (!DeckSlotMouseHoverManager.Instance.GetEditingDeck()) return;
        if(deckSlot.GetSlotTroopSO() == null && deckSlot.GetSlotBuildingSO() == null)
        {
            EnableAddTroopText();
        } else
        {
            DisableAddTroopText();
        }
    }

    public void EnableAddTroopText()
    {
        addTroopText.gameObject.SetActive(true);
    }

    public void DisableAddTroopText()
    {
        addTroopText.gameObject.SetActive(false);
    }

}
