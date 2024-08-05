using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingUI : MonoBehaviour
{

    [SerializeField] private GameObject hideTroopIconsTextUI;
    [SerializeField] private GameObject tacticalCameraTextUI;
    [SerializeField] private GameObject hideTroopIconsRadioButton;
    [SerializeField] private GameObject tacticalCameraRadioButton;

    private void Start() {
        SettingsManager.Instance.OnShowTacticalIconsDisabled += SettingsManager_OnShowTacticalIconsDisabled;
        SettingsManager.Instance.OnShowTacticalIconsEnabled += SettingsManager_OnShowTacticalIconsEnabled;
        SettingsManager.Instance.OnTacticalViewDisabled += SettingsManager_OnTacticalViewDisabled;
        SettingsManager.Instance.OnTacticalViewEnabled += SettingsManager_OnTacticalViewEnabled;

        hideTroopIconsRadioButton.SetActive(!SettingsManager.Instance.GetShowTacticalIconSetting());
        tacticalCameraRadioButton.SetActive(SettingsManager.Instance.GetTacticalViewSetting());

        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;

        tacticalCameraTextUI.SetActive(false);
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        if(BattleManager.Instance.IsPreparationPhase()) {
            tacticalCameraTextUI.SetActive(true);
        } else {
            tacticalCameraTextUI.SetActive(false);
        }
    }

    private void SettingsManager_OnTacticalViewEnabled(object sender, System.EventArgs e) {
        tacticalCameraRadioButton.SetActive(true);
    }

    private void SettingsManager_OnTacticalViewDisabled(object sender, System.EventArgs e) {
        tacticalCameraRadioButton.SetActive(false);
    }

    private void SettingsManager_OnShowTacticalIconsEnabled(object sender, System.EventArgs e) {
        hideTroopIconsRadioButton.SetActive(false);
    }

    private void SettingsManager_OnShowTacticalIconsDisabled(object sender, System.EventArgs e) {
        hideTroopIconsRadioButton.SetActive(true);
    }
}
