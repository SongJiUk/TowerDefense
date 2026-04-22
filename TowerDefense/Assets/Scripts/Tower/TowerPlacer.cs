using Cysharp.Threading.Tasks;
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
///   _cam(Main Camera), _markerLayer(Marker)
/// Addressable PrevLoad 필수:
///   UI_TowerSelectPopup 프리팹, TowerData 6종
/// </summary>
public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Instance { get; private set; }

    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _markerLayer;

    private UI_TowerSelectPopup _popup;
    private TowerData[] _allTowerData;
    private GridNode _pendingNode;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        if (_cam == null) _cam = Camera.main;
    }

    async void Start()
    {
        // 팝업 프리팹 인스턴스 생성 (PrevLoad로 미리 로드된 상태)
        GameObject popupGo = Managers.ResourceM.Instantiate("UI_TowerSelectPopup", _pooling: false);
        popupGo.transform.SetParent(Managers.UIM.Root.transform, false);
        _popup = popupGo.GetOrAddComponent<UI_TowerSelectPopup>();
        await _popup.Init();

        // TowerData 6종 — TowerType 순서대로 정렬 (팝업 버튼 순서와 일치)
        var loaded = Managers.ResourceM.GetAllLoaded<TowerData>();
        loaded.Sort((a, b) => a.towerType.CompareTo(b.towerType));
        _allTowerData = loaded.ToArray();

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
        if (_popup == null) return;
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

        _popup.Hide();
    }

    // ─── 내부 로직 ────────────────────────────────────────────────────────────

    private GridNode GetNodeFromScreen(Vector3 screenPos)
    {
        Ray ray = _cam.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f, _markerLayer)) return null;
        return Managers.Grid?.GetNode(hit.transform.position);
    }

    private void OpenPopup(GridNode node)
    {
        _pendingNode = node;
        Vector3 screenPos = _cam.WorldToScreenPoint(node.WorldPosition + Vector3.up * 1.5f);
        _popup.Show(screenPos, node.WorldPosition, _allTowerData);
    }

    private void HandleTowerSelected(TowerData data)
    {
        if (_pendingNode == null || _pendingNode.IsOccupied)
        {
            _popup.Hide();
            return;
        }

        if (data == null || data.addressableKey == null) return;

        int buildCost = Mathf.RoundToInt(data.buildCost * Managers.GameM.buildCostMultiplier);
        if (!Managers.GameM.SpendGold(buildCost))
        {
            Debug.Log("[TowerPlacer] 골드 부족");
            return;
        }

        GameObject go = Managers.PoolM.Pop(data.addressableKey);
        if (go == null)
        {
            Debug.LogError($"[TowerPlacer] 타워 프리팹 로드 실패: {data.addressableKey}");
            Managers.GameM.AddGold(buildCost);
            return;
        }

        go.transform.position = _pendingNode.WorldPosition;
        go.transform.rotation = Quaternion.identity;

        if (go.TryGetComponent(out TowerController tower))
            tower.Init(data);

        Managers.Grid.SetOccupied(_pendingNode.WorldPosition, true);

        _pendingNode = null;
        _popup.Hide();
    }
}
