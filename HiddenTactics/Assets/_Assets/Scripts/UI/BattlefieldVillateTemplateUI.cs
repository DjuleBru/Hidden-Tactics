using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlefieldVillateTemplateUI : MonoBehaviour
{
    [SerializeField] private Image villageImage;
    [SerializeField] private Image villageOutlineImage;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material cleanMaterial;

    private bool selected;
    private Sprite villageSprite;
    private Button button;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            AddOrRemoveVillageSprite();
        });
    }

    private void AddOrRemoveVillageSprite() {

        if (selected) {
            if (BattleGridVisual.Instance.TryRemovePlayerVillageSprites(villageSprite)) {
                selected = !selected;
                SetVillageSelected(false);
            }
        } else {
            if (BattleGridVisual.Instance.TryAddPlayerVillageSprites(villageSprite)) {
                selected = !selected;
                SetVillageSelected(true);
            }
        }
    }

    public void SetVillageSpriteVisual(Sprite sprite) {
        this.villageSprite = sprite;

        villageImage.sprite = sprite;
    }

    public void SetVillageSelected(bool selected) {
        this.selected = selected;

        if(selected) {
            villageOutlineImage.material = selectedMaterial;
        } else {
            villageOutlineImage.material = cleanMaterial;
        }
    }
}
