using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable {
    public enum TargetType {
        groundUnit,
        airUnit,
        building,
        village,
    }

    public bool IsOwnedByPlayer();
    public bool GetIsDead();

    public GridPosition GetCurrentGridPosition();

    public TargetType GetTargetType();

    public IDamageable GetIDamageable();

    public Transform GetProjectileTarget();
}
