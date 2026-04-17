using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonTowerController : TowerController
{
    private CannonTowerData _cannonTowerData;

    protected override void OnHit(Transform target)
    {
        Collider[] cols = Physics.OverlapSphere(
        target.position,
        _cannonTowerData.splashRadius,
        _enemyMask);

        foreach (var col in cols)
        {
            if (col.transform == target) continue;

            col.GetComponent<IDamageable>()?.TakeDamage(_currentDamage * 0.4f);
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
