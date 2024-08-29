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
    private bool flyingUp;
    private bool flyingDown;
    private bool flyingDownDown;
    private bool flyingDownUp;
    private bool unHoveringSlots;
    private bool editDeckButtonEnabled = true;

    private float hoverSlotTimer;
    private float hoverSlotRate = .025f;
    private float flySlotRate = .01f;
    private int hoverSlotIndex;
    private int flySlotIndex;
    private float flySlotDownTime = 2.5f;
    private float flySlotDownTimer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        hoverSlotIndex = deckSlotList.Count;
        editDeckButton.onClick.AddListener(() => {
            DeckEditUI.Instance.EnableEditDeckUI();

            foreach (DeckSlot deckslot in deckSlotList) {
                if (deckslot.GetDeckSlotVisual().GetDeckSlotHovered()) {
                    deckslot.GetDeckSlotVisual().SetDeckSlotUnhoveredWithoutConditions();
                }

                deckslot.GetDeckSlotVisual().DisableDeckSlotHover();
                editDeckButtonEnabled = false;
            }


        });
    }

    private void Update()
    {
        HandleSlotHover();
        HandleSlotFlyUp();
        HandleSlotFlyDown();
        HandleSlotFlyDownUp();
        HandleSlotFlyDownDown();
    }

    private void HandleSlotHover() {
        if (flyingDown || flyingUp) return;
        if (hoveringSlots && hoverSlotIndex < deckSlotList.Count) {
            hoverSlotTimer += Time.deltaTime;

            if (hoverSlotTimer > hoverSlotRate) {
                deckSlotList[hoverSlotIndex].GetComponentInChildren<DeckSlotVisual>().SetDeckSlotHovered();
                hoverSlotIndex++;
                hoverSlotTimer = 0;
            }
        }

        if (!editDeckButtonEnabled) return;

        if (!hoveringSlots && hoverSlotIndex < deckSlotList.Count) {
            hoverSlotTimer += Time.deltaTime;

            if (hoverSlotTimer > hoverSlotRate) {
                deckSlotList[hoverSlotIndex].GetComponentInChildren<DeckSlotVisual>().SetDeckSlotUnhoveredWithoutConditions();
                hoverSlotIndex++;
                hoverSlotTimer = 0;
            }
        }
    }

    private void HandleSlotFlyUp() {

        if (flyingUp && flySlotIndex < deckSlotList.Count) {
            hoverSlotTimer += Time.deltaTime;

            if (hoverSlotTimer > flySlotRate) {
                deckSlotList[flySlotIndex].TriggerFlyUp();
                flySlotIndex++;
                hoverSlotTimer = 0;
            }
        }
    }

    private void HandleSlotFlyDown() {

        if (flyingDown && flySlotIndex < deckSlotList.Count) {
            hoverSlotTimer += Time.deltaTime;

            if (hoverSlotTimer > flySlotRate) {
                deckSlotList[flySlotIndex].TriggerFlyDown();
                flySlotIndex++;
                hoverSlotTimer = 0;
            }
        }

        if (flyingDown && flySlotIndex == (deckSlotList.Count)) {
            flyingDown = false;
            flySlotIndex = 0;
            hoverSlotTimer = 0;
        }
    }

    private void HandleSlotFlyDownUp() {

        if (flyingDownUp && flySlotIndex < deckSlotList.Count) {
            hoverSlotTimer += Time.deltaTime;

            if (hoverSlotTimer > flySlotRate) {
                deckSlotList[flySlotIndex].TriggerFlyDownUp();
                flySlotIndex++;
                hoverSlotTimer = 0;
            }
        }
    }

    private void HandleSlotFlyDownDown() {

        if (flyingDownDown && flySlotIndex < deckSlotList.Count) {
            hoverSlotTimer += Time.deltaTime;

            if (hoverSlotTimer > flySlotRate) {
                deckSlotList[flySlotIndex].TriggerFlyDownDown();
                flySlotIndex++;
                hoverSlotTimer = 0;
            }
        }

        if (flyingDownDown && flySlotIndex == (deckSlotList.Count)) {
            flyingDownDown = false;
            flySlotIndex = 0;
            hoverSlotTimer = 0;
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

    public void SetDeckVisualFlyUp() {
        flyingUp = true;
        flyingDown = false;
        flySlotIndex = 0;
    }
    public void SetDeckVisualFlyDown() {
        flyingUp = false;
        flyingDown = true;
        flySlotIndex = 0;
    }

    public void SetDeckVisualFlyDownUp() {
        flyingDownUp = true;
        flyingDownDown = false;
        flySlotIndex = 0;
    }
    public void SetDeckVisualFlyDownDown() {
        flyingDownUp = false;
        flyingDownDown = true;
        flySlotIndex = 0;
    }
}
