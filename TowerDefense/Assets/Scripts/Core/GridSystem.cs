using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("그리드 크기")]
    [SerializeField] private int _width = 32;       // 가로 칸수
    [SerializeField] private int _height = 11;      // 세로 칸수
    [SerializeField] private float _cellSize = 2f;  // 칸 크기

    [Header("맵 기준점 (좌측 하단)")]
    [SerializeField] private Vector3 _origin = new Vector3(-36.5f, 0f, -5.62f);

    [Header("레이어")]
    [SerializeField] private LayerMask _roadLayer;
    [SerializeField] private LayerMask _placeableLayer;

    private GridNode[,] _grid;


    void OnEnable()
    {
        InitGrid();
    }


    /// <summary>
    /// 그리드 초기화 각 그라운드의 위치가 2차이남
    /// 기준점(맵의 좌측 하단) 기준으로 월드좌표로 변환해서 배열에저장
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

                // 타일이 감지되면 실제 타일 Y값 사용, 아니면 origin Y 유지
                if (nodeType != NodeType.None)
                    worldPos.y = hitY;

                _grid[x, z] = new GridNode(x, z, worldPos, nodeType);
            }
        }
    }

    private (NodeType, float) GetNodeInfo(Vector3 _worldPos)
    {
        Vector3 rayOrigin = _worldPos + Vector3.up * 20f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit roadHit, 30f, _roadLayer))
            return (NodeType.Road, roadHit.point.y);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit placeHit, 30f, _placeableLayer))
            return (NodeType.Placeable, placeHit.point.y);

        return (NodeType.None, _worldPos.y);
    }

    // 그리드 좌표 → 월드 좌표
    public Vector3 GridToWorld(int _x, int _z)
    {
        return _origin + new Vector3(_x * _cellSize, 0f, _z * _cellSize);
    }

    // 월드 좌표 → 그리드 노드
    public GridNode GetNode(Vector3 _worldPos)
    {
        int x = Mathf.RoundToInt((_worldPos.x - _origin.x) / _cellSize);
        int z = Mathf.RoundToInt((_worldPos.z - _origin.z) / _cellSize);

        if (!IsInBounds(x, z)) return null;
        return _grid[x, z];
    }

    public GridNode GetNode(int _x, int _z)
    {
        if (!IsInBounds(_x, _z)) return null;
        return _grid[_x, _z];
    }

    private bool IsInBounds(int _x, int _z)
    {
        return _x >= 0 && _x < _width && _z >= 0 && _z < _height;
    }

    // 상자/타워 설치 시 호출
    public void SetOccupied(Vector3 _worldPos, bool _occupied)
    {
        GridNode node = GetNode(_worldPos);
        if (node == null) return;
        node.IsOccupied = _occupied;
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
