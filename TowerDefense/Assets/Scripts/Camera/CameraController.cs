using UnityEngine;

public class CameraController : MonoBehaviour, ILateTickable
{
    [Header("이동")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _minX = -12f;
    [SerializeField] private float _maxX = 12f;

    [Header("줌")]
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _minY = 5f;   // 최대 줌인 (카메라 낮아짐)
    [SerializeField] private float _maxY = 14f;  // 최대 줌아웃 (카메라 높아짐)

    private float _moveDir = 0f;   // -1, 0, 1
    private float _zoomDir = 0f;   // -1, 0, 1

    // 카메라 기울기 유지한 채 앞뒤 이동 방향
    private Vector3 _forwardDir;

    private void Awake()
    {
        _forwardDir = transform.forward;
    }

    private void OnEnable()
    {
        Managers.UpdateM.Register(_lateTickable: this);
    }

    private void OnDisable()
    {
        Managers.UpdateM.UnRegister(_lateTickable: this);
    }

    public void LateTick(float _dt)
    {
        HandleMove(_dt);
        HandleZoom(_dt);
    }

    private void HandleMove(float _dt)
    {
        if (_moveDir == 0f) return;

        Vector3 pos = transform.position;
        pos.x += _moveDir * _moveSpeed * _dt;
        pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
        transform.position = pos;
    }

    private void HandleZoom(float _dt)
    {
        if (_zoomDir == 0f) return;

        // forward 방향으로 이동하면 줌인, 반대면 줌아웃
        Vector3 next = transform.position + _forwardDir * _zoomDir * _zoomSpeed * _dt;

        // Y축으로 줌 범위 제한
        next.y = Mathf.Clamp(next.y, _minY, _maxY);

        transform.position = next;
    }

    // 버튼에서 호출
    public void SetMoveDir(float _dir) => _moveDir = _dir;
    public void SetZoomDir(float _dir) => _zoomDir = _dir;
}
