using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer icon;

    private Sprite frameSprite;
    private Sprite iconSprite;

    private void Awake() {
        iconSprite = Sprite.Create(icon.sprite.texture, icon.sprite.rect, icon.sprite.pivot);

        icon.sprite = iconSprite;
    }

    public void SetPlayerIcon(Sprite sprite) {
        icon.sprite = sprite;
    }
}
