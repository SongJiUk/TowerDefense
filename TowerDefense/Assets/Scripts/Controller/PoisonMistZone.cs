using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// PoisonMist 스킬로 소환되는 독가스 존.
/// Init() 호출 시 매 초마다 범위 내 적에게 독 DoT를 부여하고,
/// duration 이후 자동으로 풀로 반환된다.
/// </summary>
public class PoisonMistZone : MonoBehaviour
{
    private CancellationTokenSource _cts;

    public void Init(float radius, float hpRatio, float duration)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        RunAsync(radius, hpRatio, duration, _cts.Token).Forget();
    }

    void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async UniTaskVoid RunAsync(float radius, float hpRatio, float duration, CancellationToken token)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            ApplyPoison(radius, hpRatio);
            bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token)
                .SuppressCancellationThrow();
            if (cancelled) return;
            elapsed += 1f;
        }
        Managers.ResourceM.Destroy(gameObject);
    }

    private void ApplyPoison(float radius, float hpRatio)
    {
        var hits = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            var damageable = hit.GetComponent<IDamageable>();
            var buffHandler = hit.GetComponent<BuffHandler>();
            if (damageable == null || buffHandler == null) continue;
            buffHandler.AddEffect(new PoisonEffect(damageable, hpRatio, 1.5f));
        }
    }
}
