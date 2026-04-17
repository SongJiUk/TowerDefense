using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBuff : BuffEffect
{
    private readonly float _attackMultiplier;
    private StatModifier _modifier;
    public override Type EffectType => typeof(AttackBuff);

    public AttackBuff(float attackMultiplier, float duration)
    {
        _attackMultiplier = attackMultiplier;
        Duration = duration;
        AllowStack = false;
    }
    public override void OnApply(BuffHandler handler)
    {
        _modifier = new StatModifier(Define.StatType.Damage, _attackMultiplier, Define.ModifierType.Percent);
        handler.AddModifier(_modifier);
    }

    public override void OnRemove(BuffHandler handler)
    {
        handler.RemoveModifier(_modifier);
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
    }
}
