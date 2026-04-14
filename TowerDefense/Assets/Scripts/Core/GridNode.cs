public enum NodeType { None, Road, Placeable }

public class GridNode
{
    // 그리드 좌표 (몇 번째 칸인지)
    public int GridX { get; }
    public int GridZ { get; }

    // 월드 좌표 (유니티 씬에서의 실제 위치)
    public UnityEngine.Vector3 WorldPosition { get; }

    // 칸 타입 (Road / Placeable / None)
    public NodeType NodeType { get; set; }

    // 바닥이 walkable한지
    public bool Walkable { get; set; }

    // 타워나 상자가 올라가 있는지
    public bool IsOccupied { get; set; }

    // 실제 이동 가능 여부 (바닥이 walkable이고 위에 아무것도 없어야 함)
    public bool CanWalk => Walkable && !IsOccupied;

    // A* 비용 계산용
    public int G { get; set; }  // 시작점 → 현재 칸까지 실제 비용
    public int H { get; set; }  // 현재 칸 → 목적지까지 예상 비용
    public int F => G + H;      // 총 비용 (낮을수록 우선 탐색)

    // 어느 칸에서 왔는지 (경로 역추적용)
    public GridNode Parent { get; set; }

    public GridNode(int _gridX, int _gridZ, UnityEngine.Vector3 _worldPosition, NodeType _nodeType)
    {
        GridX         = _gridX;
        GridZ         = _gridZ;
        WorldPosition = _worldPosition;
        NodeType      = _nodeType;
        Walkable      = _nodeType != NodeType.None;
        IsOccupied    = false;
    }
}
