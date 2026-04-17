/// <summary>
/// 버프/디버프 하나의 계약.
/// BuffHandler가 이 인터페이스만 알고 관리한다.
/// </summary>
public interface IBuff
{
    void OnApply(BuffHandler handler);
    void OnRemove(BuffHandler handler);
    void Tick(float deltaTime);
    bool IsExpired { get; }
}
