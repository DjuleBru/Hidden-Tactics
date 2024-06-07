using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlefieldVisualBaseTemplate : MonoBehaviour
{
    [SerializeField] private Image battlefieldBaseImage;
    private Sprite battlefieldBaseSprite;
    private Button button;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            HiddenTacticsMultiplayer.Instance.SetPlayerBattlefieldBaseSprite(PlayerCustomizationDataManager.Instance.GetBattlefieldBaseSpriteID(battlefieldBaseSprite));
            BattlefieldVisual.Instance.SetBattlefieldBaseSprite(battlefieldBaseSprite);
        });
    }

    public void SetBattlefieldBaseImage(Sprite battlefieldBaseSprite) {
        this.battlefieldBaseSprite = battlefieldBaseSprite;
        battlefieldBaseImage.sprite = battlefieldBaseSprite;
    }
}
