using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// isTargeted 스킬 발동 시 범위/블록 프리뷰를 표시하고
/// 클릭한 위치를 SkillManager.ExecuteAt()으로 전달한다.
/// Block 스킬은 Road 타일에 스냅된 프리뷰를, 나머지는 RangeIndicator를 사용한다.
/// </summary>
public class SkillTargetingController : MonoBehaviour, ITickable
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private RangeIndicator _rangeIndicator;

    [Tooltip("Block 스킬 프리뷰용 오브젝트. 초록 반투명 큐브 프리팹을 연결.")]
    [SerializeField] private GameObject _blockPreview;

    private bool _isTargeting;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    void Awake()
    {
        if (_camera == null) _camera = Camera.main;
        _blockPreview?.SetActive(false);
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

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            Managers.SkillM.CancelTargeting();
            return;
        }

        if (!TryGetGroundPoint(out Vector3 worldPos))
        {
            HideAllPreviews();
            return;
        }

        if (Managers.SkillM.PendingSkill?.skillType == Define.SkillType.Block)
            HandleBlockTargeting(worldPos);
        else
            HandleRangeTargeting(worldPos);
    }

    // ─── 타겟팅 ───────────────────────────────────────────────────────────────

    private void StartTargeting(float range)
    {
        _isTargeting = true;
    }

    private void StopTargeting()
    {
        _isTargeting = false;
        HideAllPreviews();
    }

    // ─── Block 타겟팅 ─────────────────────────────────────────────────────────

    private void HandleBlockTargeting(Vector3 worldPos)
    {
        GridNode node = Managers.Grid?.GetNode(worldPos);
        bool canPlace = node != null && node.NodeType == NodeType.Road && node.CanWalk;

        if (canPlace)
        {
            ShowBlockPreview(node.WorldPosition);

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Managers.SkillM.ExecuteAt(node.WorldPosition);
                StopTargeting();
            }
        }
        else
        {
            _blockPreview?.SetActive(false);
        }
    }

    private void ShowBlockPreview(Vector3 pos)
    {
        if (_blockPreview == null) return;
        _blockPreview.SetActive(true);
        _blockPreview.transform.position = pos;
    }

    // ─── Range 타겟팅 (나머지 스킬) ──────────────────────────────────────────

    private void HandleRangeTargeting(Vector3 worldPos)
    {
        _rangeIndicator.Show(worldPos, Managers.SkillM.TargetingRange, false);

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Managers.SkillM.ExecuteAt(worldPos);
            StopTargeting();
        }
    }

    // ─── 내부 ─────────────────────────────────────────────────────────────────

    private void HideAllPreviews()
    {
        _rangeIndicator.Hide();
        _blockPreview?.SetActive(false);
    }

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
