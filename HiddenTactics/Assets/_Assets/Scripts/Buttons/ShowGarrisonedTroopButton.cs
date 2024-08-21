using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowGarrisonedTroopButton : MonoBehaviour
{
    private Button button;
    private TroopSO troopSO;
    private BuildingSO buildingSO;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material cleanMaterial;

    [SerializeField] private Image troopIcon;
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image backgroundImage;

    private void Awake() {
        button = GetComponent<Button>();
    }

    public void SetGarrisonedTroopSO(TroopSO troopSO) {
        this.troopSO = troopSO;
        troopIcon.sprite = troopSO.troopIllustrationSlotSprite;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            IPlaceableDescriptionSlotTemplate.Instance.SetDescriptionSlot(troopSO, troopSO.unitPrefab.GetComponent<Unit>().GetUnitSO(), true);
        });
    }

    public void SetChildTroopSO(TroopSO troopSO) {
        this.troopSO = troopSO;
        troopIcon.sprite = troopSO.troopIllustrationSlotSprite;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            IPlaceableDescriptionSlotTemplate.Instance.SetDescriptionSlot(troopSO, troopSO.unitPrefab.GetComponent<Unit>().GetUnitSO(), false, true);
        });
    }

    public void SetBuildingSO(BuildingSO buildingSO) {
        this.buildingSO = buildingSO;
        troopIcon.sprite = buildingSO.buildingRecruitmentSlotSprite;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            IPlaceableDescriptionSlotTemplate.Instance.SetDescriptionSlot(buildingSO);
        });
    }

    public void SetSelected(bool selected) {
        if (selected) {
            outlineImage.material = selectedMaterial;
            backgroundImage.material = selectedMaterial;
            troopIcon.material = selectedMaterial;
        }
        else {
            outlineImage.material = cleanMaterial;
            backgroundImage.material = cleanMaterial;
            troopIcon.material = cleanMaterial;
        }
    }
}
