using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckVisualWorldUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public static DeckVisualWorldUI Instance;

    [SerializeField] private Button editDeckButton;
    [SerializeField] private List<DeckSlot> deckSlotList;

    private bool hoveringSlots;
    private bool unHoveringSlots;
    private bool editDeckButtonEnabled = true;

    private float hoverSlotTimer;
    private float hoverSlotRate = .025f;
    private int hoverSlotIndex;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        hoverSlotIndex = deckSlotList.Count;
        editDeckButton.onClick.AddListener(() => {
            DeckEditUI.Instance.EnableEditDeckUI();

            foreach(DeckSlot deckslot in deckSlotList) {
                if(deckslot.GetDeckSlotVisual().GetDeckSlotHovered()) {
                    deckslot.GetDeckSlotVisual().SetDeckSlotUnhoveredWithoutConditions();
                }

                deckslot.SetAnimatorActive(false);
                deckslot.GetDeckSlotVisual().DisableDeckSlotHover();
                editDeckButtonEnabled = false;
            }

        });
    }

    private void Update()
    {
        if(hoveringSlots && hoverSlotIndex < deckSlotList.Count)
        {
            hoverSlotTimer += Time.deltaTime;

            if(hoverSlotTimer > hoverSlotRate)
            {
                deckSlotList[hoverSlotIndex].GetComponentInChildren<DeckSlotVisual>().SetDeckSlotHovered();
                hoverSlotIndex++;
                hoverSlotTimer = 0;
            }
        }

        if (!editDeckButtonEnabled) return;


        if (!hoveringSlots && hoverSlotIndex < deckSlotList.Count)
        {
            hoverSlotTimer += Time.deltaTime;

            if (hoverSlotTimer > hoverSlotRate)
            {
                deckSlotList[hoverSlotIndex].GetComponentInChildren<DeckSlotVisual>().SetDeckSlotUnhoveredWithoutConditions();
                hoverSlotIndex++;
                hoverSlotTimer = 0;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoveringSlots = true;
        hoverSlotIndex = 0;
        hoverSlotTimer = hoverSlotRate;
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        hoveringSlots = false;
        hoverSlotIndex = 0;
    }

    public void EnableEditDeckButton()
    {
        editDeckButton.gameObject.SetActive(true);
        editDeckButtonEnabled = true;
    }

    public void DisableEditDeckButton() {
        editDeckButton.gameObject.SetActive(false);
        editDeckButtonEnabled = false;
    }

}
