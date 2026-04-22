using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 오브젝트 생성·제거 담당.
/// ResourceManager — 에셋 로드
/// PoolManager     — 오브젝트 재사용
/// ObjectManager   — 실질적인 생성/파괴 결정
/// </summary>
public class ObjectManager
{
    // ─── 활성 오브젝트 목록 ───────────────────────────────────────────────────

    public List<EnemyController> EnemyList { get; } = new List<EnemyController>();

    // ─── 월드 오브젝트 ────────────────────────────────────────────────────────

    /// <summary>
    /// 풀에서 꺼내 위치 설정 후 컴포넌트 반환.
    /// EnemyController, ProjectileController 등 풀링 대상에 사용.
    /// </summary>
    public T Spawn<T>(string addressableKey, Vector3 position) where T : MonoBehaviour
    {
        GameObject go = Managers.PoolM.Pop(addressableKey);
        if (go == null)
        {
            Debug.LogError($"[ObjectManager] Spawn 실패: {addressableKey}");
            return null;
        }

        go.transform.position = position;

        T component = go.GetComponent<T>();
        if (component == null)
            Debug.LogError($"[ObjectManager] {typeof(T).Name} 컴포넌트 없음: {addressableKey}");

        return component;
    }

    /// <summary>
    /// 풀에 반환 또는 파괴.
    /// </summary>
    public void Despawn(GameObject go)
    {
        if (go == null) return;
        Managers.ResourceM.Destroy(go);
    }

    // ─── UI ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// 풀에서 UI 오브젝트를 꺼내 parent 아래 배치 후 컴포넌트 반환.
    /// </summary>
    public T SpawnUI<T>(string addressableKey, Transform parent = null) where T : MonoBehaviour
    {
        GameObject go = Managers.PoolM.Pop(addressableKey);
        if (go == null)
        {
            Debug.LogError($"[ObjectManager] SpawnUI 실패: {addressableKey}");
            return null;
        }

        if (parent != null)
            go.transform.SetParent(parent, false);

        T component = go.GetComponent<T>();
        if (component == null)
            Debug.LogError($"[ObjectManager] {typeof(T).Name} 컴포넌트 없음: {addressableKey}");

        return component;
    }

    /// <summary>
    /// UI 오브젝트를 풀에 반환. 풀 미등록 시 Destroy.
    /// </summary>
    public void DespawnUI(GameObject go)
    {
        if (go == null) return;
        Managers.ResourceM.Destroy(go);
    }

    // ─── 씬 전환 초기화 ───────────────────────────────────────────────────────

    public void Clear()
    {
        foreach (var enemy in EnemyList.ToList())
        {
            if (enemy != null)
                Managers.ResourceM.Destroy(enemy.gameObject);
        }
        EnemyList.Clear();
    }
}
