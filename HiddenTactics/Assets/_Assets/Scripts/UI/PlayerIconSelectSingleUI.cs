using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIconSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int iconSpriteId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            HiddenTacticsMultiplayer.Instance.SetPlayerIconSprite(iconSpriteId);
            UpdateIsSelected();
        });
    }

    private void Start() {
        image.sprite = HiddenTacticsMultiplayer.Instance.GetPlayerSprite(iconSpriteId);
        UpdateIsSelected();
    }

    private void UpdateIsSelected() {
        if(HiddenTacticsMultiplayer.Instance.GetPlayerData().iconSpriteId == iconSpriteId) {
            selectedGameObject.SetActive(true);
        } else {
            selectedGameObject.SetActive(false);
        }
    }
}
