using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 최초 사망 시 HP를 reviveHpRatio만큼 회복하여 1회 부활하는 적.
/// 부활 시 렌더러 색이 회색으로 바뀌어 부활 상태임을 표시한다.
/// </summary>
public class ReviveEnemyController : EnemyController
{
    private static readonly int HASH_REVIVE = Animator.StringToHash("IsRevive");

    private ReviveEnemyData _reviveData;
    private bool _hasRevived;
    private bool _isReviving;
    private Renderer[] _renderers;
    private UniTaskCompletionSource _reviveEndTcs;

    public override bool IsDead => _isDead || _isReviving;

    protected override void Awake()
    {
        base.Awake();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    public override void Init(EnemyData data, float hpMultiplier = 1f, float speedMultiplier = 1f)
    {
        _reviveData = data as ReviveEnemyData;
        _hasRevived = false;
        SetTint(Color.white);
        base.Init(data, hpMultiplier, speedMultiplier);
    }

    protected override async UniTask PlayDeathAnimationAsync(CancellationToken token)
    {
        await base.PlayDeathAnimationAsync(token); // Die 애니메이션 대기

        if (_hasRevived || _reviveData == null || _animator == null) return;

        // IsDie 끄기 → Any State → Death 재진입 방지
        _isReviving = true;
        _animator.SetBool(HASH_DIE, false);
        _animator.SetBool(HASH_REVIVE, true);

        _reviveEndTcs = new UniTaskCompletionSource();
        await _reviveEndTcs.Task.AttachExternalCancellation(token);

        _animator.SetBool(HASH_REVIVE, false);
        _isReviving = false;
    }

    protected override void OnDeathComplete()
    {
        if (!_hasRevived && _reviveData != null)
        {
            _hasRevived = true;
            _isDead = false;
            _hp = _maxHp * _reviveData.reviveHpRatio;
            _hpBar?.SetHP(_hp, _maxHp);
            SetTint(new Color(0.55f, 0.55f, 0.55f));
            if (_animator != null)
            {
                _animator.SetBool(HASH_DIE, false);
                _animator.SetBool(HASH_REVIVE, false);
            }
            RequestPath();
            return;
        }
        base.OnDeathComplete();
    }

    // Revive 애니메이션 클립 마지막 프레임에 Animation Event로 연결
    public void OnReviveAnimationEnd()
    {
        _reviveEndTcs?.TrySetResult();
    }

    private void SetTint(Color color)
    {
        foreach (var r in _renderers)
        {
            if (r.material.HasProperty("_BaseColor"))
                r.material.SetColor("_BaseColor", color);
            else
                r.material.color = color;
        }
    }
}
