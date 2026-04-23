using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour
{
    [SerializeField] private Image _fill;

    private static readonly Color COLOR_FULL = new Color(0.20f, 0.85f, 0.20f, 1f);
    private static readonly Color COLOR_HALF = new Color(0.95f, 0.80f, 0.10f, 1f);
    private static readonly Color COLOR_LOW  = new Color(0.90f, 0.15f, 0.10f, 1f);

    private Camera _cam;
    private Transform _target;
    private Vector3 _offset;

    void Awake()
    {
        _cam = Camera.main;
    }

    public void Follow(Transform target, Vector3 offset)
    {
        _target = target;
        _offset = offset;
    }

    void LateUpdate()
    {
        if (_target != null)
            transform.position = _target.position + _offset;

        if (_cam != null)
            transform.rotation = _cam.transform.rotation;
    }

    public void SetHP(float current, float max)
    {
        if (max <= 0f) return;
        float ratio = Mathf.Clamp01(current / max);
        _fill.fillAmount = ratio;
        _fill.color = ratio > 0.5f
            ? Color.Lerp(COLOR_HALF, COLOR_FULL, (ratio - 0.5f) * 2f)
            : Color.Lerp(COLOR_LOW,  COLOR_HALF, ratio * 2f);
    }
}
