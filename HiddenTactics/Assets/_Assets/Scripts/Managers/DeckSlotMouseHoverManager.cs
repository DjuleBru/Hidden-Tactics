using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DeckSlotMouseHoverManager : MonoBehaviour
{

    public static DeckSlotMouseHoverManager Instance;

    private List<DeckSlotVisual> deckSlotVisualHoveredList = new List<DeckSlotVisual>();
    private DeckSlotVisual deckSlotVisualHovered;

    private bool editingDeck;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!editingDeck) return;

        if(Input.GetMouseButtonDown(0)) {
            if(deckSlotVisualHovered != null) {
                // Player is clicking on deck slot
                deckSlotVisualHovered.GetComponentInParent<DeckSlot>().SetSelecting(true);
            }
        }

        RaycastHit2D[] hit = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        //Unhover deck slot hovered that are not in the list anymore
        List<DeckSlotVisual> deckSlotsToRemoveFromHoveredList = new List<DeckSlotVisual>();
        foreach (DeckSlotVisual deckSlotVisual in deckSlotVisualHoveredList) {

            bool deckSlotInHoveredList = false;

            foreach (RaycastHit2D hit2d in hit)
            {
                DeckSlotVisual deckSlotVisualHovered = hit2d.collider.GetComponentInChildren<DeckSlotVisual>();

                if (deckSlotVisualHovered != null && deckSlotVisualHovered == deckSlotVisual)
                {
                    deckSlotInHoveredList = true;
                }
            }

            if(!deckSlotInHoveredList)
            {
                deckSlotVisual.SetDeckSlotUnhovered();
                deckSlotsToRemoveFromHoveredList.Add(deckSlotVisual);
            }
        }


        //Remove deck slot hovered that are not in the list anymore
        foreach(DeckSlotVisual deckSlotVisual in deckSlotsToRemoveFromHoveredList)
        {
            deckSlotVisualHoveredList.Remove(deckSlotVisual);
        }


        // Check new deck slots hovered
        bool newDeckSlotVisualHovered = false;

        foreach (RaycastHit2D hit2d in hit)
        {
            DeckSlotVisual deckSlotVisualHovered = hit2d.collider.GetComponentInChildren<DeckSlotVisual>();

            if (deckSlotVisualHovered != null && !deckSlotVisualHoveredList.Contains(deckSlotVisualHovered))
            {
                deckSlotVisualHoveredList.Add(deckSlotVisualHovered);
                deckSlotVisualHovered.SetDeckSlotHovered();
                this.deckSlotVisualHovered = deckSlotVisualHovered;
                newDeckSlotVisualHovered = true;
            }
        }

        //Unhover all previous deck slots
        if(newDeckSlotVisualHovered)
        {
            foreach(DeckSlotVisual deckSlotVisual in deckSlotVisualHoveredList)
            {
                if (deckSlotVisual == deckSlotVisualHovered) return; 

                deckSlotVisual.SetDeckSlotUnhovered();
            }
        }

        // If only 1 item in list : hover
        if(deckSlotVisualHoveredList.Count == 1)
        {
            if (!deckSlotVisualHoveredList[0].GetDeckSlotHovered())
            {
                deckSlotVisualHoveredList[0].SetDeckSlotHovered();
                this.deckSlotVisualHovered = deckSlotVisualHoveredList[0];
            }
        }

        // If 0 items in list : unhover
        if (deckSlotVisualHoveredList.Count == 0)
        {
            this.deckSlotVisualHovered = null;
        }
    }


    public void SetEditingDeck(bool editing)
    {
        editingDeck = editing;
    }

    public bool GetEditingDeck()
    {
        return editingDeck;
    }

}
