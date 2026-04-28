using UnityEngine;

/// <summary>
/// 최초 사망 시 HP를 reviveHpRatio만큼 회복하여 1회 부활하는 적.
/// 부활 시 렌더러 색이 회색으로 바뀌어 부활 상태임을 표시한다.
/// </summary>
public class ReviveEnemyController : EnemyController
{
    private ReviveEnemyData _reviveData;
    private bool _hasRevived;
    private Renderer[] _renderers;

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

    protected override void Die()
    {
        if (!_hasRevived && _reviveData != null)
        {
            _hasRevived = true;
            _hp = _maxHp * _reviveData.reviveHpRatio;
            _hpBar?.SetHP(_hp, _maxHp);
            SetTint(new Color(0.55f, 0.55f, 0.55f));
            return;
        }
        base.Die();
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
