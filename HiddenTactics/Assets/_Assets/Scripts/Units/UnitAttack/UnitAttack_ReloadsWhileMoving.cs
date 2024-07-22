using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAttack_ReloadsWhileMoving : UnitAttack
{
    protected override void Update() {
        base.Update();
        attackTimer -= Time.deltaTime;
    }
}
