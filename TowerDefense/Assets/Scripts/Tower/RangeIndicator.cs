using UnityEngine;

/// <summary>
/// 타워 클릭 시 공격 범위 표시.
/// - Cylinder: 반투명 채움 영역
/// - LineRenderer: 외곽 원선 (각 점마다 지면 레이캐스트 → 타일 높낮이 따라감)
/// </summary>
public class RangeIndicator : MonoBehaviour
{
    [SerializeField] private Color _fillColor    = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private Color _outlineColor = new Color(0f, 0.85f, 1f, 1f);
    [SerializeField] private float _lineWidth    = 0.1f;
    [SerializeField] private int   _segments     = 64;
    [SerializeField] private float _yOffset      = 0.25f;
    [SerializeField] private bool  _showOutline  = true;

    private GameObject   _disc;
    private LineRenderer _lr;
    private static int   _groundMask;

    void Awake()
    {
        _groundMask = LayerMask.GetMask("Road", "Placeable");

        // ── 채움 디스크 ──────────────────────────────────────────
        _disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _disc.transform.SetParent(transform);
        Destroy(_disc.GetComponent<Collider>());

        var fillMat = new Material(Shader.Find("Sprites/Default"));
        fillMat.color = _fillColor;
        _disc.GetComponent<MeshRenderer>().material = fillMat;
        _disc.SetActive(false);

        // ── 외곽선 ───────────────────────────────────────────────
        _lr = GetComponentInChildren<LineRenderer>();
        if (_lr == null) _lr = gameObject.AddComponent<LineRenderer>();

        _lr.loop          = true;
        _lr.positionCount = _segments;
        _lr.startWidth    = _lineWidth;
        _lr.endWidth      = _lineWidth;
        _lr.useWorldSpace = true;

        var outlineMat = new Material(Shader.Find("Sprites/Default"));
        outlineMat.color = _outlineColor;
        _lr.material = outlineMat;
        _lr.enabled  = false;
    }

    public void Show(Vector3 center, float radius, bool followTerrain = true)
    {
        float centerGroundY = GetGroundY(center.x, center.y, center.z);

        Vector3 ps = transform.lossyScale;
        _disc.SetActive(true);
        _disc.transform.position   = new Vector3(center.x, centerGroundY + _yOffset, center.z);
        _disc.transform.localScale = new Vector3(radius * 2f / ps.x, 0.01f, radius * 2f / ps.z);

        for (int i = 0; i < _segments; i++)
        {
            float angle = (float)i / _segments * Mathf.PI * 2f;
            float x     = center.x + Mathf.Cos(angle) * radius;
            float z     = center.z + Mathf.Sin(angle) * radius;
            float y     = followTerrain ? GetGroundY(x, center.y, z) : centerGroundY;
            _lr.SetPosition(i, new Vector3(x, y + _yOffset, z));
        }

        _lr.enabled = _showOutline;
    }

    public void Hide()
    {
        _disc.SetActive(false);
        _lr.enabled = false;
    }

    private float GetGroundY(float x, float fromY, float z)
    {
        Ray ray = new Ray(new Vector3(x, fromY + 5f, z), Vector3.down);
        return Physics.Raycast(ray, out RaycastHit hit, 20f, _groundMask)
            ? hit.point.y
            : fromY;
    }
}
