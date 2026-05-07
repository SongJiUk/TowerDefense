using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 보스/중간보스 등장 연출 UI.
/// WaveManager.OnBossAppear 이벤트로 자동 실행 후 자동 숨김.
/// PrevLoad Addressable 그룹에 등록 필요.
/// </summary>
public class UI_BossAnnounce : UI_Base
{
    enum Texts   { Text_Warning, Text_BossName, Text_BossType }
    enum Images  { Image_Overlay, Image_Stripe, Image_BossIcon,
                   Image_BossType_BG, Image_BossType_Border,
                   Image_Top, Image_Bottom }
    enum Objects { Container_Content }

    // 중간보스
    private static readonly Color MIDBOSS_THEME   = new Color(1.000f, 0.702f, 0.000f);       // #FFB300
    private static readonly Color MIDBOSS_OVERLAY = new Color(0.220f, 0.137f, 0.000f, 0.85f); // #381F00
    private static readonly Color MIDBOSS_BORDER  = new Color(0.427f, 0.275f, 0.000f);        // #6D4600

    // 최종보스
    private static readonly Color BOSS_THEME   = new Color(0.800f, 0.133f, 0.200f);           // #CC2233
    private static readonly Color BOSS_OVERLAY = new Color(0.290f, 0.063f, 0.086f, 0.85f);    // #4A1016
    private static readonly Color BOSS_BORDER  = new Color(0.588f, 0.153f, 0.145f);           // #962725

    private CanvasGroup   _canvasGroup;
    private RectTransform _topRect;
    private RectTransform _bottomRect;
    private RectTransform _contentRect;
    private RectTransform _warningRect;
    private Tweener       _warningPulse;
    private Tweener       _stripeTween;
    private Material      _stripeMat;
    private float _topOriginY;
    private float _bottomOriginY;
    private Sequence _seq;
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
        BindObject(typeof(Objects));

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _topRect    = GetImage(typeof(Images), (int)Images.Image_Top).rectTransform;
        _bottomRect = GetImage(typeof(Images), (int)Images.Image_Bottom).rectTransform;
        _contentRect = GetObject(typeof(Objects), (int)Objects.Container_Content)
                           .GetComponent<RectTransform>();

        _warningRect = GetText(typeof(Texts), (int)Texts.Text_Warning).rectTransform;
        _stripeMat   = GetImage(typeof(Images), (int)Images.Image_Stripe).material;
        _topOriginY    = _topRect.anchoredPosition.y;
        _bottomOriginY = _bottomRect.anchoredPosition.y;

        Managers.WaveM.OnBossAppear += OnBossAppear;
        gameObject.SetActive(false);
        return true;
    }

    void OnDestroy()
    {
        _seq?.Kill();
        _warningPulse?.Kill();
        _stripeTween?.Kill();
        if (Managers.WaveM != null)
            Managers.WaveM.OnBossAppear -= OnBossAppear;
    }

    // ─── 연출 ─────────────────────────────────────────────────────────────────

    private void OnBossAppear(EnemyData data)
    {
        bool isFinal = data.enemyType == Define.EnemyType.Boss;

        Color theme   = isFinal ? BOSS_THEME   : MIDBOSS_THEME;
        Color overlay = isFinal ? BOSS_OVERLAY : MIDBOSS_OVERLAY;
        Color border  = isFinal ? BOSS_BORDER  : MIDBOSS_BORDER;

        // 텍스트
        GetText(typeof(Texts), (int)Texts.Text_BossName).text = data.enemyName;
        GetText(typeof(Texts), (int)Texts.Text_BossType).text = isFinal ? "× FINAL BOSS ×" : "× MID BOSS ×";

        // 테마 색 적용
        GetText(typeof(Texts), (int)Texts.Text_Warning).color  = theme;
        GetText(typeof(Texts), (int)Texts.Text_BossType).color = theme;

        GetImage(typeof(Images), (int)Images.Image_Overlay).color        = overlay;
        GetImage(typeof(Images), (int)Images.Image_Stripe).color          = theme;
        GetImage(typeof(Images), (int)Images.Image_Top).color            = theme;
        GetImage(typeof(Images), (int)Images.Image_Bottom).color         = theme;
        GetImage(typeof(Images), (int)Images.Image_BossType_BG).color    = overlay;
        GetImage(typeof(Images), (int)Images.Image_BossType_Border).color = border;

        // 아이콘
        var iconImg    = GetImage(typeof(Images), (int)Images.Image_BossIcon);
        var iconSprite = Managers.ResourceM.GetAtlas(data.prefabKey);
        iconImg.sprite  = iconSprite;
        iconImg.enabled = iconSprite != null;

        PlaySequence();
    }

    private void PlaySequence()
    {
        _seq?.Kill();

        // 초기 상태 — Top은 위로, Bottom은 아래로 대기
        _canvasGroup.alpha = 0f;
        _topRect.anchoredPosition    = new Vector2(_topRect.anchoredPosition.x,    _topOriginY    + 200f);
        _bottomRect.anchoredPosition = new Vector2(_bottomRect.anchoredPosition.x, _bottomOriginY - 200f);
        _contentRect.localScale  = Vector3.one * 0.88f;
        _warningRect.localScale  = Vector3.zero;
        gameObject.SetActive(true);

        _warningPulse?.Kill();
        _stripeTween?.Kill();

        // 줄무늬 흘러가는 루프 애니메이션
        if (_stripeMat != null)
        {
            _stripeMat.SetFloat("_Offset", 0f);
            _stripeTween = DOTween.To(
                () => _stripeMat.GetFloat("_Offset"),
                v  => _stripeMat.SetFloat("_Offset", v),
                1f, 1.2f
            ).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetUpdate(true);
        }

        _seq = DOTween.Sequence().SetUpdate(true)
            // 등장
            .Append(_canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad))
            .Join(_topRect.DOAnchorPosY(_topOriginY, 0.45f).SetEase(Ease.OutCubic))
            .Join(_bottomRect.DOAnchorPosY(_bottomOriginY, 0.45f).SetEase(Ease.OutCubic))
            .Join(_contentRect.DOScale(1f, 0.4f).SetEase(Ease.OutBack))
            // Warning 텍스트: 펀치 스케일로 등장 후 깜빡임
            .Join(_warningRect.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
            .AppendCallback(() =>
            {
                _warningPulse = _warningRect.DOPunchScale(Vector3.one * 0.12f, 0.3f, 5, 0.5f)
                    .SetLoops(-1, LoopType.Restart)
                    .SetUpdate(true);
            })
            // 유지
            .AppendInterval(2.2f)
            // 퇴장
            .AppendCallback(() => { _warningPulse?.Kill(); _stripeTween?.Kill(); })
            .Append(_canvasGroup.DOFade(0f, 0.35f).SetEase(Ease.InQuad))
            .OnComplete(() => gameObject.SetActive(false));
    }
}
