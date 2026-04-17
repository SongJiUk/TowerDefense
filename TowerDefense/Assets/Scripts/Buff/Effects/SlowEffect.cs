using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowEffect : BuffEffect
{
    private readonly float _speedMultiplier;
    private StatModifier _modifier;
    public override System.Type EffectType => typeof(SlowEffect);

    public SlowEffect(float speedMultiplier, float duration)
    {
        _speedMultiplier = speedMultiplier;
        Duration = duration;
        AllowStack = false;
    }

    public override void OnApply(BuffHandler handler)
    {
        _modifier = new StatModifier(Define.StatType.Speed, _speedMultiplier, Define.ModifierType.Percent);
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
