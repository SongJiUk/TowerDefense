using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, bool isCritical = false, bool isPoison = false);
    void Heal(float amount);
    float CurrentHp { get; }
}
