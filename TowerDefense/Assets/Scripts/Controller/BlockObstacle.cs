using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Block 스킬로 설치된 장애물.
/// Init() 호출 시 duration 이후 자동으로 그리드 점유를 해제하고 풀로 반환된다.
/// </summary>
public class BlockObstacle : MonoBehaviour
{
    private CancellationTokenSource _cts;

    public void Init(float duration)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        RemoveAfterDelay(duration, _cts.Token).Forget();
    }

    void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async UniTaskVoid RemoveAfterDelay(float duration, CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
        Managers.Grid.SetOccupied(transform.position, false);
        Managers.ResourceM.Destroy(gameObject);
    }
}
