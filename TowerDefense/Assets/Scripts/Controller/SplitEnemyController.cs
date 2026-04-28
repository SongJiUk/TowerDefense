using UnityEngine;

/// <summary>
/// 사망 시 작은 적으로 분열하는 적.
/// WaveManager.PreloadEnemyData()로 캐시된 EnemyData를 동기 접근한다.
/// </summary>
public class SplitEnemyController : EnemyController
{
    private SplitEnemyData _splitData;

    public override void Init(EnemyData data, float hpMultiplier = 1f, float speedMultiplier = 1f)
    {
        _splitData = data as SplitEnemyData;
        base.Init(data, hpMultiplier, speedMultiplier);
    }

    protected override void Die()
    {
        if (_splitData != null)
        {
            var afterData = Managers.ResourceM.Load<EnemyData>(_splitData.afterSplitKey);
            if (afterData != null)
            {
                Managers.WaveM.RegisterExtraEnemy(_splitData.splitCount);
                for (int i = 0; i < _splitData.splitCount; i++)
                    Managers.WaveM.SpawnEnemyAt(
                        afterData,
                        transform.position,
                        _storedHpMult * _splitData.splitHpRatio,
                        _storedSpeedMult
                    );
            }
        }
        base.Die();
    }
}
