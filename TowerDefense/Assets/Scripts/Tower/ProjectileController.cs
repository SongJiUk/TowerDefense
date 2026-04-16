using UnityEngine;

/// <summary>
/// 발사된 투사체 하나를 담당. TowerController.Fire()에서 Init()으로 초기화된다.
/// 매 프레임 타겟을 향해 이동하다가 가까워지면 데미지를 주고 풀에 반환된다.
/// ObjectPool로 재사용되므로 Init() 호출마다 상태가 초기화된다.
/// </summary>
public class ProjectileController : MonoBehaviour
{
    private Transform _target;
    private float _damage;
    private float _speed;
    private IDamageable _IDamage;
    private const float HIT_DISTANCE = 0.3f;
    private const float AIM_HEIGHT_OFFSET = 0.8f;  // 적 루트가 발 위치라서 중심부로 보정

    /// <summary>
    /// 발사 직후 TowerController가 호출. 타겟·데미지·이동속도를 설정한다.
    /// </summary>
    /// <param name="target">추적할 적의 Transform</param>
    /// <param name="damage">명중 시 입힐 데미지</param>
    /// <param name="speed">이동 속도 (유닛/초)</param>
    public void Init(Transform target, float damage, float speed)
    {
        _target = target;
        _damage = damage;
        _speed = speed;
        _IDamage = _target.GetComponent<IDamageable>();
    }

    void Update()
    {
        // 타겟이 사망해서 null이 되면 그냥 풀 반환
        if (_target == null)
        {
            _target = null;
            _IDamage = null;
            Managers.ResourceM.Destroy(gameObject);
            return;
        }

        Vector3 aimPos = _target.position + Vector3.up * AIM_HEIGHT_OFFSET;
        Vector3 dir = (aimPos - transform.position).normalized;
        transform.position += dir * _speed * Time.deltaTime;
        transform.forward = dir;

        // HIT_DISTANCE 이내로 진입하면 데미지 후 풀 반환
        if (Vector3.Distance(transform.position, aimPos) < HIT_DISTANCE)
        {
            _IDamage?.TakeDamage(_damage);
            Managers.ResourceM.Destroy(gameObject);
        }
    }
}
