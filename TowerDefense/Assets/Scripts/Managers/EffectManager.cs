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

    private async UniTaskVoid ReturnAsync(GameObject go, float duration)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
        if (go != null && go.activeSelf)
            Managers.PoolM.Push(go);
    }
}
