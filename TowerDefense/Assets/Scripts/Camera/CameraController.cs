using UnityEngine;

public class CameraController : MonoBehaviour, ILateTickable
{
    [Header("줌아웃 기본 위치")]
    [SerializeField] private Vector3 _zoomedOutPos = new Vector3(-16.4f, 18.15f, -1.2f);

    [Header("줌인 상태 위치")]
    [SerializeField] private float _zoomedInY    = 10.84f;
    [SerializeField] private float _zoomedInZ    =  6.11f;
    [SerializeField] private float _zoomedInMinX = -29.87f;
    [SerializeField] private float _zoomedInMaxX =  -3.01f;

    [Header("감도")]
    [SerializeField] private float _zoomSpeed        = 5f;
    [SerializeField] private float _moveSpeed        = 8f;
    [SerializeField] private float _pinchSensitivity = 0.05f;
    [SerializeField] private float _dragSensitivity  = 0.02f;

    private float   _moveDir = 0f;
    private float   _zoomDir = 0f;
    private Vector3 _forwardDir;
    private float   _prevPinchDist = 0f;
    private bool    _isDragging = false;
    private Vector2 _touchStartPos;
    private const float DRAG_THRESHOLD = 10f;

    public static bool IsDragging { get; private set; }

    private void Awake()
    {
        _forwardDir = transform.forward;
    }

    private void OnEnable()  => Managers.UpdateM.Register(_lateTickable: this);
    private void OnDisable() => Managers.UpdateM.UnRegister(_lateTickable: this);

    public void LateTick(float dt)
    {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Q))      _zoomDir =  1f;
        else if (Input.GetKey(KeyCode.E)) _zoomDir = -1f;
        else                              _zoomDir =  0f;

        HandleZoom(dt);
        HandleMove(dt);
#else
        HandleTouch(dt);
        HandleMove(dt);
#endif
        ClampPosition();
    }

    // ─── 터치 (모바일) ───────────────────────────────────────────────────────

    private void HandleTouch(float dt)
    {
        int touchCount = Input.touchCount;

        if (touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            float currentDist = Vector2.Distance(t0.position, t1.position);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _prevPinchDist = currentDist;
                return;
            }

            float delta = currentDist - _prevPinchDist;
            _prevPinchDist = currentDist;

            Vector3 next = transform.position + _forwardDir * delta * _pinchSensitivity;
            transform.position = next;
        }
        else if (touchCount == 1)
        {
            if (ZoomRatio() < 0.05f) { IsDragging = false; return; }

            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                _touchStartPos = t.position;
                _isDragging    = false;
                IsDragging     = false;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                if (!_isDragging && Vector2.Distance(t.position, _touchStartPos) > DRAG_THRESHOLD)
                    _isDragging = true;

                if (_isDragging)
                {
                    IsDragging = true;
                    Vector3 pos = transform.position;
                    pos.x -= t.deltaPosition.x * _dragSensitivity;
                    transform.position = pos;
                }
            }
            else if (t.phase == TouchPhase.Ended)
            {
                IsDragging = false;
                _isDragging = false;
            }
        }
        else
        {
            _prevPinchDist = 0f;
            IsDragging     = false;
        }
    }

    // ─── 버튼 줌 / 이동 ─────────────────────────────────────────────────────

    private void HandleZoom(float dt)
    {
        if (_zoomDir == 0f) return;
        Vector3 next = transform.position + _forwardDir * _zoomDir * _zoomSpeed * dt;
        transform.position = next;
    }

    private void HandleMove(float dt)
    {
        if (_moveDir == 0f) return;
        Vector3 pos = transform.position;
        pos.x += _moveDir * _moveSpeed * dt;
        transform.position = pos;
    }

    // ─── 위치 제한 ───────────────────────────────────────────────────────────

    /// <summary>줌 비율 0 = 줌아웃, 1 = 최대 줌인</summary>
    private float ZoomRatio()
    {
        float totalRange = _zoomedOutPos.y - _zoomedInY;
        if (Mathf.Approximately(totalRange, 0f)) return 0f;
        return Mathf.Clamp01((_zoomedOutPos.y - transform.position.y) / totalRange);
    }

    private void ClampPosition()
    {
        float ratio = ZoomRatio();
        Vector3 pos = transform.position;

        // Y, Z: 줌아웃~줌인 범위로 제한
        pos.y = Mathf.Clamp(pos.y, _zoomedInY, _zoomedOutPos.y);
        pos.z = Mathf.Lerp(_zoomedOutPos.z, _zoomedInZ, ratio);

        // X: 줌 비율에 따라 허용 범위가 열림
        float minX = Mathf.Lerp(_zoomedOutPos.x, _zoomedInMinX, ratio);
        float maxX = Mathf.Lerp(_zoomedOutPos.x, _zoomedInMaxX, ratio);
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        transform.position = pos;
    }

    // ─── 외부 호출 ───────────────────────────────────────────────────────────

    public void SetMoveDir(float dir) => _moveDir = dir;
    public void SetZoomDir(float dir) => _zoomDir = dir;
}
