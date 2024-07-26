using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitAI_Jump : UnitAI
{
    private UnitBuffManager unitBuffManager;
    [SerializeField] private UnitVisual unitVisual;
    [SerializeField] private float jumpAnimationTime;
    [SerializeField] private GameObject jumpLandExplosion;
    [SerializeField] private float jumpForce;

    private float jumpTimer;
    private bool jumping;
    [SerializeField] private bool jumpTargetsEnemy;

    protected override void Awake() {
        base.Awake();
        unitBuffManager = GetComponent<UnitBuffManager>();
        specialActive = true;
    }

    protected void Start() {
        unitAttack.SetActiveAttackSO(unit.GetUnitSO().specialAttackSO);
    }

    protected void Update() {
        if(jumping) {
            jumpTimer -= Time.deltaTime;
            if(jumpTimer < 0) {
                Land();
            }
        }
    }

    [ClientRpc]
    protected override void ChangeStateClientRpc() {

        if (state.Value == State.idle) {
            specialActive = true;
            unitAttack.SetActiveAttackSO(unit.GetUnitSO().specialAttackSO);
        }

        if (state.Value == State.attackingMelee) {
            specialActive = false;
            unitAttack.SetActiveAttackSO(unit.GetUnitSO().mainAttackSO);
        }

        if (state.Value == State.moveToMeleeTarget) {
            specialActive = false;
            unitAttack.SetActiveAttackSO(unit.GetUnitSO().mainAttackSO);
        }

        if (state.Value == State.moveForwards) {
        }

        if (state.Value == State.jumping) {
        }

        if (state.Value == State.dead) {

        }

        base.ChangeStateClientRpc();
    }

    public void Jump(Vector3 jumpDir) {
        Vector3 jumpForceVector3 = new Vector3();

        if (jumpTargetsEnemy) {
            jumpForceVector3 = jumpDir * jumpForce;
        } else {
            jumpForceVector3 = new Vector3(jumpForce, 0, 0);
        }

        unit.TakeDazed(jumpAnimationTime);
        unit.TakeKnockBack(jumpForceVector3);

        unitAttack.SetActiveAttackSO(unit.GetUnitSO().specialAttackSO);
        unitVisual.EnableTrailRenderer();
        unit.DisableCollider();
        jumpTimer = jumpAnimationTime;
        jumping = true;
    }

    private void Land() {
        unit.EnableCollider();

        unitAttack.UnitHasLanded(transform.position, unitTargetingSystem.GetSpecialAttackTarget());
        StartCoroutine(PostLandingCoroutine());

        if(unitAttack.GetActiveAttackSO().attackHasAOE) { 
            SpawnProjectileHitVisualGameObjectServerRpc();
        }

        ChangeState(State.moveToMeleeTarget);
        jumping = false;
    }

    private IEnumerator PostLandingCoroutine() {
        yield return new WaitForSeconds(.3f);
        unitVisual.DisableTrailRenderer();
    }

    [ServerRpc(RequireOwnership = false)]
    protected void SpawnProjectileHitVisualGameObjectServerRpc() {

        GameObject projectileHitVisualGameObject = Instantiate(jumpLandExplosion);
        NetworkObject projectileHitVisualGameObjectNetworkObject = projectileHitVisualGameObject.GetComponent<NetworkObject>();

        projectileHitVisualGameObjectNetworkObject.Spawn(true);

        SetProjectileHitVisualGameObjectPositionClientRpc(projectileHitVisualGameObjectNetworkObject);
    }


    [ClientRpc]
    protected void SetProjectileHitVisualGameObjectPositionClientRpc(NetworkObjectReference projectileHitVisualGameObjectNetworkObjectReference) {
        projectileHitVisualGameObjectNetworkObjectReference.TryGet(out NetworkObject projectileHitVisualGameObjectNetworkObject);
        ProjectileHitVisual projectileHitVisual = projectileHitVisualGameObjectNetworkObject.GetComponent<ProjectileHitVisual>();

        projectileHitVisual.Initialize(transform.position);
    }
}
