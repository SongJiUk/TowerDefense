using UnityEngine;

/// <summary>
/// 타워 선택 팝업에서 버튼 호버 시 사거리를 3D 원으로 표시.
/// LineRenderer 48각형으로 원을 근사. 씬에 하나만 배치하고 UI_TowerSelectPopup이 참조한다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class RangeIndicator : MonoBehaviour
{
    private const int SEGMENTS = 48;

    private LineRenderer _line;

    void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.loop           = true;
        _line.positionCount  = SEGMENTS;
        _line.useWorldSpace  = true;
        _line.startWidth     = 0.12f;
        _line.endWidth       = 0.12f;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 월드좌표 center를 기준으로 radius 반지름의 원을 표시한다.
    /// UI_TowerSelectPopup에서 버튼 호버 시 호출.
    /// </summary>
    /// <param name="center">원의 중심 월드좌표 (타일 위치)</param>
    /// <param name="radius">사거리 반지름 (유닛)</param>
    public void Show(Vector3 center, float radius)
    {
        gameObject.SetActive(true);

        for (int i = 0; i < SEGMENTS; i++)
        {
            float angle = i * (360f / SEGMENTS) * Mathf.Deg2Rad;
            float x = center.x + radius * Mathf.Cos(angle);
            float z = center.z + radius * Mathf.Sin(angle);
            _line.SetPosition(i, new Vector3(x, center.y + 0.15f, z));
        }
    }

    /// <summary>버튼 호버 이탈 또는 팝업 닫힘 시 호출.</summary>
    public void Hide() => gameObject.SetActive(false);
}
