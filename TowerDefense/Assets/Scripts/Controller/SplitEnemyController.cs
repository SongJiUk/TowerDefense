using UnityEngine;

/// <summary>
/// 사망 시 작은 적 2마리로 분열하는 적.
/// Inspector에서 splitEnemyData에 분열 후 스폰할 EnemyData를 연결해야 한다.
/// </summary>
public class SplitEnemyController : EnemyController
{
    [SerializeField] private EnemyData _splitEnemyData;
    [SerializeField] private int _splitCount = 2;
    [SerializeField] private float _splitHpRatio = 0.6f;

    protected override void Die()
    {
        if (_splitEnemyData != null)
        {
            Managers.WaveM.RegisterExtraEnemy(_splitCount);
            for (int i = 0; i < _splitCount; i++)
                Managers.WaveM.SpawnEnemyAt(
                    _splitEnemyData,
                    transform.position,
                    _storedHpMult * _splitHpRatio,
                    _storedSpeedMult
                );
        }
        base.Die();
    }
}
