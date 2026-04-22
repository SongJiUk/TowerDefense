using UnityEngine;

public class FreezeEffect : BuffEffect
{
    private StatModifier _modifier;
    private Animator _animator;
    public override System.Type EffectType => typeof(FreezeEffect);

    public FreezeEffect(float duration)
    {
        Duration = duration;
        AllowStack = false;
    }

    public override void OnApply(BuffHandler handler)
    {
        _modifier = new StatModifier(Define.StatType.Speed, 0f, Define.ModifierType.Percent);
        handler.AddModifier(_modifier);

        _animator = handler.GetComponent<Animator>();
        if (_animator != null) _animator.speed = 0f;
    }

    public override void OnRemove(BuffHandler handler)
    {
        handler.RemoveModifier(_modifier);

        if (_animator != null) _animator.speed = 1f;
        _animator = null;
    }

    public override void Tick(float deltaTime) => base.Tick(deltaTime);
}
