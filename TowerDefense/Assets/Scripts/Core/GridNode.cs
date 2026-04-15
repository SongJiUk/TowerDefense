public enum NodeType { None, Road, Placeable }

/// <summary>
/// 그리드 칸 하나의 데이터 클래스 (MonoBehaviour 아님).
/// GridSystem이 초기화 시 생성하고, PathFinder·TowerPlacer가 읽는다.
/// </summary>
public class GridNode
{
    /// <summary>배열 인덱스 (가로)</summary>
    public int GridX { get; }

    /// <summary>배열 인덱스 (세로)</summary>
    public int GridZ { get; }

    /// <summary>유니티 씬에서의 실제 위치</summary>
    public UnityEngine.Vector3 WorldPosition { get; }

    /// <summary>타일 종류 — Road(길) / Placeable(설치가능) / None</summary>
    public NodeType NodeType { get; set; }

    /// <summary>A*가 이 칸을 지나갈 수 있는지 (Road면 true)</summary>
    public bool Walkable { get; set; }

    /// <summary>타워나 상자가 올라가 있는지. SetOccupied()로 변경.</summary>
    public bool IsOccupied { get; set; }

    /// <summary>실제 이동 가능 여부. Walkable이고 비어있어야 true.</summary>
    public bool CanWalk => Walkable && !IsOccupied;

    /// <summary>
    /// Placeable 칸 위에 스폰된 마커 큐브.
    /// GridSystem.SetOccupied()에서 타워 설치 시 숨김, 철거 시 복원.
    /// </summary>
    public UnityEngine.GameObject Marker { get; set; }

    // ─── A* 계산용 ────────────────────────────────────────────────────────────

    /// <summary>시작점 → 현재 칸까지 실제 이동 비용</summary>
    public int G { get; set; }

    /// <summary>현재 칸 → 목적지까지 예상 비용 (맨해튼 거리)</summary>
    public int H { get; set; }

    /// <summary>총 비용. 낮을수록 A*에서 먼저 탐색.</summary>
    public int F => G + H;

    /// <summary>경로 역추적용. 어느 칸에서 이 칸으로 왔는지.</summary>
    public GridNode Parent { get; set; }

    /// <summary>
    /// GridSystem.InitGrid()에서 호출. NodeType에 따라 Walkable 자동 설정.
    /// </summary>
    public GridNode(int _gridX, int _gridZ, UnityEngine.Vector3 _worldPosition, NodeType _nodeType)
    {
        GridX         = _gridX;
        GridZ         = _gridZ;
        WorldPosition = _worldPosition;
        NodeType      = _nodeType;
        Walkable      = _nodeType == NodeType.Road;
        IsOccupied    = false;
    }
}
