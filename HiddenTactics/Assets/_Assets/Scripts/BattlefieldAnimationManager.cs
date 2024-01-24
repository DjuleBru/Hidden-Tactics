using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlefieldAnimationManager : MonoBehaviour
{
    [SerializeField] Animator battlefield1Animator;
    [SerializeField] Animator battlefield2Animator;

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
}
