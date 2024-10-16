using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackColliderAOE : MonoBehaviour
{
    protected List<Collider2D> collidersInAttackAOE = new List<Collider2D>();


    protected virtual void OnTriggerEnter2D(Collider2D hitCollider) {
        //if the object is not already in the list
        if (!collidersInAttackAOE.Contains(hitCollider)) {
            //add the object to the list
            collidersInAttackAOE.Add(hitCollider);
        }
    }

    //called when something exits the trigger
    protected virtual void OnTriggerExit2D(Collider2D hitCollider) {
        //if the object is in the list
        if (collidersInAttackAOE.Contains(hitCollider)) {
            //remove it from the list
            collidersInAttackAOE.Remove(hitCollider);
        }
    }

    public List<Collider2D> GetCollidersInAttackAOEList() {
        return collidersInAttackAOE;
    }


}
