public abstract class BuffEffect : IBuff
{

    protected float _elapsed;
    public float Duration { get; protected set; }
    public bool AllowStack { get; protected set; }

    public bool IsExpired => _elapsed >= Duration;
    public abstract System.Type EffectType { get; }

    public abstract void OnApply(BuffHandler handler);

    public abstract void OnRemove(BuffHandler handler);

    public virtual void Tick(float deltaTime)
    {
        _elapsed += deltaTime;
    }

    /// <summary>재적중 시 지속시간 초기화. 필요한 효과에서 override.</summary>
    public virtual void Refresh() => _elapsed = 0f;

}
