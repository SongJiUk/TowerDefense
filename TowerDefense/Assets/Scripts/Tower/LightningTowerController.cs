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
        int chains = _lightningTowerData.chainCount + _lightningTowerData.stageChainCountBonus[UniqueEffectStage];
        return $"최대 {chains}마리 연쇄 공격";
    }

    protected override void OnHit(Transform target)
    {
        _chainTargets.Clear();
        _chainTargets.Add(target);

        int totalChains = _lightningTowerData.chainCount + _lightningTowerData.stageChainCountBonus[UniqueEffectStage];

        if (Managers.SynergyM != null && Managers.SynergyM.Conductor)
        {
            var buff = target.GetComponent<BuffHandler>();
            if (buff != null && buff.HasEffect<SlowEffect>())
            {
                totalChains += 1;
                Debug.Log($"[Synergy:도체] 슬로우 적 명중 → 체인 +1 (총 {totalChains})");
            }
        }

        bool conductivePoison = Managers.SynergyM != null && Managers.SynergyM.ConductivePoison;

        for (int i = 1; i <= totalChains; i++)
        {
            Transform last = _chainTargets[_chainTargets.Count - 1];
            Transform next = FindChainTarget(last);
            if (next == null) break;

            float damage = CurrentDamage * Mathf.Pow(_lightningTowerData.chainDamageFalloff, i);

            if (conductivePoison)
            {
                var buff = next.GetComponent<BuffHandler>();
                if (buff != null && buff.HasEffect<PoisonEffect>())
                {
                    float mult = 1f + 0.5f * (Managers.GameM?.synergyMultiplier ?? 1f);
                    damage *= mult;
                    Debug.Log($"[Synergy:전도성독] 체인 {i}번째 독 적 → 데미지 {mult:F2}배 ({damage:F1})");
                }
            }

            bool isCritical = UnityEngine.Random.value < Managers.GameM.criticalChanceBonus + GetBonusCritChance(next);
            if (isCritical) damage *= 2f;
            next.GetComponent<IDamageable>()?.TakeDamage(damage, isCritical);
            _chainTargets.Add(next);
        }
    }

    private Transform FindChainTarget(Transform from)
    {
        Collider[] cols = Physics.OverlapSphere(from.position, _lightningTowerData.chainRange, _enemyMask);
        foreach (var col in cols)
        {
            if (_chainTargets.Contains(col.transform)) continue;
            if (col.GetComponent<EnemyController>()?.IsDead == true) continue;
            return col.transform;
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
