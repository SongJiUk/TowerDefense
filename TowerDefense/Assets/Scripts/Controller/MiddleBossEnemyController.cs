using DG.Tweening;
using UnityEngine;

/// <summary>
/// 중간 보스 적. 사망 시 카메라 약한 진동 연출.
/// </summary>
public class MiddleBossEnemyController : EnemyController
{
    protected override void Die()
    {
        Camera.main?.transform.DOShakePosition(0.3f, 0.15f, 15, 90, false).SetUpdate(true);
        base.Die();
    }
}
