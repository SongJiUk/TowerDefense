using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// iOS 스타일 슬라이딩 토글 비주얼.
/// Toggle 컴포넌트와 함께 사용. Toggle Transition → None, Graphic → None.
/// _track, _handle을 Inspector에서 연결할 것.
/// </summary>
[RequireComponent(typeof(Toggle))]
public class UI_ToggleSwitch : MonoBehaviour
{
    [SerializeField] private Image          _track;
    [SerializeField] private RectTransform  _handle;

    [SerializeField] private Color _colorOn  = new Color(1f, 0.55f, 0f);
    [SerializeField] private Color _colorOff = new Color(0.35f, 0.35f, 0.35f);
    [SerializeField] private float _handleOffset = 12f;
    [SerializeField] private float _duration     = 0.2f;

    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();

        // Toggle 루트에 Image가 없으면 투명하게 추가 — 레이캐스트 수신용
        if (_toggle.targetGraphic == null)
        {
            var img = gameObject.AddComponent<Image>();
            img.color = Color.clear;
            _toggle.targetGraphic = img;
        }

        _toggle.onValueChanged.AddListener(_ => Apply(_toggle.isOn, animate: true));
        Apply(_toggle.isOn, animate: false);
    }

    /// <summary>SetIsOnWithoutNotify 후 비주얼만 즉시 동기화.</summary>
    public void Refresh() => Apply(_toggle.isOn, animate: false);

    private void Apply(bool isOn, bool animate)
    {
        float targetX     = isOn ? _handleOffset : -_handleOffset;
        Color targetColor = isOn ? _colorOn : _colorOff;

        _handle.anchoredPosition = new Vector2(targetX, 0f);
        _track.color             = targetColor;

        if (animate)
        {
            _handle.DOAnchorPosX(targetX, _duration).SetEase(Ease.OutQuad);
            _track.DOColor(targetColor, _duration);
        }
    }
}
