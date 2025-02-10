using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class AStarPathfinding {
                

    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos, List<Vector2Int> monsterView) {                        
        
        // スタート位置から目標位置までの直線距離
        int directDistance = Mathf.Abs(startPos.x - targetPos.x) + Mathf.Abs(startPos.y - targetPos.y);        

        Node startNode = new Node(startPos);
        Node targetNode = new Node(targetPos);

        // 経路上の各タイルが視界内にあるかチェック
        for (int x = Mathf.Min(startPos.x, targetPos.x); x <= Mathf.Max(startPos.x, targetPos.x); x++) {
            for (int y = Mathf.Min(startPos.y, targetPos.y); y <= Mathf.Max(startPos.y, targetPos.y); y++) {
                Vector2Int pos = new Vector2Int(x, y);
                if (!monsterView.Contains(pos)) {
                    Debug.Log($"視界外のタイル検出: {pos}");
                }
            }
        }

        if (!TileManager.i.CheckWalkableTile(startPos, targetPos)) {
            Debug.Log("Target position is not standable. Finding alternative target.");
            targetNode = GetAlternativeTarget(targetNode, startPos, monsterView); // プレイヤー位置を追加
            if (targetNode == null) {
                Debug.Log("No alternative target found.");
                return null;
            }
        }

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        openList.Add(startNode);

        int loopCount = 0;
        int maxLoopCount = 1000; // 最大ループ回数
        int maxOpenListSize = 100; // openListの最大サイズ

        while (openList.Count > 0) {
            loopCount++;
            if (loopCount >= maxLoopCount) {
                Debug.Log($"ループ回数が上限({maxLoopCount})に達しました。経路探索を中止します。Start: {startPos}, Target: {targetPos}");
                return null;
            }

            // 最小コストのノードを見つける
            Node currentNode = openList.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.position == targetNode.position) {
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbour in GetNeighbours(currentNode, monsterView)) {
                // 移動不可能、既に探索済み、または視界外のノードはスキップ
                if (!TileManager.i.CheckWalkableTile(currentNode.position, neighbour.position) || 
                    closedList.Any(n => n.position == neighbour.position) || 
                    !monsterView.Contains(neighbour.position)) {
                    continue;
                }

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                var existingNode = openList.FirstOrDefault(n => n.position == neighbour.position);

                if (existingNode != null && newCostToNeighbour >= existingNode.gCost) {
                    continue;
                }

                neighbour.gCost = newCostToNeighbour;
                neighbour.hCost = GetDistance(neighbour, targetNode);
                neighbour.parent = currentNode;

                if (existingNode == null) {
                    if (openList.Count >= maxOpenListSize) {
                        Debug.Log($"OpenListが上限({maxOpenListSize})に達しました。経路探索を中止します。");
                        return null;
                    }
                    openList.Add(neighbour);
                }
            }
        }

        Debug.Log($"経路が見つかりませんでした。Start: {startPos}, Target: {targetPos}");
        return null;
    }

    //ゴールから一つずつNodeのparentを辿っていく
    private List<Vector2Int> RetracePath(Node startNode, Node endNode) {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        int loopCount = 0;
        while (currentNode != startNode) {
            loopCount++;
            if (loopCount >= 100) { //無限ループ回避                
                Debug.Log("無限ループ2");
                break;
            }

            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    private List<Node> GetNeighbours(Node node, List<Vector2Int> monsterView) {
        List<Node> neighbours = new List<Node>();
        foreach (var direction in DungeonConstants.EightDirections) {
            Vector2Int neighbourPos = node.position + DungeonConstants.ToVector2Int[direction];
            
            if (monsterView.Contains(neighbourPos)) {
                neighbours.Add(new Node(neighbourPos));
            }
        }        
        return neighbours;
    }

    private int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);
        return Mathf.Max(dstX, dstY);
    }

    private Node GetAlternativeTarget(Node targetNode, Vector2Int startPos, List<Vector2Int> monsterView) {
        List<Node> neighbours = GetNeighbours(targetNode, monsterView);
        Node closestNode = null;
        int closestDistance = int.MaxValue;

        foreach (Node neighbour in neighbours) {
            if (TileManager.i.CheckTileStandable(neighbour.position) && 
                monsterView.Contains(neighbour.position)) {
                int distance = GetDistance(neighbour, new Node(startPos));
                if (distance < closestDistance) {
                    closestNode = neighbour;
                    closestDistance = distance;
                }
            }
        }

        return closestNode;
    }
}
