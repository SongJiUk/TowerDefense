using DG.Tweening;
using UnityEngine;

/// <summary>
/// 최종 보스 적.
/// - HP 50% 이하 진입 시 이동속도 증가 (격노)
/// - 사망 시 강한 카메라 진동 연출
/// </summary>
public class BossEnemyController : EnemyController
{
    [SerializeField] private float _enrageSpeedMultiplier = 1.4f;

    private bool _isEnraged;

    public override void Init(EnemyData data, float hpMultiplier = 1f, float speedMultiplier = 1f)
    {
        _isEnraged = false;
        base.Init(data, hpMultiplier, speedMultiplier);
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (!_isEnraged && !_isDead && _hp <= _maxHp * 0.5f)
            Enrage();
    }

    protected override void Die()
    {
        Camera.main?.transform.DOShakePosition(0.6f, 0.4f, 25, 90, false).SetUpdate(true);
        base.Die();
    }

    private void Enrage()
    {
        _isEnraged = true;
        _baseSpeed *= _enrageSpeedMultiplier;
        RecalculateSpeed();
    }
}
