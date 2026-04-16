using UnityEngine;

/// <summary>
/// 플레이어가 지켜야 할 코어 오브젝트.
/// OnEnable에서 Managers.Core에 자신을 등록해 EnemyController·PathFinder가 참조할 수 있게 한다.
///
/// 주의: 반드시 Road 타일 위에 배치.
/// PathFinder.FindPath()의 endPos가 Road 노드가 아니면 null을 반환해 적이 출발하지 않는다.
///
/// TODO: HP 시스템 추가 예정 (Phase 4)
/// </summary>
public class Core : MonoBehaviour, IDamageable
{

    private float maxHp = 10f;
    private float currentHp = 10f;


    private void Awake()
    {
        Managers.ICore = this;
    }

    private void OnEnable() => Managers.CoreTransform = this.transform;
    private void OnDisable() => Managers.CoreTransform = null;

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log(currentHp);
        if (currentHp <= 0f)
        {
            
            currentHp = 0f;
            Die();
        }
    }

    private void Die()
    {

    }
}
