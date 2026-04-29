using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonEffect : BuffEffect
{
    private readonly float _hpRatio;
    private IDamageable _target;
    public override Type EffectType => typeof(PoisonEffect);
    private float _tickTimer = 0.5f; // 첫 틱: 적중 0.5초 뒤

    public PoisonEffect(IDamageable target, float hpRatio, float duration)
    {
        _target = target;
        _hpRatio = hpRatio;
        Duration = duration;
        AllowStack = false;
    }

    public override void OnApply(BuffHandler handler) { }

    public override void OnRemove(BuffHandler handler) { }

    public override void Refresh()
    {
        base.Refresh();
        _tickTimer = 0.5f; // 재적중 시 0.5초 딜레이로 리셋
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        _tickTimer += deltaTime;
        if (_tickTimer >= 1f)
        {
            _tickTimer -= 1f;
            float damage = Mathf.Max(1f, _target.CurrentHp * _hpRatio);

            _target.TakeDamage(damage, false, isPoison: true);
            if (_target is MonoBehaviour mb)
                Managers.FloatingTextM?.ShowPoison(mb.transform.position, damage);
        }
    }
}
