using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FactionSelectionButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private FactionSO factionSO;
    [SerializeField] private Image factionImage;
    [SerializeField] private Image factionImageShadow;
    [SerializeField] private Image factionOutlineImage;
    [SerializeField] private Image factionOutlineShadowImage;
    [SerializeField] private Image factionBackgroundImage;

    private Button button;
    private Animator buttonAnimator;
    private bool selected;

    private void Awake() {
        button = GetComponent<Button>();
        buttonAnimator = GetComponent<Animator>();

        button.onClick.AddListener(() => {
            DeckManager.LocalInstance.SetDeckSelected(factionSO);
        });

        if(factionSO != null) {
             SetFactionSO(factionSO);
        }
    }

    private void Start() {
        if(DeckManager.LocalInstance.GetDeckSelected().deckFactionSO == factionSO) {
            SetSelected(true);
        } else {
            SetSelected(false);
        }

        DeckManager.LocalInstance.OnSelectedDeckChanged += DeckManager_OnSelectedDeckChanged;
        
    }

    private void DeckManager_OnSelectedDeckChanged(object sender, DeckManager.OnDeckChangedEventArgs e) {
        if (DeckManager.LocalInstance.GetDeckSelected().deckFactionSO == factionSO) {
            SetSelected(true);
        }
        else {
            SetSelected(false);
        }
    }

    public void SetFactionSO(FactionSO factionSO) {
        this.factionSO = factionSO;
        factionImage.sprite = factionSO.factionSprite;
        factionImageShadow.sprite = factionSO.factionSprite;
        factionOutlineImage.sprite = factionSO.slotBorder;
        factionOutlineShadowImage.sprite = factionSO.slotBorder;
        factionBackgroundImage.sprite = factionSO.slotBackgroundSquare;
    }

    private void SetSelected(bool selected) {
        this.selected = selected;
        buttonAnimator.SetBool("Selected", selected);
        buttonAnimator.ResetTrigger("Hover");
        buttonAnimator.ResetTrigger("Unhover");

        if (selected) {
            buttonAnimator.Play("Idle_Selected");
        } else {
            buttonAnimator.Play("Idle_Unselected");
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (selected) return;
        buttonAnimator.SetTrigger("Unhover");
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (selected) return;
        buttonAnimator.SetTrigger("Hover");
    }
}
