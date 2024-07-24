using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable {

    public void TakeDamage(float damage);

    public void Heal(float healAmount);

    public float GetMaxHP();
    public float GetHP();
}
