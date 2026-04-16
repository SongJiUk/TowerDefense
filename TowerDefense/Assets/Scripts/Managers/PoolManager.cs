using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 프리팹 이름을 키로 Pool을 관리하는 내부 클래스.
/// PoolManager.Pop() 호출 시 해당 프리팹의 Pool이 없으면 자동 생성.
/// </summary>
class Pool
{
    GameObject prefab;
    IObjectPool<GameObject> pool;
    Transform root;

    /// <summary>{prefab이름}Root GameObject를 자동 생성해 풀 오브젝트를 정리.</summary>
    Transform Root
    {
        get
        {
            if (root == null)
            {
                GameObject go = new GameObject() { name = $"{prefab.name}Root" };
                root = go.transform;
            }
            return root;
        }
    }

    public Pool(GameObject _prefab)
    {
        prefab = _prefab;
        pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
    }

    /// <summary>풀에서 오브젝트를 꺼낸다. 없으면 OnCreate로 새로 생성.</summary>
    public GameObject Pop() => pool.Get();


    /// <summary>오브젝트를 풀에 반환한다. SetActive(false) 처리됨.</summary>
    public void Push(GameObject _go) => pool.Release(_go);

    GameObject OnCreate()
    {
        GameObject go = GameObject.Instantiate(prefab);
        go.name = prefab.name;
        go.transform.SetParent(Root);
        return go;
    }

    void OnGet(GameObject _go)
    {
        if (_go == null) return;
        _go.SetActive(true);
    }

    void OnRelease(GameObject _go) => _go.SetActive(false);

    void OnDestroy(GameObject _go) => GameObject.Destroy(_go);

    /// <summary>Root GameObject와 모든 풀 오브젝트를 제거.</summary>
    public void DestroyPool()
    {
        if (root != null)
        {
            GameObject.Destroy(root.gameObject);
            root = null;
        }
        pool.Clear();
    }
}

/// <summary>
/// Unity ObjectPool 기반 오브젝트 풀 관리자.
/// 적·투사체처럼 반복 생성/제거되는 오브젝트를 Destroy 없이 재사용해 GC 부하를 제거한다.
/// Managers.PoolM으로 접근. 직접 사용보다 Managers.ResourceM.Destroy()를 통해 사용 권장.
/// </summary>
public class PoolManager
{
    Dictionary<string, Pool> pools = new Dictionary<string, Pool>();

    /// <summary>
    /// 풀에서 오브젝트를 꺼낸다. 해당 프리팹의 풀이 없으면 자동 생성.
    /// prefab이 null이면 null 반환.
    /// </summary>
    public GameObject Pop(GameObject _prefab)
    {
        if (_prefab == null) return null;

        if (!pools.TryGetValue(_prefab.name, out var pool))
        {
            CreatePool(_prefab);
            pool = pools[_prefab.name];
        }

        return pool.Pop();
    }
    /// <summary>
    /// Addressable로 사용해서 pop, 없으면 자동 생성
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public GameObject Pop(string key)
    {
        GameObject prefab = Managers.ResourceM.Load<GameObject>(key);
        if (prefab == null)
        {
            Debug.LogError($"[PoolManager] 로드된 프리팹이 없음 : {key}");
            return null;
        }

        return Pop(prefab);
    }

    /// <summary>
    /// 오브젝트를 풀에 반환한다.
    /// 해당 이름의 풀이 없으면 false 반환 (풀 미등록 오브젝트는 Destroy 필요).
    /// </summary>
    public bool Push(GameObject _go)
    {
        if (_go == null) return false;

        if (!pools.TryGetValue(_go.name, out var pool)) return false;

        pool.Push(_go);
        return true;
    }

    /// <summary>특정 프리팹의 풀을 미리 생성한다. Pop 호출 시 자동 생성되므로 선택적.</summary>
    public void CreatePool(GameObject _prefab)
    {
        Pool pool = new Pool(_prefab);
        pools.Add(_prefab.name, pool);
    }

    /// <summary>모든 풀과 풀 오브젝트를 제거. 씬 전환 시 Managers.Clear()에서 호출.</summary>
    public void Clear()
    {
        foreach (var pool in pools.Values)
            pool.DestroyPool();

        pools.Clear();
    }
}
