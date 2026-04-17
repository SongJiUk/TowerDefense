using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonEffect : BuffEffect
{
    private readonly float _hpRatio;
    private IDamageable _target;
    public override Type EffectType => typeof(PoisonEffect);
    private float _tickTimer = 0f;

    public PoisonEffect(IDamageable target, float hpRatio, float duration)
    {
        _target = target;
        _hpRatio = hpRatio;
        Duration = duration;
        AllowStack = false;
    }

    public override void OnApply(BuffHandler handler) { }

    public override void OnRemove(BuffHandler handler) { }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        _tickTimer += deltaTime;
        if (_tickTimer >= 1f)
        {
            _tickTimer -= 1f;
            float damage = _target.CurrentHp * _hpRatio;

            _target.TakeDamage(damage);
            Debug.Log($"[Poison] target : {damage:F1} 데미지 / 남은 HP: {_target.CurrentHp:F1}");
        }
    }
}
