using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class AStarPathfinding {
                
    // マンハッタン距離の最大値
    private int maxDistance = 20;

    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos, List<Vector2Int> monsterView) {
        // 視界外の場合は即座にnullを返す
        if (!monsterView.Contains(targetPos)) {
            Debug.Log("targetが視野外です。");
            return null;
        }

        // マンハッタン距離で目標が遠すぎる場合は即座にnullを返す
        int manhattanDistance = Mathf.Abs(startPos.x - targetPos.x) + Mathf.Abs(startPos.y - targetPos.y);
        if (manhattanDistance > maxDistance) { // 適切な距離制限を設定
            Debug.Log("マンハッタン距離が遠すぎます。");
            return null;
        }

        bool isTargetPlayer = CharacterManager.i.GetObjectTypeByPosition(targetPos) == "Player";
        Node startNode = new Node(startPos);
        Node targetNode = new Node(targetPos);

        // HashSetを使用してパフォーマンスを向上
        HashSet<Vector2Int> openSet = new HashSet<Vector2Int>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        List<Node> openList = new List<Node>();
        
        // startNodeを初期ノードとして追加
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        openList.Add(startNode);
        openSet.Add(startPos);

        int loopCount = 0;
        const int maxLoopCount = 100; // ループ回数を制限
        const int maxOpenListSize = 50; // openListのサイズを制限

        while (openList.Count > 0) {
            if (++loopCount > maxLoopCount) {
                Debug.Log("ループ回数が多すぎます。");
                return null;
            }

            // 最小コストのノードを効率的に取得
            int currentIndex = 0;
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++) {
                if (openList[i].fCost < currentNode.fCost || 
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost)) {
                    currentNode = openList[i];
                    currentIndex = i;
                }
            }

            // リストの操作を最適化
            openList[currentIndex] = openList[openList.Count - 1];
            openList.RemoveAt(openList.Count - 1);
            openSet.Remove(currentNode.position);
            closedSet.Add(currentNode.position);

            if (currentNode.position == targetPos) {
                return RetracePath(startNode, currentNode);
            }

            // 隣接ノードの処理を最適化
            foreach (var direction in DungeonConstants.EightDirections) {
                Vector2Int neighbourPos = currentNode.position + DungeonConstants.ToVector2Int[direction];
                
                // キャラクターの存在チェックを追加
                bool isTargetPosition = neighbourPos == targetPos;
                if (!monsterView.Contains(neighbourPos) || 
                    closedSet.Contains(neighbourPos) || 
                    (!isTargetPosition && !TileManager.i.CheckTileStandable(neighbourPos)) ||  // キャラクターがいる場合は迂回
                    (!isTargetPlayer && isTargetPosition && !TileManager.i.CheckTileStandable(neighbourPos))) {
                    continue;
                }

                bool isDiagonal = direction == DungeonConstants.UpRight || 
                                direction == DungeonConstants.UpLeft || 
                                direction == DungeonConstants.DownRight || 
                                direction == DungeonConstants.DownLeft;
                int newCost = currentNode.gCost + (isDiagonal ? 14 : 10);

                Node neighbour = new Node(neighbourPos);
                if (!openSet.Contains(neighbourPos)) {
                    if (openList.Count >= maxOpenListSize) {
                        Debug.Log("openListのサイズが多すぎます。");
                        return null;
                    }
                    neighbour.gCost = newCost;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    openList.Add(neighbour);
                    openSet.Add(neighbourPos);
                }
                else if (newCost < neighbour.gCost) {
                    neighbour.gCost = newCost;
                    neighbour.parent = currentNode;
                }
            }
        }
        Debug.Log($"経路が見つかりませんでした。Start: {startPos}, Target: {targetPos}");
        return null;
    }

    private List<Vector2Int> RetracePath(Node startNode, Node endNode) {
        var path = new List<Vector2Int>(maxDistance); // キャパシティを予め確保
        var currentNode = endNode;

        while (currentNode != startNode) {
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

        // 斜め移動を優先するためのコスト計算
        // 斜め移動のコストを14（√2 * 10）、直線移動のコストを10とする
        int diagonalSteps = Mathf.Min(dstX, dstY);  // 斜め移動できる回数
        int straightSteps = Mathf.Abs(dstX - dstY); // 残りの直線移動

        return (diagonalSteps * 14) + (straightSteps * 10);
    }

    private bool IsDiagonalMove(Vector2Int from, Vector2Int to) {
        return Mathf.Abs(from.x - to.x) == 1 && Mathf.Abs(from.y - to.y) == 1;
    }

    private int GetMoveCost(Node currentNode, Node neighbour) {
        // 斜め移動の場合は14、直線移動の場合は10のコストを返す
        return IsDiagonalMove(currentNode.position, neighbour.position) ? 14 : 10;
    }

    private Node GetAlternativeTarget(Node targetNode, Vector2Int startPos, List<Vector2Int> monsterView) {
        List<Node> neighbours = GetNeighbours(targetNode, monsterView);
        Node closestNode = null;
        int closestDistance = int.MaxValue;

        foreach (Node neighbour in neighbours) {
            if (TileManager.i.CheckMovableTile(startPos,neighbour.position) && 
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

    private Vector2Int? FindNearestAccessibleTile(Vector2Int targetPos, List<Vector2Int> monsterView) {
        // 目標位置の周囲8マスをチェック
        foreach (var direction in DungeonConstants.EightDirections) {
            Vector2Int checkPos = targetPos + DungeonConstants.ToVector2Int[direction];
            if (TileManager.i.CheckTileStandable(checkPos) && monsterView.Contains(checkPos)) {
                return checkPos;
            }
        }
        return null;
    }
}
