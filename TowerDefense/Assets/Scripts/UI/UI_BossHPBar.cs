using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 보스(중간/최종) HP바. WaveManager.OnBossSpawned 이벤트로 자동 표시.
/// Image_HPFill(앞, 밝은색) = 딜레이 트윈 / Image_DelayFill(뒤, 어두운색) = 즉시 업데이트
/// </summary>
public class UI_BossHPBar : UI_Base
{
    enum Texts  { Text_EnemyName/*, Text_BossName, Text_BossSubtitle*/ }
    enum Images { Image_HPFill, Image_DelayFill, Image_BG, Image_Border }
    // enum Objects { Object_BossAnnounce }

    // 중간보스 — UI_NextWavePanel 색상 체계 동일
    private static readonly Color MIDBOSS_HP_FILL = new Color(1.000f, 0.702f, 0.000f); // #FFB300
    private static readonly Color MIDBOSS_BG      = new Color(0.220f, 0.137f, 0.000f); // #381F00
    private static readonly Color MIDBOSS_BORDER  = new Color(0.427f, 0.275f, 0.000f); // #6D4600

    // 최종보스
    private static readonly Color BOSS_HP_FILL = new Color(0.800f, 0.133f, 0.200f); // #CC2233
    private static readonly Color BOSS_BG      = new Color(0.290f, 0.063f, 0.086f); // #4A1016
    private static readonly Color BOSS_BORDER  = new Color(0.588f, 0.153f, 0.145f); // #962725

    private static Color DarkFill(Color c) => new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, c.a);

    private EnemyController _boss;
    private Tweener _hpTween;
    private Tweener _slideTween;
    private RectTransform _rect;
    private float _originY;
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
        // BindObject(typeof(Objects));

        _rect    = GetComponent<RectTransform>();
        _originY = _rect.anchoredPosition.y;

        Managers.WaveM.OnBossSpawned += OnBossSpawned;
        gameObject.SetActive(false);
        return true;
    }

    void OnDestroy()
    {
        _hpTween?.Kill();
        _slideTween?.Kill();
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

        GetText(typeof(Texts), (int)Texts.Text_EnemyName).text = boss.Data?.enemyName ?? "BOSS";
        // GetText(typeof(Texts), (int)Texts.Text_BossName).text = boss.Data?.enemyName ?? "BOSS";
        // GetText(typeof(Texts), (int)Texts.Text_BossSubtitle).text = boss.Data?.subtitle ?? "";
        // GetObject(typeof(Objects), (int)Objects.Object_BossAnnounce).SetActive(true);

        bool isFinalBoss = boss is BossEnemyController;
        Color fillColor = isFinalBoss ? BOSS_HP_FILL : MIDBOSS_HP_FILL;
        GetImage(typeof(Images), (int)Images.Image_HPFill).color    = fillColor;
        GetImage(typeof(Images), (int)Images.Image_DelayFill).color = DarkFill(fillColor);
        GetImage(typeof(Images), (int)Images.Image_BG).color        = isFinalBoss ? BOSS_BG     : MIDBOSS_BG;
        GetImage(typeof(Images), (int)Images.Image_Border).color    = isFinalBoss ? BOSS_BORDER : MIDBOSS_BORDER;

        GetImage(typeof(Images), (int)Images.Image_HPFill).fillAmount   = 1f;
        GetImage(typeof(Images), (int)Images.Image_DelayFill).fillAmount = 1f;

        _slideTween?.Kill();
        _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, _originY + 80f);
        gameObject.SetActive(true);
        _slideTween = _rect.DOAnchorPosY(_originY, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void RefreshHP(float cur, float max)
    {
        if (max <= 0f) return;
        float ratio = Mathf.Clamp01(cur / max);

        // HPFill(앞, 밝은색) = 즉시 → 현재 HP 바로 반영
        GetImage(typeof(Images), (int)Images.Image_HPFill).fillAmount = ratio;

        // DelayFill(뒤, 어두운색) = 딜레이 트윈 → 유령 효과
        _hpTween?.Kill();
        _hpTween = GetImage(typeof(Images), (int)Images.Image_DelayFill)
            .DOFillAmount(ratio, 0.4f)
            .SetDelay(0.3f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
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
