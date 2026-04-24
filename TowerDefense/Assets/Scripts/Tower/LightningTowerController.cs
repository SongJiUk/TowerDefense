using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTowerController : TowerController
{
    private LightningTowerData _lightningTowerData;
    private readonly List<Transform> _chainTargets = new List<Transform>();

    public override void Init(TowerData data)
    {
        base.Init(data);
        _lightningTowerData = data as LightningTowerData;
    }

    public override string GetUniqueEffectText()
    {
        if (_lightningTowerData == null) return "";
        int stage  = UniqueEffectStage;
        int chains = _lightningTowerData.chainCount + _lightningTowerData.stageChainCountBonus[stage];
        if (stage < 3)
        {
            int nextChains = _lightningTowerData.chainCount + _lightningTowerData.stageChainCountBonus[stage + 1];
            return $"체인 {chains}회  ->  {nextChains}회";
        }
        return $"체인 {chains}회  (최대)";
    }

    protected override void OnHit(Transform target)
    {
        _chainTargets.Clear();
        _chainTargets.Add(target);


        int totalChains = _lightningTowerData.chainCount + _lightningTowerData.stageChainCountBonus[UniqueEffectStage];
        for (int i = 1; i <= totalChains; i++)
        {
            Transform last = _chainTargets[_chainTargets.Count - 1];
            Transform next = FindChainTarget(last);
            if (next == null) break;

            float damage = CurrentDamage * Mathf.Pow(_lightningTowerData.chainDamageFalloff, i);
            next.GetComponent<IDamageable>()?.TakeDamage(damage);
            _chainTargets.Add(next);
        }
    }

    private Transform FindChainTarget(Transform from)
    {
        Collider[] cols = Physics.OverlapSphere(from.position, _lightningTowerData.chainRange, _enemyMask);
        foreach (var col in cols)
        {
            if (!_chainTargets.Contains(col.transform))
            {
                return col.transform;
            }

        }
        return null;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // 기본 사거리 표시
        if (_lightningTowerData == null) return;

        // 체인 연결선 (플레이 중에만 보임)
        Gizmos.color = Color.cyan;
        for (int i = 0; i < _chainTargets.Count - 1; i++)
        {
            if (_chainTargets[i] == null || _chainTargets[i + 1] == null) continue;
            Gizmos.DrawLine(_chainTargets[i].position, _chainTargets[i + 1].position);
        }

        // 마지막 체인 타겟 기준 탐색 반경
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        if (_chainTargets.Count > 0 && _chainTargets[0] != null)
            Gizmos.DrawSphere(_chainTargets[0].position, _lightningTowerData.chainRange);
    }
#endif

}
