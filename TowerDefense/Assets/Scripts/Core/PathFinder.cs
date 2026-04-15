using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* 경로탐색. MonoBehaviour 아님 — Managers.Path로 접근.
/// Road 노드만 통과하며 CanWalk(Walkable &amp;&amp; !IsOccupied)가 false인 칸은 우회.
/// 타워 설치로 길이 막히면 RecalculatePath()로 이벤트 발행 →
/// 모든 EnemyController가 수신해 현재 위치에서 재탐색.
/// </summary>
public class PathFinder
{
    /// <summary>
    /// 길이 바뀔 때 발행. 구독한 EnemyController가 각자 경로를 재계산한다.
    /// GridSystem.SetOccupied()에서 호출.
    /// </summary>
    public event Action OnPathChanged;

    /// <summary>
    /// 외부에서 경로 변경을 알릴 때 호출. OnPathChanged 이벤트를 발행한다.
    /// </summary>
    public void RecalculatePath() => OnPathChanged?.Invoke();

    // 상하좌우 4방향 (대각선 없음)
    private static readonly (int x, int z)[] _directions =
    {
        ( 0,  1),
        ( 0, -1),
        (-1,  0),
        ( 1,  0),
    };

    /// <summary>
    /// A*로 startPos에서 endPos까지의 경로를 계산해 월드좌표 리스트로 반환.
    /// startPos 또는 endPos가 Road 타일이 아니면 null 반환.
    /// 경로가 없으면 null 반환 (타워로 완전히 막힌 경우).
    /// </summary>
    /// <param name="_startPos">출발 월드좌표 (Road 타일 위여야 함)</param>
    /// <param name="_endPos">도착 월드좌표 (Road 타일 위여야 함)</param>
    public List<Vector3> FindPath(Vector3 _startPos, Vector3 _endPos)
    {
        GridSystem grid = Managers.Grid;
        if (grid == null) return null;

        GridNode startNode = grid.GetNode(_startPos);
        GridNode endNode   = grid.GetNode(_endPos);
        if (startNode == null || endNode == null) return null;

        // Open: 탐색할 후보 노드 / Closed: 이미 확인 완료된 노드
        List<GridNode>     open    = new List<GridNode>();
        HashSet<GridNode>  closed  = new HashSet<GridNode>();
        List<GridNode>     visited = new List<GridNode>();

        startNode.G      = 0;
        startNode.H      = GetHeuristic(startNode, endNode);
        startNode.Parent = null;
        open.Add(startNode);
        visited.Add(startNode);

        while (open.Count > 0)
        {
            GridNode current = GetLowestF(open);

            if (current == endNode)
            {
                List<Vector3> path = TracePath(endNode);
                ResetNodes(visited);
                return path;
            }

            open.Remove(current);
            closed.Add(current);

            foreach (var (dx, dz) in _directions)
            {
                GridNode neighbor = grid.GetNode(current.GridX + dx, current.GridZ + dz);

                if (neighbor == null || !neighbor.CanWalk || closed.Contains(neighbor))
                    continue;

                int newG = current.G + 1;

                if (!open.Contains(neighbor))
                {
                    neighbor.G      = newG;
                    neighbor.H      = GetHeuristic(neighbor, endNode);
                    neighbor.Parent = current;
                    open.Add(neighbor);
                    visited.Add(neighbor);
                }
                else if (newG < neighbor.G)
                {
                    neighbor.G      = newG;
                    neighbor.Parent = current;
                }
            }
        }

        ResetNodes(visited);
        return null;
    }

    /// <summary>맨해튼 거리 — 4방향이므로 대각선 없음.</summary>
    private int GetHeuristic(GridNode _a, GridNode _b)
        => Mathf.Abs(_a.GridX - _b.GridX) + Mathf.Abs(_a.GridZ - _b.GridZ);

    /// <summary>Open 리스트에서 F값이 가장 낮은 노드를 반환.</summary>
    private GridNode GetLowestF(List<GridNode> _open)
    {
        GridNode lowest = _open[0];
        for (int i = 1; i < _open.Count; i++)
        {
            if (_open[i].F < lowest.F)
                lowest = _open[i];
        }
        return lowest;
    }

    /// <summary>탐색 후 G·H·Parent를 초기화해 다음 FindPath 호출을 오염시키지 않는다.</summary>
    private void ResetNodes(List<GridNode> _visited)
    {
        foreach (GridNode node in _visited)
        {
            node.G      = 0;
            node.H      = 0;
            node.Parent = null;
        }
    }

    /// <summary>endNode에서 Parent를 따라 역추적한 뒤 순서를 뒤집어 시작→끝 순서로 반환.</summary>
    private List<Vector3> TracePath(GridNode _endNode)
    {
        List<Vector3> path    = new List<Vector3>();
        GridNode      current = _endNode;

        while (current != null)
        {
            path.Add(current.WorldPosition);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }
}
