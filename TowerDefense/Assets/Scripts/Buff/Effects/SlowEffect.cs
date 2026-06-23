using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowEffect : BuffEffect
{
    private readonly float _speedMultiplier;
    private StatModifier _modifier;
    private GameObject _vfx;
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
        if (handler.TryGetComponent(out MonoBehaviour mb))
        {
            Managers.FloatingTextM?.ShowSlow(mb.transform.position);
            if (Managers.SettingsM.IsParticleOn)
            {
                _vfx = Managers.PoolM.Pop("FX_Buff_Slow");
                if (_vfx != null) _vfx.transform.SetParent(mb.transform, false);
            }
        }
    }

    public override void OnRemove(BuffHandler handler)
    {
        handler.RemoveModifier(_modifier);
        if (_vfx != null)
        {
            Managers.PoolM.Push(_vfx);
            _vfx = null;
        }
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
    }
}
