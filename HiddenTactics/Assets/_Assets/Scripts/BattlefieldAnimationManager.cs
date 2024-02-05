using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlefieldAnimationManager : MonoBehaviour {

    public static BattlefieldAnimationManager Instance;

    [SerializeField] Animator battlefield1Animator;
    [SerializeField] Animator battlefield2Animator;
    [SerializeField] ParticleSystem[] particleSystems;

    public event EventHandler OnBattlefieldSlammed;

    MMF_Player MMFplayer;

    bool slammed;

    private void Awake() {
        Instance = this;
        MMFplayer = GetComponentInChildren<MMF_Player>();
    }

    [Button]
    private void SlamBattlefields() {
        battlefield1Animator.SetTrigger("Slam");
        battlefield2Animator.SetTrigger("Slam");
    }

    [Button]
    private void SplitBattlefields() {
        battlefield1Animator.SetTrigger("Split");
        battlefield2Animator.SetTrigger("Split");
    }

    private void Update() {
        if (battlefield1Animator.GetCurrentAnimatorStateInfo(0).IsName("Battlefield_Slam") & !slammed & battlefield1Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
            slammed = true;
            OnBattlefieldSlammed?.Invoke(this, EventArgs.Empty);
            MMFplayer.PlayFeedbacks();
            foreach (ParticleSystem particleSystem in particleSystems) {
                particleSystem.Play();
            }
        }


        if (Input.GetKeyUp(KeyCode.T)) {
            SlamBattlefields();
        }
    }
}
