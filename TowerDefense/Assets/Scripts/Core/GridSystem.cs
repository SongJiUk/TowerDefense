using UnityEngine;

/// <summary>
/// 맵 전체를 32×11 격자로 관리.
/// OnEnable 시 Raycast로 타일을 자동 인식하고 GridNode 배열을 초기화한다.
/// Placeable 칸에는 마커 큐브를 자동 스폰.
/// Managers.Grid에 자신을 등록해 PathFinder·TowerPlacer가 전역에서 접근 가능.
/// </summary>
public class GridSystem : MonoBehaviour
{
    [Header("그리드 크기")]
    [SerializeField] private int _width = 32;
    [SerializeField] private int _height = 11;
    [SerializeField] private float _cellSize = 2f;

    [Header("맵 기준점 (좌측 하단)")]
    [SerializeField] private Vector3 _origin = new Vector3(-36.5f, 0f, -5.62f);

    [Header("레이어")]
    [SerializeField] private LayerMask _roadLayer;
    [SerializeField] private LayerMask _placeableLayer;

    [Header("설치 가능 마커")]
    [Tooltip("Placeable 칸에 자동으로 올릴 큐브 프리팹. 비워두면 기본 큐브로 생성.")]
    [SerializeField] private GameObject _markerPrefab;

    /// <summary>TowerPlacer의 Plane.Raycast 기준 Y값.</summary>
    public float OriginY => _origin.y;

    private GridNode[,] _grid;


    void OnEnable()  => InitGrid();
    void OnDisable() => ClearMarkers();


    /// <summary>
    /// 그리드 전체 초기화.
    /// 각 칸의 월드좌표를 계산하고 위에서 아래로 Raycast해 NodeType을 판별.
    /// Placeable 칸에는 마커 큐브를 스폰해 GridNode.Marker에 저장.
    /// 완료 후 Managers.Grid에 자신을 등록.
    /// </summary>
    private void InitGrid()
    {
        _grid = new GridNode[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                Vector3 worldPos = GridToWorld(x, z);
                (NodeType nodeType, float hitY) = GetNodeInfo(worldPos);

                if (nodeType != NodeType.None)
                    worldPos.y = hitY;

                _grid[x, z] = new GridNode(x, z, worldPos, nodeType);

                if (nodeType == NodeType.Placeable)
                    _grid[x, z].Marker = SpawnMarker(worldPos);
            }
        }

        Managers.Grid = this;
    }

    /// <summary>
    /// 해당 월드좌표 위에서 아래로 Raycast해 NodeType과 타일 Y값을 반환.
    /// Road 레이어 우선 검사 후 Placeable 레이어 검사.
    /// </summary>
    private (NodeType, float) GetNodeInfo(Vector3 _worldPos)
    {
        Vector3 rayOrigin = _worldPos + Vector3.up * 20f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit roadHit, 30f, _roadLayer))
            return (NodeType.Road, roadHit.point.y);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit placeHit, 30f, _placeableLayer))
            return (NodeType.Placeable, placeHit.point.y);

        return (NodeType.None, _worldPos.y);
    }

    /// <summary>
    /// 마커 큐브 스폰.
    /// _markerPrefab이 있으면 인스턴스화, 없으면 기본 큐브를 생성하고 Marker 레이어 설정.
    /// </summary>
    private GameObject SpawnMarker(Vector3 worldPos)
    {
        return _markerPrefab != null
            ? Instantiate(_markerPrefab, worldPos, Quaternion.identity, transform)
            : CreateDefaultMarker(worldPos);
    }

    /// <summary>
    /// 프리팹 없을 때 기본 마커 큐브 생성.
    /// 납작한 큐브(1.6×0.3×1.6)를 만들고 Marker 레이어 할당.
    /// </summary>
    private GameObject CreateDefaultMarker(Vector3 worldPos)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(transform);
        go.transform.position = worldPos;
        go.transform.localScale = new Vector3(1.6f, 0.3f, 1.6f);
        go.layer = LayerMask.NameToLayer("Marker");
        return go;
    }

    /// <summary>
    /// OnDisable 시 모든 마커 큐브를 Destroy하고 그리드를 초기화.
    /// 씬 전환 시 마커가 남아있지 않도록 정리.
    /// </summary>
    private void ClearMarkers()
    {
        if (_grid == null) return;
        foreach (GridNode node in _grid)
        {
            if (node?.Marker == null) continue;
            Destroy(node.Marker);
            node.Marker = null;
        }
        _grid = null;
    }

    /// <summary>그리드 인덱스 → 월드좌표 변환.</summary>
    public Vector3 GridToWorld(int _x, int _z)
    {
        return _origin + new Vector3(_x * _cellSize, 0f, _z * _cellSize);
    }

    /// <summary>
    /// 월드좌표 → GridNode 변환.
    /// RoundToInt로 가장 가까운 칸을 찾는다. 범위 밖이면 null 반환.
    /// </summary>
    public GridNode GetNode(Vector3 _worldPos)
    {
        int x = Mathf.RoundToInt((_worldPos.x - _origin.x) / _cellSize);
        int z = Mathf.RoundToInt((_worldPos.z - _origin.z) / _cellSize);

        if (!IsInBounds(x, z)) return null;
        return _grid[x, z];
    }

    /// <summary>그리드 인덱스로 직접 GridNode 반환. 범위 밖이면 null.</summary>
    public GridNode GetNode(int _x, int _z)
    {
        if (!IsInBounds(_x, _z)) return null;
        return _grid[_x, _z];
    }

    private bool IsInBounds(int _x, int _z)
    {
        return _x >= 0 && _x < _width && _z >= 0 && _z < _height;
    }

    /// <summary>
    /// 타워 설치/철거 시 호출.
    /// IsOccupied 업데이트 → 마커 토글 → PathFinder 경로 재계산 트리거.
    /// </summary>
    public void SetOccupied(Vector3 _worldPos, bool _occupied)
    {
        GridNode node = GetNode(_worldPos);
        if (node == null) return;
        node.IsOccupied = _occupied;

        node.Marker?.SetActive(!_occupied);

        Managers.Path.RecalculatePath();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_grid == null) return;

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                GridNode node = _grid[x, z];
                Vector3 pos = node.WorldPosition + Vector3.up * .5f;
                Vector3 size = new Vector3(_cellSize * 0.85f, 0.05f, _cellSize * 0.85f);

                Color faceColor;
                Color edgeColor;

                switch (node.NodeType)
                {
                    case NodeType.Road:
                        faceColor = new Color(0f, 1f, 0f, 0.6f);
                        edgeColor = new Color(0f, 0.9f, 0f, 1f);
                        break;
                    case NodeType.Placeable:
                        faceColor = new Color(0f, 0.4f, 1f, 0.6f);
                        edgeColor = new Color(0f, 0.5f, 1f, 1f);
                        break;
                    default:
                        faceColor = new Color(1f, 0f, 0f, 0.5f);
                        edgeColor = new Color(0.9f, 0f, 0f, 1f);
                        break;
                }

                Gizmos.color = faceColor;
                Gizmos.DrawCube(pos, size);
                Gizmos.color = edgeColor;
                Gizmos.DrawWireCube(pos, size);
            }
        }
    }
#endif
}
