using Cysharp.Threading.Tasks;
using UnityEngine;

public class EffectManager
{
    public void Play(string key, Vector3 position, float duration = 2f)
    {
        if (string.IsNullOrEmpty(key)) return;
        GameObject go = Managers.PoolM.Pop(key);
        if (go == null) return;
        go.transform.position = position;
        ReturnAsync(go, duration).Forget();
    }

    // 시작점에서 목표 방향으로 향하는 방향성 이펙트
    public void PlayLine(string key, Vector3 from, Vector3 to, float duration = 0.3f)
    {
        if (string.IsNullOrEmpty(key)) return;
        GameObject go = Managers.PoolM.Pop(key);
        if (go == null) return;

        go.transform.position = from;
        go.transform.rotation = Quaternion.LookRotation(to - from);

        ReturnAsync(go, duration).Forget();
    }

    private async UniTaskVoid ReturnAsync(GameObject go, float duration)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
        if (go != null && go.activeSelf)
            Managers.PoolM.Push(go);
    }
}
