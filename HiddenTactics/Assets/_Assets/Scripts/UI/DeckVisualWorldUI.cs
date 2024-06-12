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

    private float hoverSlotTimer;
    private float hoverSlotRate = .025f;
    private int hoverSlotIndex;

    private void Awake()
    {
        Instance = this;
        editDeckButton.onClick.AddListener(() =>
        {
            hoveringSlots = false;
            hoverSlotIndex = 0;
            DeckVisualUI.Instance.StartEditingDeck();
            editDeckButton.gameObject.SetActive(false);
        });
    }

    private void Start()
    {
        hoverSlotIndex = deckSlotList.Count;
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
    }

}
