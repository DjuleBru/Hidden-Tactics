using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
   public static SettingsManager Instance;

    private bool showTacticalIcons = true;
    private bool tacticalView;

    public event EventHandler OnTacticalViewEnabled;
    public event EventHandler OnTacticalViewDisabled;
    public event EventHandler OnShowTacticalIconsDisabled;
    public event EventHandler OnShowTacticalIconsEnabled;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnShowIPlaceableIconPerformed += GameInput_OnShowIPlaceableIconPerformed;
        GameInput.Instance.OnTacticalViewPerformed += GameInput_OnTacticalViewPerformed;
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseStarting()) {
            if(tacticalView) {
                tacticalView = false;
                OnTacticalViewDisabled?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void GameInput_OnTacticalViewPerformed(object sender, EventArgs e) {
        if (!BattleManager.Instance.IsPreparationPhase()) return;

        tacticalView = !tacticalView;

        if (tacticalView) {
            // Automatically enable troop icons when going into tactical view
            OnTacticalViewEnabled?.Invoke(this, EventArgs.Empty);
            if(!showTacticalIcons) {
                showTacticalIcons = true;
                OnShowTacticalIconsEnabled?.Invoke(this, EventArgs.Empty);
            }
        }
        else {
            OnTacticalViewDisabled?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnShowIPlaceableIconPerformed(object sender, System.EventArgs e) {
        showTacticalIcons = !showTacticalIcons;

        if(showTacticalIcons) {
            OnShowTacticalIconsEnabled?.Invoke(this, EventArgs.Empty);
        } else {
            OnShowTacticalIconsDisabled?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool GetShowTacticalIconSetting() {
        return showTacticalIcons;
    }

    public bool GetTacticalViewSetting() {
        return tacticalView;
    }

}
