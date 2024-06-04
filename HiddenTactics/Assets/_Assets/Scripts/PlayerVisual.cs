using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Image icon;

    private Sprite frameSprite;
    private Sprite iconSprite;

    private void Awake() {
        //iconSprite = Sprite.Create(icon.sprite.texture, icon.sprite.rect, icon.sprite.pivot);

        //icon.sprite = iconSprite;
    }

    public void SetPlayerIcon(Sprite sprite) {
        icon.sprite = sprite;
    }
}
