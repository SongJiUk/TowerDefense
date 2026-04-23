using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Placeable 마커 클릭 시 마커 위치 기준으로 6개 타워 버튼을 방사형으로 펼쳐 표시.
/// </summary>
public class UI_TowerSelectPopup : UI_Base
{
    // ─── Enum (자식 오브젝트 이름과 정확히 일치) ──────────────────────────────

    enum Buttons
    {
        Tower_Basic, Tower_Cannon, Tower_Slow,
        Tower_Sniper, Tower_Poison, Tower_Lighting,
    }

    private const int   TOWER_COUNT   = 6;
    private const float ANIM_DURATION = 0.25f;

    // ─── 이벤트 ───────────────────────────────────────────────────────────────

    public event Action<TowerData> OnTowerSelected;

    // ─── 설정 ─────────────────────────────────────────────────────────────────

    [SerializeField] private float          _radius         = 90f;
    [SerializeField] private float          _staggerDelay   = 0.03f;  // 버튼 간 딜레이
    [SerializeField] private RangeIndicator _rangeIndicator;

    // ─── 내부 상태 ────────────────────────────────────────────────────────────

    private TowerData[]   _currentData;
    private Vector3       _tileWorldPos;
    private RectTransform _rect;
    private Canvas        _rootCanvas;

    // 각 버튼의 방사형 목표 위치 (ArrangeRadial에서 계산 후 저장)
    private readonly Vector2[] _radialPositions = new Vector2[TOWER_COUNT];

    // ─── 원형 배치 각도 (위쪽부터 시계방향, 6개 60° 간격) ────────────────────

    private static readonly float[] ANGLES = { 90f, 30f, -30f, -90f, -150f, 150f };

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    void OnDestroy()
    {
        Managers.GameM.OnGoldChanged -= RefreshButtonStates;
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        _rect       = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();

        BindButton(typeof(Buttons));

        for (int i = 0; i < TOWER_COUNT; i++)
        {
            int idx = i;
            Button btn = GetButton(typeof(Buttons), (int)Buttons.Tower_Basic + idx);

            btn.onClick.AddListener(() => OnTowerButtonClicked(idx));

            BindEvent(btn.gameObject, () => OnButtonHoverEnter(idx),
                      _type: Define.UIEvent.PointerEnter);

            BindEvent(btn.gameObject, null, (_) => _rangeIndicator?.Hide(),
                      Define.UIEvent.OnPointerExit);
        }

        Managers.GameM.OnGoldChanged += RefreshButtonStates;

        CalcRadialPositions();
        ApplyTheme(Managers.WaveM.CurrentStage);

        return true;
    }

    public override void ApplyTheme(StageData stage)
    {
        if (stage == null) return;
        // 버튼 이미지에 강조 색 적용 — 필요한 오브젝트 추가
        for (int i = 0; i < TOWER_COUNT; i++)
        {
            var btn = GetButton(typeof(Buttons), (int)Buttons.Tower_Basic + i);
            btn.GetComponent<Image>().color = stage.uiAccentColor;
        }
    }

    // ─── 공개 API ─────────────────────────────────────────────────────────────

    public void Show(Vector3 screenPos, Vector3 tileWorldPos, TowerData[] towerData)
    {
        if (!isInit) Init().Forget();

        _currentData  = towerData;
        _tileWorldPos = tileWorldPos;

        PositionPopup(screenPos);
        PopulateButtons();
        RefreshButtonStates(Managers.GameM.Gold);

        gameObject.SetActive(true);
        PlayOpenAnim();
    }

    public void Hide()
    {
        _rangeIndicator?.Hide();
        DOTween.Kill(gameObject);  // 진행 중 애니메이션 중단
        gameObject.SetActive(false);
    }

    // ─── 방사형 위치 계산 ────────────────────────────────────────────────────

    private void CalcRadialPositions()
    {
        for (int i = 0; i < TOWER_COUNT; i++)
        {
            float rad = ANGLES[i] * Mathf.Deg2Rad;
            _radialPositions[i] = new Vector2(
                Mathf.Cos(rad) * _radius,
                Mathf.Sin(rad) * _radius
            );
        }
    }

    // ─── 열기 애니메이션 ─────────────────────────────────────────────────────

    private void PlayOpenAnim()
    {
        for (int i = 0; i < TOWER_COUNT; i++)
        {
            Button btn = GetButton(typeof(Buttons), (int)Buttons.Tower_Basic + i);
            if (!btn.gameObject.activeSelf) continue;

            RectTransform rt = btn.GetComponent<RectTransform>();

            // 시작: 중앙, 스케일 0
            rt.anchoredPosition = Vector2.zero;
            rt.localScale       = Vector3.zero;

            float delay = i * _staggerDelay;

            // 목표 위치로 OutBack 튀어나오기
            rt.DOAnchorPos(_radialPositions[i], ANIM_DURATION)
              .SetDelay(delay)
              .SetEase(Ease.OutBack)
              .SetLink(gameObject);

            rt.DOScale(Vector3.one, ANIM_DURATION)
              .SetDelay(delay)
              .SetEase(Ease.OutBack)
              .SetLink(gameObject);
        }
    }

    // ─── 버튼 내용 채우기 ────────────────────────────────────────────────────

    private void PopulateButtons()
    {
        for (int i = 0; i < TOWER_COUNT; i++)
        {
            Button btn = GetButton(typeof(Buttons), (int)Buttons.Tower_Basic + i);
            bool hasData = _currentData != null
                        && i < _currentData.Length
                        && _currentData[i] != null;

            btn.gameObject.SetActive(true);
            btn.interactable = hasData;
        }
    }

    private void RefreshButtonStates(int gold)
    {
        if (_currentData == null) return;

        for (int i = 0; i < TOWER_COUNT; i++)
        {
            if (i >= _currentData.Length || _currentData[i] == null) continue;
            GetButton(typeof(Buttons), (int)Buttons.Tower_Basic + i).interactable
                = gold >= _currentData[i].buildCost;
        }
    }

    // ─── 이벤트 핸들러 ───────────────────────────────────────────────────────

    private void OnTowerButtonClicked(int index)
    {
        if (_currentData == null || index >= _currentData.Length) return;
        _rangeIndicator?.Hide();
        OnTowerSelected?.Invoke(_currentData[index]);
    }

    private void OnButtonHoverEnter(int index)
    {
        if (_currentData == null || index >= _currentData.Length) return;
        TowerData data = _currentData[index];
        if (data == null || _rangeIndicator == null) return;

        _rangeIndicator.Show(_tileWorldPos, data.baseRange);
    }

    // ─── 팝업 위치 ────────────────────────────────────────────────────────────

    private void PositionPopup(Vector3 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.transform as RectTransform,
            screenPos,
            _rootCanvas.worldCamera,
            out Vector2 localPos
        );
        _rect.localPosition = localPos;

        Canvas.ForceUpdateCanvases();
        Vector3[] corners = new Vector3[4];
        _rect.GetWorldCorners(corners);

        Vector2 offset = Vector2.zero;
        if (corners[0].x < 0)             offset.x = -corners[0].x;
        if (corners[2].x > Screen.width)  offset.x = Screen.width  - corners[2].x;
        if (corners[0].y < 0)             offset.y = -corners[0].y;
        if (corners[2].y > Screen.height) offset.y = Screen.height - corners[2].y;

        _rect.localPosition += (Vector3)(offset / _rootCanvas.scaleFactor);
    }
}
