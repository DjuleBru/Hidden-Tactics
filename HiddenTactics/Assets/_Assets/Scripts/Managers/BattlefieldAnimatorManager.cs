using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BattlefieldAnimatorManager : NetworkBehaviour {

    Animator battlefieldAnimator;
    private bool slammed;
    private bool slamming;
    private bool slammedOrdered;

    private bool split;
    private bool splitOrdered;

    public event EventHandler OnBattlefieldsSlammed;
    public event EventHandler OnBattlefieldsSplit;

    [SerializeField] ParticleSystem[] particleSystems;
    [SerializeField] private bool isPlayerBattlefield;

    private void Start() {
        battlefieldAnimator = GetComponent<Animator>();

        if(HiddenTacticsMultiplayer.Instance.IsMultiplayer()) {
            battlefieldAnimator.enabled = false;
        }

        BattleManager.Instance.OnAllIPlaceablesSpawned += BattleManager_OnAllIPlaceablesSpawned;
    }

    private void Update() {
        if (slammedOrdered & !slammed) {
            if(AnimatorIsPlaying("Battlefield_Slam") & !slamming) {
                slamming = true;
                battlefieldAnimator.speed = 1;
            }

            if(AnimatorIsPlaying("Battlefield_Slam") && battlefieldAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
                // Animator has finished playing slammed animation
                slammed = true;
                OnBattlefieldsSlammed?.Invoke(this, EventArgs.Empty);
                PlaySlamFeedbacks();
            }
        }

        if (splitOrdered & !split) {
            if (AnimatorIsPlaying("Battlefield_Idle")) {
                // Animator has finished playing split animation
                split = true;
                OnBattlefieldsSplit?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void PlaySlamFeedbacks() {
        foreach (ParticleSystem particleSystem in particleSystems) {
            particleSystem.Play();
        }
    }

    private void BattleManager_OnAllIPlaceablesSpawned(object sender, EventArgs e) {
        battlefieldAnimator.enabled = true;
    }


    public void SlamBattlefields() {
        battlefieldAnimator.SetTrigger("Slam");
        battlefieldAnimator.speed = 2f;

        slammedOrdered = true;

        split = false;
        splitOrdered = false;
    }

    public void SplitBattlefields() {
        battlefieldAnimator.SetTrigger("Split");
        splitOrdered = true;

        slammed = false;
        slammedOrdered = false;
        slamming = false;
    }

    public bool AnimatorIsPlaying(string stateName) {
        return battlefieldAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

}
