using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 킹덤러쉬 스타일 타워 배치 전담 싱글톤.
/// Marker 큐브 클릭 → 원형 팝업 → 타워 선택 → 즉시 설치.
///
/// 클릭 흐름:
///   Update → Physics.Raycast(Marker 레이어) → GetNodeFromScreen()
///   → 빈 Placeable 노드 → OpenPopup(node)
///   → 팝업에서 타워 선택 → HandleTowerSelected(data)
///   → 골드 차감 → PoolM.Pop(prefab) → TowerController.Init(data) → SetOccupied
///
/// Inspector 연결 필수:
///   _cam(Main Camera), _markerLayer(Marker), _popup(UI_TowerSelectPopup), _allTowerData[6]
/// </summary>
public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Instance { get; private set; }

    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _markerLayer;
    [SerializeField] private UI_TowerSelectPopup _popup;
    [SerializeField] private TowerData[] _allTowerData;   // 6종 ScriptableObject

    private GridNode _pendingNode;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        if (_cam == null) _cam = Camera.main;
    }

    void Start()
    {
        _popup.OnTowerSelected += HandleTowerSelected;
        _popup.Hide();
    }

    void OnDestroy()
    {
        if (_popup != null)
            _popup.OnTowerSelected -= HandleTowerSelected;

        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (!Input.GetMouseButtonUp(0)) return;
        if (CameraController.IsDragging) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (EventSystem.current.IsPointerOverGameObject()) return;
#else
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
#endif

        GridNode node = GetNodeFromScreen(Input.mousePosition);

        if (node != null && node.NodeType == NodeType.Placeable && !node.IsOccupied)
        {
            OpenPopup(node);
            return;
        }

        // Placeable이 아닌 곳 클릭 → 팝업 닫기
        _popup.Hide();
    }

    // ─── 내부 로직 ────────────────────────────────────────────────────────────

    /// <summary>
    /// Marker 레이어에만 Physics.Raycast를 쏴서 클릭한 GridNode를 반환.
    /// 마커 큐브는 부피가 있어 쿼터뷰(45도)에서도 정확하게 클릭 감지 가능.
    /// (이전: Plane.Raycast 방식은 얇은 타일 콜라이더로 정확도가 낮았음)
    /// </summary>
    private GridNode GetNodeFromScreen(Vector3 screenPos)
    {
        Ray ray = _cam.ScreenPointToRay(screenPos);

        if (!Physics.Raycast(ray, out RaycastHit hit, 200f, _markerLayer)) return null;

        return Managers.Grid?.GetNode(hit.transform.position);
    }

    /// <summary>팝업을 열고 _pendingNode를 저장한다.</summary>
    private void OpenPopup(GridNode node)
    {
        _pendingNode = node;
        Vector3 screenPos = _cam.WorldToScreenPoint(node.WorldPosition + Vector3.up * 1.5f);
        _popup.Show(screenPos, node.WorldPosition, _allTowerData);
    }

    /// <summary>
    /// UI_TowerSelectPopup.OnTowerSelected 이벤트 수신 시 호출.
    /// 골드 차감 → 타워 스폰 → Init → SetOccupied(마커 숨김 + 경로 재계산).
    /// </summary>
    private void HandleTowerSelected(TowerData data)
    {
        if (_pendingNode == null || _pendingNode.IsOccupied)
        {
            _popup.Hide();
            return;
        }

        if (data == null || data.prefab == null) return;

        if (!Managers.SpendGold(data.buildCost))
        {
            Debug.Log("[TowerPlacer] 골드 부족");
            return;
        }

        GameObject go = Managers.PoolM.Pop(data.prefab);
        go.transform.position = _pendingNode.WorldPosition;
        go.transform.rotation = Quaternion.identity;

        if (go.TryGetComponent(out TowerController tower))
            tower.Init(data);

        Managers.Grid.SetOccupied(_pendingNode.WorldPosition, true);

        _pendingNode = null;
        _popup.Hide();
    }
}
