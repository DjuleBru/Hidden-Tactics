using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DwarfWallBlade : NetworkBehaviour, IDamageSource
{
    private List<Collider2D> collidersInAttackAOE = new List<Collider2D>();

    private Building parentBuilding;
    private bool attackTriggered;
    private Animator bladeAnimator;

    [SerializeField] private float damageDealt;
    [SerializeField] private float timeToTriggerBladeAfterUnitCollision;
    [SerializeField] private float timeToDamageUnitsAfterBladeAnimationTrigger;

    [SerializeField] private SpriteRenderer bladeSpriteRenderer;
    [SerializeField] private Sprite bladeTriggeredSprite;

    private void Awake() {
        parentBuilding = GetComponentInParent<Building>();
        bladeAnimator = GetComponent<Animator>();
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if(BattleManager.Instance.IsBattlePhase()) {
            attackTriggered = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D hitCollider) {
        if (!IsServer) return;
        if (!BattleManager.Instance.IsBattlePhase()) return;

        //if the object is not already in the list
        if (!collidersInAttackAOE.Contains(hitCollider)) {
            //add the object to the list
            collidersInAttackAOE.Add(hitCollider);
        }

        if(!attackTriggered) {
            attackTriggered = true;
            StartCoroutine(TriggerBlade());
        }
    } 
    
    //called when something exits the trigger
    protected virtual void OnTriggerExit2D(Collider2D hitCollider) {
        if (!IsServer) return;
        if (!BattleManager.Instance.IsBattlePhase()) return;

        //if the object is in the list
        if (collidersInAttackAOE.Contains(hitCollider)) {
            //remove it from the list
            collidersInAttackAOE.Remove(hitCollider);
        }
    }

    private IEnumerator TriggerBlade() {
        bladeSpriteRenderer.sprite = bladeTriggeredSprite;
        yield return new WaitForSeconds(timeToTriggerBladeAfterUnitCollision);
        bladeAnimator.SetTrigger("Trigger");
        yield return new WaitForSeconds(timeToDamageUnitsAfterBladeAnimationTrigger);
        InflictDamage();
    }


    public List<Collider2D> GetCollidersInAttackAOEList() {
        return collidersInAttackAOE;
    }

    protected void InflictDamage() {
        // Attack AOE uses a specific collider
        List<Collider2D> collidersInAttackAOEList = GetCollidersInAttackAOEList();
        List<Unit> unitsInAttackAOE = FindTargetUnitsInColliderList(collidersInAttackAOEList);

        foreach (Unit unitAOETarget in unitsInAttackAOE) {
            // Don't damage target unit twice

            if (unitAOETarget == null) return;
            IDamageable targetIDamageable = unitAOETarget.GetIDamageable();
            if (unitAOETarget.GetIsDead()) return;
            targetIDamageable.TakeDamage(damageDealt, this);
        }
    }

    protected List<Unit> FindTargetUnitsInColliderList(List<Collider2D> colliderList, bool targetAllyUnits = false) {
        List<Unit> targetUnitList = new List<Unit>();

        foreach (Collider2D collider in colliderList) {
            if (collider.TryGetComponent<Unit>(out Unit unit)) {
                // Collider is a unit
                bool correctTeam = unit.GetParentTroop().IsOwnedByPlayer() != parentBuilding.IsOwnedByPlayer();

                if (targetAllyUnits) {
                    correctTeam = unit.GetParentTroop().IsOwnedByPlayer() == parentBuilding.IsOwnedByPlayer();
                }

                if (correctTeam && !unit.GetIsDead()) {
                    // target unit is not from the same team AND Unit is not dead
                    targetUnitList.Add(unit);
                }
            };
        }
        return targetUnitList;
    }

}
