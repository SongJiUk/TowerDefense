using UnityEngine;

public class PoisonTowerController : TowerController
{
    private PoisonTowerData _poisonTowerData;

    public override void Init(TowerData data)
    {
        base.Init(data);
        _poisonTowerData = data as PoisonTowerData;
    }

    protected override Transform FindTarget()
    {
        Vector3 bottom = new Vector3(transform.position.x, -50f, transform.position.z);
        Vector3 top    = new Vector3(transform.position.x,  50f, transform.position.z);
        Collider[] hits = Physics.OverlapCapsule(bottom, top, CurrentRange, _enemyMask, QueryTriggerInteraction.Collide);
        if (hits.Length == 0) return null;

        Vector3 corePos = Managers.CoreTransform != null ? Managers.CoreTransform.position : Vector3.zero;

        Transform bestUnpoisoned = null;
        float minDistUnpoisoned = float.MaxValue;
        Transform bestFallback  = null;
        float minDistFallback   = float.MaxValue;

        foreach (Collider col in hits)
        {
            bool hasPoison = col.GetComponent<BuffHandler>()?.HasEffect<PoisonEffect>() ?? false;
            float dist = Vector3.Distance(col.transform.position, corePos);

            if (!hasPoison)
            {
                if (dist < minDistUnpoisoned)
                {
                    minDistUnpoisoned = dist;
                    bestUnpoisoned = col.transform;
                }
            }
            else
            {
                if (dist < minDistFallback)
                {
                    minDistFallback = dist;
                    bestFallback = col.transform;
                }
            }
        }

        return bestUnpoisoned != null ? bestUnpoisoned : bestFallback;
    }

    public override string GetUniqueEffectText()
    {
        if (_poisonTowerData == null) return "";
        int stage    = UniqueEffectStage;
        float dps    = _poisonTowerData.poisonDps * _poisonTowerData.stageDpsMultiplier[stage];
        float dur    = _poisonTowerData.poisonDuration + _poisonTowerData.stageDurationBonus[stage];
        if (stage < 3)
        {
            float nextDps = _poisonTowerData.poisonDps * _poisonTowerData.stageDpsMultiplier[stage + 1];
            float nextDur = _poisonTowerData.poisonDuration + _poisonTowerData.stageDurationBonus[stage + 1];
            return $"독 {dps:F1}dmg/s | {dur:F1}초  ->  {nextDps:F1}dmg/s | {nextDur:F1}초";
        }
        return $"독 {dps:F1}dmg/s | {dur:F1}초  (최대)";
    }

    protected override void OnHit(Transform target)
    {
        int stage    = UniqueEffectStage;
        float dps      = _poisonTowerData.poisonDps * _poisonTowerData.stageDpsMultiplier[stage];
        float duration = _poisonTowerData.poisonDuration + _poisonTowerData.stageDurationBonus[stage];
        var damageable = target.GetComponent<IDamageable>();
        target.GetComponent<BuffHandler>()?.AddEffect(new PoisonEffect(damageable, dps, duration));
    }
}
