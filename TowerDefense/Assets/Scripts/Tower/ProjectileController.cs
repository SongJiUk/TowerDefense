using UnityEngine;

/// <summary>
/// 발사된 투사체 하나를 담당. TowerController.Fire()에서 Init()으로 초기화된다.
/// 매 프레임 타겟을 향해 이동하다가 가까워지면 데미지를 주고 풀에 반환된다.
/// ObjectPool로 재사용되므로 Init() 호출마다 상태가 초기화된다.
/// </summary>
public class ProjectileController : MonoBehaviour
{
    private const float HIT_DISTANCE = 0.3f;
    private const float AIM_HEIGHT_OFFSET = 0.8f;  // 적 루트가 발 위치라서 중심부로 보정
    private Transform _target;
    private float _damage;
    private float _speed;
    private bool _isCritical;
    private IDamageable _IDamage;
    private BuffHandler _buffHandler;
    private BuffEffect _onHitEffect;

    private System.Action<Transform> _onHit;

    public void Init(Transform target, float damage, float speed, bool isCritical = false, BuffEffect onHitEffect = null, System.Action<Transform> onHit = null)
    {
        _target = target;
        _damage = damage;
        _speed = speed;
        _isCritical = isCritical;
        _IDamage = _target.GetComponent<IDamageable>();
        _buffHandler = _target.GetComponent<BuffHandler>();
        _onHitEffect = onHitEffect;
        _onHit = onHit;
    }

    void Update()
    {
        // 타겟이 사망해서 null이 되면 그냥 풀 반환
        if (_target == null || !_target.gameObject.activeInHierarchy)
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
            _IDamage?.TakeDamage(_damage, _isCritical);
            if (_onHitEffect != null) _buffHandler?.AddEffect(_onHitEffect);
            _onHit?.Invoke(_target);
            Managers.ResourceM.Destroy(gameObject);
        }
    }
}
