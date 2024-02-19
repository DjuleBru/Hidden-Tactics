using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlefieldAnimatorManager : MonoBehaviour {

    public static BattlefieldAnimatorManager Instance;

    Animator battlefield1Animator;
    bool slammed;

    private void Start() {
        battlefield1Animator = GetComponent<Animator>();
    }
    private void Awake() {
        Instance = this;
    }

    public void SlamBattlefields() {
        battlefield1Animator.SetTrigger("Slam");
    }

    public void SplitBattlefields() {
        battlefield1Animator.SetTrigger("Split");
    }

}
