using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 보스(중간/최종) HP바. WaveManager.OnBossSpawned 이벤트로 자동 표시.
/// GameScene Canvas 상단에 배치, 기본 비활성화.
/// 오브젝트: Text_BossName / Image_HPFill
/// </summary>
public class UI_BossHPBar : UI_Base
{
    enum Texts  { Text_BossName }
    enum Images { Image_HPFill }

    private EnemyController _boss;
    private Tweener _fillTween;
    private bool _bound;

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    async void Start()
    {
        await GameSceneBootstrap.ReadyTask;
        await Init();
    }

    public override async UniTask<bool> Init()
    {
        if (_bound) return true;
        if (!await base.Init()) return false;
        _bound = true;

        BindText(typeof(Texts));
        BindImage(typeof(Images));

        Managers.WaveM.OnBossSpawned += OnBossSpawned;
        gameObject.SetActive(false);
        return true;
    }

    void OnDestroy()
    {
        _fillTween?.Kill();
        if (Managers.WaveM != null)
            Managers.WaveM.OnBossSpawned -= OnBossSpawned;
        DetachBoss();
    }

    // ─── 이벤트 ───────────────────────────────────────────────────────────────

    private void OnBossSpawned(EnemyController boss)
    {
        DetachBoss();
        _boss = boss;
        _boss.OnHpChanged  += RefreshHP;
        _boss.OnDeathEvent += OnBossDied;

        GetText(typeof(Texts), (int)Texts.Text_BossName).text = boss.Data?.enemyName ?? "BOSS";
        GetImage(typeof(Images), (int)Images.Image_HPFill).fillAmount = 1f;

        gameObject.SetActive(true);
    }

    private void RefreshHP(float cur, float max)
    {
        if (max <= 0f) return;
        float ratio = Mathf.Clamp01(cur / max);

        _fillTween?.Kill();
        _fillTween = GetImage(typeof(Images), (int)Images.Image_HPFill)
            .DOFillAmount(ratio, 0.15f)
            .SetEase(Ease.OutQuad);
    }

    private void OnBossDied()
    {
        DetachBoss();
        gameObject.SetActive(false);
    }

    // ─── 유틸 ─────────────────────────────────────────────────────────────────

    private void DetachBoss()
    {
        if (_boss == null) return;
        _boss.OnHpChanged  -= RefreshHP;
        _boss.OnDeathEvent -= OnBossDied;
        _boss = null;
    }
}
