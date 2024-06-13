using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemTemplateUI_UnlockItem : ItemTemplateUI, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Button unlockItemButton;

    public void OnPointerEnter(PointerEventData eventData) {
        PlayerStateUI.Instance.SetPlayerGoldChangingUI(-BattleDeckUI.Instance.GetNextUnlockCost());
    }

    public void OnPointerExit(PointerEventData eventData) {
        PlayerStateUI.Instance.ResetPlayerGoldChangingUI();
    }

    private void Awake() {



        unlockItemButton.onClick.AddListener(() => { 

            if(troopSO != null) {
                if(CheckUnlockConditions()) {
                    PlayerGoldManager.Instance.SpendGold(BattleDeckUI.Instance.GetNextUnlockCost(), NetworkManager.Singleton.LocalClientId);
                    BattleDeckUI.Instance.AddNewTroop(troopSO);
                    BattleDeckUI.Instance.CloseUnlockPanel();
                }
            }

            if(buildingSO != null) {
                if(CheckUnlockConditions()) {
                    PlayerGoldManager.Instance.SpendGold(BattleDeckUI.Instance.GetNextUnlockCost(), NetworkManager.Singleton.LocalClientId);
                    BattleDeckUI.Instance.AddNewBuilding(buildingSO);
                    BattleDeckUI.Instance.CloseUnlockPanel();
                }
            }

        });
    }

    private bool CheckUnlockConditions() {
        if (PlayerGoldManager.Instance.CanSpendGold(BattleDeckUI.Instance.GetNextUnlockCost(), NetworkManager.Singleton.LocalClientId)) {
            return true;
        }
        else {
            return false;
        }
    }
}
