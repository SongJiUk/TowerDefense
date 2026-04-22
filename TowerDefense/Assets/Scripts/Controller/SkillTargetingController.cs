using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// isTargeted 스킬 발동 시 RangeIndicator로 범위를 표시하고
/// 클릭한 위치를 SkillManager.ExecuteAt()으로 전달한다.
/// ITickable로 UpdateManager에 등록해 개별 Update를 사용하지 않는다.
/// </summary>
public class SkillTargetingController : MonoBehaviour, ITickable
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private RangeIndicator _rangeIndicator;

    private bool _isTargeting;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    void Awake()
    {
        if (_camera == null) _camera = Camera.main;
    }

    void OnEnable()
    {
        Managers.UpdateM.Register(this);
        Managers.SkillM.OnTargetingStarted += StartTargeting;
        Managers.SkillM.OnTargetingCancelled += StopTargeting;
    }

    void OnDisable()
    {
        Managers.UpdateM.UnRegister(this);
        Managers.SkillM.OnTargetingStarted -= StartTargeting;
        Managers.SkillM.OnTargetingCancelled -= StopTargeting;
    }

    // ─── ITickable ────────────────────────────────────────────────────────────

    public void Tick(float deltaTime)
    {
        if (!_isTargeting) return;

        if (!TryGetGroundPoint(out Vector3 worldPos))
        {
            _rangeIndicator.Hide();
            return;
        }

        _rangeIndicator.Show(worldPos, Managers.SkillM.TargetingRange, false);

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            Managers.SkillM.CancelTargeting();
            return;
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Managers.SkillM.ExecuteAt(worldPos);
            StopTargeting();
        }
    }

    // ─── 타겟팅 ───────────────────────────────────────────────────────────────

    private void StartTargeting(float range)
    {
        _isTargeting = true;
    }

    private void StopTargeting()
    {
        _isTargeting = false;
        _rangeIndicator.Hide();
    }

    // ─── 내부 ─────────────────────────────────────────────────────────────────

    private bool TryGetGroundPoint(out Vector3 point)
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, _groundLayer))
        {
            point = hit.point;
            return true;
        }
        point = Vector3.zero;
        return false;
    }
}
