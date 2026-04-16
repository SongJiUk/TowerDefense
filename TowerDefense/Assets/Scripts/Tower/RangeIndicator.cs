using UnityEngine;

/// <summary>
/// 타워 클릭 시 공격 범위를 반투명 디스크로 바닥에 표시.
/// Cylinder 프리미티브를 납작하게 눌러 원형 면으로 사용.
/// </summary>
public class RangeIndicator : MonoBehaviour
{
    [SerializeField] private Material _rangeMaterial;   // 반투명 머티리얼 (Inspector에서 연결)
    [SerializeField] private float    _yOffset = 0.05f; // 바닥 z-fighting 방지 오프셋

    private GameObject _disc;

    void Awake()
    {
        _disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _disc.transform.SetParent(transform);
        Destroy(_disc.GetComponent<Collider>());

        var mr = _disc.GetComponent<MeshRenderer>();
        if (_rangeMaterial != null)
        {
            mr.material = _rangeMaterial;
        }
        else
        {
            // 머티리얼 미연결 시 반투명 초록 자동 생성
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color   = new Color(0.2f, 0.9f, 0.2f, 0.25f);
            mr.material = mat;
        }

        _disc.SetActive(false);
    }

    /// <summary>타워 클릭 시 호출. 타워 위치 기준으로 반투명 원을 표시한다.</summary>
    public void Show(Vector3 center, float radius)
    {
        _disc.SetActive(true);
        _disc.transform.position   = new Vector3(center.x, center.y + _yOffset, center.z);
        _disc.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
    }

    public void Hide() => _disc.SetActive(false);
}
