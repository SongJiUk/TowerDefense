using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonTowerController : TowerController
{
    private CannonTowerData _cannonTowerData;

    public override string GetUniqueEffectText()
    {
        if (_cannonTowerData == null) return "";
        float radius = _cannonTowerData.splashRadius + _cannonTowerData.stageSplashBonus[UniqueEffectStage];
        return $"폭발 공격  반경 {radius:F1}";
    }

    protected override void OnHit(Transform target)
    {
        float radius = _cannonTowerData.splashRadius + _cannonTowerData.stageSplashBonus[UniqueEffectStage];

        // 정밀 포격: 슬로우 걸린 적 명중 시 스플래시 반경 1.5배
        if (Managers.SynergyM != null && Managers.SynergyM.PrecisionBombardment)
        {
            var buff = target.GetComponent<BuffHandler>();
            if (buff != null && buff.HasEffect<SlowEffect>())
            {
                float radiusMult = 1f + 0.5f * (Managers.GameM?.synergyMultiplier ?? 1f);
                radius *= radiusMult;
                Debug.Log($"[Synergy:정밀포격] 슬로우 적 명중 → 스플래시 반경 {radiusMult:F2}배 ({radius:F2})");
            }
        }

        Collider[] cols = Physics.OverlapSphere(target.position, radius, _enemyMask);
        foreach (var col in cols)
        {
            if (col.transform == target) continue;
            col.GetComponent<IDamageable>()?.TakeDamage(CurrentDamage * 0.4f);
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // 기본 사거리 표시
        if (_cannonTowerData == null) return;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // 주황 반투명
        Gizmos.DrawSphere(transform.position, _cannonTowerData.splashRadius);
    }
#endif

    public override void Init(TowerData data)
    {
        base.Init(data);
        _cannonTowerData = data as CannonTowerData;
    }
}
